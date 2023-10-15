using RockwellSoftware.RSLogix5000ServicesDotNet;
using RSLogix5000RevisionDirectoryLib;
using RSLogix5000ServicesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixUploadDownloadTool
{
    /// <summary>
    /// Logix Utilities
    /// </summary>
    internal static class LogixUtilities
    {
        /// <summary>
        /// Gets the ProgId for the specified revision.
        /// </summary>
        /// <param name="revision">Revision of Logix</param>
        /// <returns>ProgId</returns>
        public static string GetProgIdForRevision(Revision revision)
        {
            var progId = ((RevisionClass)revision).RSLogix5000ServicesProgId;
            
            return progId;
        }

        /// <summary>
        /// Verifies that the required software version is installed.
        /// </summary>
        /// <param name="projectFile">Project file</param>
        /// <returns>True if the version is installed, false if the version is not installed.</returns>
        public static bool RequiredVersionIsInstalled(string projectFile)
        {
            Revision rev = GetRevisionFromProjectFile(projectFile);

            if (rev.IsAvailable <= 0)
                return false;

            return true;
        }

        /// <summary>
        /// Gets the revision from the project file.
        /// </summary>
        /// <param name="projectFile">Project file</param>
        /// <returns>Revision</returns>
        public static Revision GetRevisionFromProjectFile(string projectFile)
        {
            RevisionDirectoryClass revisionDirectory = new RevisionDirectoryClass();
            revisionDirectory.Initialize();

            return revisionDirectory.GetAvailableRevisionFromProjectFile(projectFile);            
        }

        /// <summary>
        /// Gets the type from the ProgId
        /// </summary>
        /// <param name="progId">ProgId</param>
        /// <returns>Type</returns>
        /// <exception cref="RequiredVersionNotInstalledException">Thrown when the ProgId is not installed or registered.</exception>
        public static Type GetTypeFromProgID(string progId)
        {
            var logixType = Type.GetTypeFromProgID(progId);

            if (logixType == null)
            {
                throw new RequiredVersionNotInstalledException($"The required version is not installed.");
            }

            return logixType;
        }

        /// <summary>
        /// Creates the LogixServices for the specified revision.
        /// </summary>
        /// <param name="revision">Revision</param>
        /// <returns>LogixServices</returns>
        /// <exception cref="RequiredVersionNotInstalledException">Thrown when the required Logix version is not installed.</exception>
        public static LogixServicesLifetimeManager CreateLogixServices(Revision revision)
        {
            if (revision.IsAvailable <= 0)
                throw new RequiredVersionNotInstalledException($"The required version is not installed, need {revision.MajorSoftwareVersion}.{revision.MinorSoftwareVersion}");
            
            var services = new RockwellSoftware.RSLogix5000ServicesDotNet.LogixServices();
            services.InstantiateLogixServices(revision.AutomationInterfaceVersion, out object svc, out Type lgxType);
            services.logixServicesHandle = svc;
            services.logixServicesType = lgxType;

            ILogixServices lxc = (ILogixServices)svc;

            return new(lxc, services);
        }
    }
}
