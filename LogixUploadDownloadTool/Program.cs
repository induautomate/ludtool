using CommandLine;
using RockwellSoftware.RSLogix5000ServicesDotNet;
using RSLogix5000RevisionDirectoryLib;
using RSLogix5000ServicesLib;
using System;
using System.ComponentModel.Design;
using System.Text;

namespace LogixUploadDownloadTool
{
    internal class Program
    {
        static int Main(string[] args) =>
            Parser.Default.ParseArguments<DownloadOptions, UploadOptions, InfoOptions>(args)
                .MapResult(
                    (DownloadOptions opts) => (int)Download(opts),
                    (UploadOptions opts) => (int)Upload(opts),
                    (InfoOptions opts) => (int)Info(opts),
                    errors => (int)RuntimeErrorCodes.InvalidArguments);

        /// <summary>
        /// Uploads the file from the controller.
        /// </summary>
        /// <param name="options">Command line options</param>
        /// <returns>Runtime Code</returns>
        static RuntimeErrorCodes Upload(UploadOptions options)
        {
            ConsoleLog.EnableVerbose = options.Verbose;

            if (File.Exists(options.Filename))
                return UploadIntoExistingFile(options);
            else
                return UploadIntoNewFile(options);
        }

        /// <summary>
        /// Uploads the controller into a new file.
        /// </summary>
        /// <param name="options">Command line options</param>
        /// <returns>Runtime Code</returns>
        static RuntimeErrorCodes UploadIntoExistingFile(UploadOptions options)
        {
            LogixServicesLifetimeManager? svcManager = null;
            RSLogix5000ServicesLib.Controller? controller = null;

            if (!File.Exists(options.Filename))
            {
                ConsoleLog.LogError($"The file {options.Filename} could not be found.");
                return RuntimeErrorCodes.FileNotFound;
            }

            try
            {
                ConsoleLog.Verbose("Getting revision information...");
                Revision revision = LogixUtilities.GetRevisionFromProjectFile(options.Filename);
                ConsoleLog.Verbose($"Revision info found, Version {revision.MajorSoftwareVersion}.{revision.MinorSoftwareVersion}");

                ConsoleLog.Verbose("Checking if revision is installed...");
                if (!LogixUtilities.RequiredVersionIsInstalled(options.Filename))
                {
                    ConsoleLog.LogError($"The required Logix version is not installed, requires {revision.MajorSoftwareVersion}.{revision.MinorSoftwareVersion}");
                    return RuntimeErrorCodes.RequiredVersionNotInstalled;
                }
                ConsoleLog.Verbose("Version is installed!");

                ConsoleLog.Log("Please wait, instantiating Logix services (this can take a while)...");
                svcManager = LogixUtilities.CreateLogixServices(revision);

                ConsoleLog.Log("Opening ACD file...");

                controller = (RSLogix5000ServicesLib.Controller)svcManager.Services.OpenCopyOf(options.Filename);
            }
            catch (Exception ex)
            {
                ConsoleLog.LogError($"Exception occurred while instantiating services - {ex.Message}\n{ex.StackTrace}");
                svcManager?.Release();
                return RuntimeErrorCodes.UnknownError;
            }

            try
            {
                ConsoleLog.Verbose("Connecting to controller...");
                controller.CurrentCommPath = options.Path;
                controller.GoOnline();

                ConsoleLog.Verbose("Checking correlation status...");
                if (controller.CanCorrelate(out bool needCorrlateLog, out bool canMerge, out lgxResultCodes status))
                {
                    ConsoleLog.Log("Processor returned correlation OK.");
                }
                else
                {
                    ConsoleLog.LogError($"Processor returned correlation failed, result code {status}");
                    return RuntimeErrorCodes.CanNotCorrelate;
                }

                controller.CommLost += () => { ConsoleLog.LogWarning("Communications lost..."); };
                controller.ConnectedStateChange += (e) => { ConsoleLog.LogWarning($"Connected state change: {e}"); };
                controller.ForceEnableStateChange += (e) => { ConsoleLog.LogWarning($"Force enabled stage change: {(e ? "Enabled" : "Disabled")}"); };
                controller.KeySwitchPositionChange += (e) => { ConsoleLog.LogWarning($"Keyswitch Position changed: {e}"); };
                controller.MajorFaultStateChange += (e) => { ConsoleLog.LogWarning($"Major fault status changed: {(e ? "Faults" : "No Faults")}"); };
                controller.ModeChange += (e) => { ConsoleLog.LogWarning($"Mode changed: {e}"); };
                controller.OnlineImageCorrelationLoss += () => { ConsoleLog.LogWarning("Online image correlation lost."); };
                controller.ProgessChanged += (e) =>
                {
                    ConsoleLog.UpdateProgress(e);
                };

                controller.StatusChanged += (e) => { ConsoleLog.UpdateProgressText(e); };

                ConsoleLog.StartProgressBar("Starting upload...");

                controller.Upload();

                if (options.TagValues)
                {
                    controller.UploadTagData();
                }

                ConsoleLog.StopProgressBar();

                ConsoleLog.Log("Saving project...");
                controller.Save();

                ConsoleLog.Log("Complete!");
            }
            catch (Exception ex)
            {
                ConsoleLog.StopProgressBar();
                ConsoleLog.LogError($"Exception occurred while uploading - {ex.Message}\n{ex.StackTrace}");
                svcManager?.Release();
                return RuntimeErrorCodes.UnknownError;
            }
            finally
            {
                controller?.ForceClose();
                ConsoleLog.StopProgressBar();
                controller?.ForceClose();
                svcManager?.Release();
            }     

            return RuntimeErrorCodes.NoError;
        }

        /// <summary>
        /// Uploads the controller into an existing file.
        /// </summary>
        /// <param name="options">Command line options</param>
        /// <returns>Runtime code</returns>
        static RuntimeErrorCodes UploadIntoNewFile(UploadOptions options)
        {
            LogixServicesLifetimeManager? svcManager = null;
            RSLogix5000ServicesLib.Controller? controller = null;

            try
            {
                ConsoleLog.Verbose("Getting revision information...");
                RevisionDirectoryClass revisionDirectory = new();
                revisionDirectory.Initialize();
                ConsoleLog.Verbose($"The latest available revision is {revisionDirectory.LatestAvailableRevision.MajorSoftwareVersion}:{revisionDirectory.LatestAvailableRevision.MinorSoftwareVersion:00}");


                ConsoleLog.Log("Please wait, instantiating Logix services (this can take a while)...");
                svcManager = LogixUtilities.CreateLogixServices(revisionDirectory.LatestAvailableRevision);
                ConsoleLog.Verbose("Services created...");
            }
            catch (Exception ex)
            {
                ConsoleLog.LogError($"Exception occurred while instantiating services - {ex.Message}\n{ex.StackTrace}");
                svcManager?.Release();
                return RuntimeErrorCodes.UnknownError;
            }

            try
            {
                ConsoleLog.Log("Obtaining the processor type from the online controller...");
                lgxProcessorTypes processor = svcManager.Services.GetProjectTypeFromController(options.Path);
                ConsoleLog.Verbose($"Read {processor} from the online controller...");

                ConsoleLog.Log("Creating the ACD file...");
                controller = (RSLogix5000ServicesLib.Controller)svcManager.Services.Create(options.Filename, processor);

                controller.CommLost += () => { ConsoleLog.LogWarning("Communications lost..."); };
                controller.ConnectedStateChange += (e) => { ConsoleLog.LogWarning($"Connected state change: {e}"); };
                controller.ForceEnableStateChange += (e) => { ConsoleLog.LogWarning($"Force enabled stage change: {(e ? "Enabled" : "Disabled")}"); };
                controller.KeySwitchPositionChange += (e) => { ConsoleLog.LogWarning($"Keyswitch Position changed: {e}"); };
                controller.MajorFaultStateChange += (e) => { ConsoleLog.LogWarning($"Major fault status changed: {(e ? "Faults" : "No Faults")}"); };
                controller.ModeChange += (e) => { ConsoleLog.LogWarning($"Mode changed: {e}"); };
                controller.OnlineImageCorrelationLoss += () => { ConsoleLog.LogWarning("Online image correlation lost."); };
                controller.ProgessChanged += (e) =>
                {
                    ConsoleLog.UpdateProgress(e);
                };

                controller.StatusChanged += (e) => { ConsoleLog.UpdateProgressText(e); };

                ConsoleLog.StartProgressBar("Starting upload...");

                controller.Upload();

                if (options.TagValues)
                    controller.UploadTagData();

                ConsoleLog.StopProgressBar();

                ConsoleLog.Log("Saving file changes...");
                controller.Save();
                controller.ForceClose();
                ConsoleLog.Log("Complete!");
            }
            catch (Exception ex)
            {
                ConsoleLog.StopProgressBar();
                ConsoleLog.LogError($"Exception occurred while uploading - {ex.Message}\n{ex.StackTrace}");
                return RuntimeErrorCodes.UnknownError;
            }
            finally
            {
                ConsoleLog.StopProgressBar();
                svcManager?.Release();
            }

            return RuntimeErrorCodes.NoError;
        }

        /// <summary>
        /// Downloads to the controller.
        /// </summary>
        /// <param name="options">Command line options</param>
        /// <returns>Runtime code</returns>
        static RuntimeErrorCodes Download(DownloadOptions options)
        {
            ConsoleLog.EnableVerbose = options.Verbose;

            LogixServicesLifetimeManager? svcManager = null;

            if (!File.Exists(options.Filename))
            {
                ConsoleLog.LogError($"The file {options.Filename} could not be found.");
                return RuntimeErrorCodes.FileNotFound;
            }

            try
            {
                ConsoleLog.Verbose("Getting revision information...");
                Revision revision = LogixUtilities.GetRevisionFromProjectFile(options.Filename);
                ConsoleLog.Verbose($"Revision info found, Version {revision.MajorSoftwareVersion}.{revision.MinorSoftwareVersion}");

                ConsoleLog.Verbose("Checking if revision is installed...");
                if (!LogixUtilities.RequiredVersionIsInstalled(options.Filename))
                {
                    ConsoleLog.LogError($"The required Logix version is not installed, requires {revision.MajorSoftwareVersion}.{revision.MinorSoftwareVersion}");
                    return RuntimeErrorCodes.RequiredVersionNotInstalled;
                }
                ConsoleLog.Verbose("Version is installed!");                

                ConsoleLog.Log("Please wait, instantiating Logix services (this can take a while)...");
                svcManager = LogixUtilities.CreateLogixServices(revision);
                
                ConsoleLog.Verbose("Logix services instantiated, opening file...");

                var controller = (RSLogix5000ServicesLib.Controller)svcManager.Services.OpenCopyOf(options.Filename);
                                    
                PrintControllerInfo(controller);

                controller.AutoFlushEvents = true;

                lgxControllerModes mode = lgxControllerModes.lgxMode_Run;

                if (options.ProgramMode)
                    mode = lgxControllerModes.lgxMode_Program;

                int progress = 0;

                controller.CommLost += () => { ConsoleLog.LogWarning("Communications lost..."); };
                controller.ConnectedStateChange += (e) => { ConsoleLog.LogWarning($"Connected state change: {e}"); };
                controller.ForceEnableStateChange += (e) => { ConsoleLog.LogWarning($"Force enabled stage change: {(e ? "Enabled" : "Disabled")}"); };
                controller.KeySwitchPositionChange += (e) => { ConsoleLog.LogWarning($"Keyswitch Position changed: {e}"); };
                controller.MajorFaultStateChange += (e) => { ConsoleLog.LogWarning($"Major fault status changed: {(e ? "Faults" : "No Faults")}"); };
                controller.ModeChange += (e) => { ConsoleLog.LogWarning($"Mode changed: {e}"); };
                controller.OnlineImageCorrelationLoss += () => { ConsoleLog.LogWarning("Online image correlation lost."); };
                controller.ProgessChanged += (e) =>
                {
                    ConsoleLog.UpdateProgress(e);
                };

                controller.StatusChanged += (e) => { ConsoleLog.UpdateProgressText(e); };

                if (!string.IsNullOrEmpty(options.Path))
                {
                    ConsoleLog.Log($"Using path {options.Path} instead of project stored {controller.ProjectCommPath}");
                    controller.CurrentCommPath = options.Path;
                }

                ConsoleLog.Log($"Downloading to {controller.CurrentCommPath}");

                ConsoleLog.Verbose("Connectiong to controller...");
                controller.GoConnected();

                ConsoleLog.Verbose("Setting the controller mode to program...");
                controller.Mode = lgxControllerModes.lgxMode_Program;

                ConsoleLog.StartProgressBar("Downloading...");

                controller.Download(options.ForcesOn, false, mode, true);

                ConsoleLog.StopProgressBar();

                ConsoleLog.Log("Complete!");
            }
            catch (RequiredVersionNotInstalledException rvni)
            {
                ConsoleLog.LogError(rvni.Message);
                return RuntimeErrorCodes.RequiredVersionNotInstalled;
            }
            catch (Exception e)
            {
                ConsoleLog.LogError($"Exception occurred in download - {e.Message}\n{e.StackTrace}");
                return RuntimeErrorCodes.UnknownError;
            }
            finally
            {
                ConsoleLog.StopProgressBar();
                ConsoleLog.Verbose("Shutting down Logix...");
                svcManager?.Release();
            }

            return RuntimeErrorCodes.NoError;
        }

        /// <summary>
        /// Gathers the controller information for the console.
        /// </summary>
        /// <param name="options">Command line options</param>
        /// <returns>Runtime code</returns>
        static RuntimeErrorCodes Info(InfoOptions options)
        {
            ConsoleLog.EnableVerbose = options.Verbose;

            LogixServicesLifetimeManager? svcManager = null;

            if (!File.Exists(options.Filename))
            {
                ConsoleLog.LogError($"The file {options.Filename} could not be found.");
                return RuntimeErrorCodes.FileNotFound;
            }

            try
            {
                ConsoleLog.Verbose("Getting revision information...");
                Revision revision = LogixUtilities.GetRevisionFromProjectFile(options.Filename);
                ConsoleLog.Verbose($"Revision info found, Version {revision.MajorSoftwareVersion}.{revision.MinorSoftwareVersion}");

                ConsoleLog.Verbose("Checking if revision is installed...");
                if (!LogixUtilities.RequiredVersionIsInstalled(options.Filename))
                {
                    ConsoleLog.LogError($"The required Logix version is not installed, requires {revision.MajorSoftwareVersion}.{revision.MinorSoftwareVersion}");
                    return RuntimeErrorCodes.RequiredVersionNotInstalled;
                }
                ConsoleLog.Verbose("Version is installed!");

                ConsoleLog.Log("Please wait, instantiating Logix services (this can take a while)...");
                svcManager = LogixUtilities.CreateLogixServices(revision);

                ConsoleLog.Verbose("Logix services instantiated, opening file...");

                var controller = (RSLogix5000ServicesLib.Controller)svcManager.Services.OpenCopyOf(options.Filename);

                PrintControllerInfo(controller);

            }
            catch (RequiredVersionNotInstalledException rvni)
            {
                ConsoleLog.LogError(rvni.Message);
                return RuntimeErrorCodes.RequiredVersionNotInstalled;
            }
            catch (Exception e)
            {
                ConsoleLog.LogError($"Exception occurred in info - {e.Message}\n{e.StackTrace}");
                return RuntimeErrorCodes.UnknownError;
            }
            finally
            {
                ConsoleLog.Verbose("Shutting down Logix...");
                svcManager?.Release();
            }

            return RuntimeErrorCodes.NoError;
        }

        /// <summary>
        /// Prints the controller information.
        /// </summary>
        /// <param name="controller">Controller</param>
        static void PrintControllerInfo(IController controller)
        {
            StringBuilder sb = new();
            sb.AppendLine("Controller Info:");
            sb.AppendLine($"Name: {controller.Name}");
            sb.AppendLine($"Description: {controller.Description}");
            sb.AppendLine($"Project Comm Path: {controller.ProjectCommPath}");
            sb.AppendLine($"Processor Type: {controller.ProcessorTypeString}");
            sb.AppendLine($"Pending Edits? {(controller.PendingEditsExist ? "Yes" : "No")}");
            controller.GetTotalMemoryBlocks(out uint standardBlocks, out uint safetyBlocks);
            controller.GetUsedMemoryBlocks(out uint standardUsed, out uint safetyUsed);
            sb.AppendLine($"Standard Memory (used/total): {standardUsed}/{standardBlocks}");
            sb.AppendLine($"Safety Memory (used/total): {safetyUsed}/{safetyBlocks}");
            
            ConsoleLog.Log(sb.ToString());
        }

    }
}