using UsbIpMonitor.Core.Abstractions;
using UsbIpMonitor.Core.Linux.Grammars;

namespace UsbIpMonitor.Core.Linux
{
    public interface IKernelVersionWrapper
    {
        LinuxKernelVersion? GetKernelVersion();
    }

    public class KernelVersionWrapper : IKernelVersionWrapper
    {
        private readonly ILinuxOutputParser _parser;
        private readonly IFileWrapper _fileWrapper;

        public KernelVersionWrapper(ILinuxOutputParser parser, IFileWrapper fileWrapper)
        {
            _parser = parser;
            _fileWrapper = fileWrapper;
        }

        public LinuxKernelVersion? GetKernelVersion()
        {
            const string versionPath = "/proc/version";

            if (_fileWrapper.Exists(versionPath))
            {
                var version = _fileWrapper.ReadAllText(versionPath);
                return _parser.ParseKernelVersion(version);
            }

            return null;
        }
    }
}
