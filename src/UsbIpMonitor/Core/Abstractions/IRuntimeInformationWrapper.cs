using System.Runtime.InteropServices;

namespace UsbIpMonitor.Core.Abstractions
{
    public interface IRuntimeInformationWrapper
    {
        // ReSharper disable once InconsistentNaming
        bool IsOSPlatform(OSPlatform platform);
    }

    public class RuntimeInformationWrapper : IRuntimeInformationWrapper
    {
        public bool IsOSPlatform(OSPlatform platform)
        {
            return RuntimeInformation.IsOSPlatform(platform);
        }
    }
}
