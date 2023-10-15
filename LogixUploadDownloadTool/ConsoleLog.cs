using ShellProgressBar;
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
        private static ProgressBar? _progressBar;

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
            Console.WriteLine($"[{DateTime.Now:dd-MMM-yyyy HH:mm:ss.fff}] {message}");
            Console.ForegroundColor = currentColor;
        }

        /// <summary>
        /// Starts a progress bar.
        /// </summary>
        /// <param name="initialText">Initial text</param>
        public static void StartProgressBar(string initialText)
        {
            if (_progressBar != null)
            {
                LogError("Attempt to start a new progress bar with an existing one running.");
                return;
            }

            if (Console.IsOutputRedirected)
                return;         //We won't do this for redirected console

            Console.WriteLine();
            _progressBar = new(100, initialText, ConsoleColor.Green);
        }

        /// <summary>
        /// Updates the progress bar.
        /// </summary>
        /// <param name="progress">Progress (0-100)</param>
        public static void UpdateProgress(int progress)
        {
            _progressBar?.Tick(progress);
        }

        /// <summary>
        /// Updates the progress bar text.
        /// </summary>
        /// <param name="text">Text</param>
        public static void UpdateProgressText(string text)
        {
            _progressBar?.Tick(_progressBar.CurrentTick, text);
        }

        /// <summary>
        /// Stops the progress bar.
        /// </summary>
        public static void StopProgressBar()
        {
            _progressBar?.Dispose();
            _progressBar = null;
            Console.WriteLine();
        }
    }
}
