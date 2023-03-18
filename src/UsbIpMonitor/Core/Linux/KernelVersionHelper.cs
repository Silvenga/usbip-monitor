using System.IO;

namespace UsbIpMonitor.Core.Linux
{
    public class KernelVersionHelper
    {
        public KernelVersionHelper()
        {
        }

        public void A()
        {
        }

        public string? GetKernelVersion()
        {
            const string versionPath = "/proc/version";

            if (File.Exists(versionPath))
            {
                var version = File.ReadAllText(versionPath);
                return version;
            }

            return null;
        }
    }
}
