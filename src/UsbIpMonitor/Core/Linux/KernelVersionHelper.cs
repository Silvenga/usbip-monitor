using System.IO;
using UsbIpMonitor.Core.Linux.Grammars;

namespace UsbIpMonitor.Core.Linux
{
    public class KernelVersionHelper
    {
        private readonly ILinuxOutputParser _parser;

        public KernelVersionHelper(ILinuxOutputParser parser)
        {
            _parser = parser;
        }

        public LinuxKernelVersion? GetKernelVersion()
        {
            const string versionPath = "/proc/version";

            if (File.Exists(versionPath))
            {
                var version = File.ReadAllText(versionPath);
                return _parser.ParseKernelVersion(version);
            }

            return null;
        }
    }
}
