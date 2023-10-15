using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixUploadDownloadTool
{
    /// <summary>
    /// Runtime error code.
    /// </summary>
    internal enum RuntimeErrorCodes
    {
        /// <summary>
        /// No error occurred.
        /// </summary>
        NoError = 0,
        /// <summary>
        /// Invalid arguments.
        /// </summary>
        InvalidArguments = 1,
        /// <summary>
        /// Unknown error.
        /// </summary>
        UnknownError = 2,
        /// <summary>
        /// Required version not installed.
        /// </summary>
        RequiredVersionNotInstalled = 3,
        /// <summary>
        /// File not found.
        /// </summary>
        FileNotFound = 4,
        /// <summary>
        /// Upload could not be correlated.
        /// </summary>
        CanNotCorrelate = 5,
    }
}
