using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixUploadDownloadTool
{
    internal class SharedOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Sets the program to verbose output.")]
        public bool Verbose { get; set; } = false;

        [Option("user", Required = false, HelpText = "Specifies the user for logging into FactoryTalk directory.")]
        public string User { get; set; } = string.Empty;

        [Option("pass", Required = false, HelpText = "Specifies the password for the user.")]
        public string Password { get; set; } = string.Empty;
    }

    [Verb("upload", false, new[] { "up", "u" }, HelpText = "Uploads from the controller to the file.")]
    internal class UploadOptions : SharedOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Specifies the file to use.")]
        public string Filename { get; set; } = string.Empty;

        [Option('p', "path", Required = true, HelpText = "Overrides the path in the file.")]
        public string Path { get; set; } = string.Empty;

        [Option('t', "tags", HelpText = "Uploads the tag values with the project.")]
        public bool TagValues { get; set; } = false;
    }

    [Verb("download", true, new[] { "down", "d" }, HelpText = "Downloads from the file to the controller.")]
    internal class DownloadOptions : SharedOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Specifies the file to use.")]
        public string Filename { get; set; } = string.Empty;

        [Option('p', "path", Required = false, HelpText = "Overrides the path in the file.")]
        public string Path { get; set; } = string.Empty;

        [Option("prog-mode", HelpText = "If set, leaves the controller in program mode after download.")]
        public bool ProgramMode { get; set; } = false;

        [Option("forces-on", HelpText = "If set, enables forces in the controller (default disabled).")]
        public bool ForcesOn { get; set; } = false;
    }

    [Verb("info", false, new[] { "i" }, HelpText = "Provides info about a project.")]
    internal class InfoOptions : SharedOptions
    {
        [Option('f', "filename", Required = false, HelpText = "Specifies the file to use.")]
        public string Filename { get; set; } = string.Empty;
    }
}
