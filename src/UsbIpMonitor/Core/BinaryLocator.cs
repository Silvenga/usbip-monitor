using System.Diagnostics.CodeAnalysis;

namespace UsbIpMonitor.Core
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
