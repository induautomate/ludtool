using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixUploadDownloadTool
{

    [Serializable]
    public class RequiredVersionNotInstalledException : Exception
    {
        public RequiredVersionNotInstalledException() { }
        public RequiredVersionNotInstalledException(string message) : base(message) { }
        public RequiredVersionNotInstalledException(string message, Exception inner) : base(message, inner) { }
        protected RequiredVersionNotInstalledException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
