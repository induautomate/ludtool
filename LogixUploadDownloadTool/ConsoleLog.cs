using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixUploadDownloadTool
{
    /// <summary>
    /// Simplified class for logging to the console
    /// </summary>
    internal class ConsoleLog
    {

        /// <summary>
        /// Gets or sets the verbose output.
        /// </summary>
        public static bool EnableVerbose { get; set; } = false;

        /// <summary>
        /// Logs verbose output.
        /// </summary>
        /// <param name="message"></param>
        public static void Verbose(string message) { if (EnableVerbose) Log(ConsoleColor.DarkGray, message); }

        /// <summary>
        /// Logs a message to the console.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Log(string message) => Log(ConsoleColor.Green, message);

        /// <summary>
        /// Logs a warning to the console.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogWarning(string message) => Log(ConsoleColor.Yellow, $"Warning: {message}");

        /// <summary>
        /// Logs an error to the console.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogError(string message) => Log(ConsoleColor.Red, $"Error: {message}");

        /// <summary>
        /// Logs to the console in the specified format with the color and message.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="message"></param>
        private static void Log(ConsoleColor color, string message)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"{DateTime.Now:dd-MMM-yyyy HH:mm:ss.fff} {message}");
            Console.ForegroundColor = currentColor;
        }
    }
}
