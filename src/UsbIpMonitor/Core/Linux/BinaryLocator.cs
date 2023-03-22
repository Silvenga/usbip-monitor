using System.Diagnostics.CodeAnalysis;

namespace UsbIpMonitor.Core.Linux
{
    public class BinaryLocator
    {
        public bool TryLocateIpUsb([NotNullWhen(true)] out string? fullPath)
        {
            fullPath = default;
            return false;
        }
    }
}
