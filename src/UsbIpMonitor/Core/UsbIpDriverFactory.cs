using System;
using UsbIpMonitor.Core.Abstractions;
using UsbIpMonitor.Core.Cli;
using UsbIpMonitor.Core.Linux;

namespace UsbIpMonitor.Core
{
    public interface IUsbIpDriverFactory
    {
        IUsbIpDriver Create(CliOptions options);
    }

    public class UsbIpDriverFactory : IUsbIpDriverFactory
    {
        private readonly ILinuxOutputParser _linuxOutputParser;
        private readonly IBinaryLocator _binaryLocator;
        private readonly IProcessWrapper _processWrapper;

        public UsbIpDriverFactory(ILinuxOutputParser linuxOutputParser,
                                  IBinaryLocator binaryLocator,
                                  IProcessWrapper processWrapper)
        {
            _linuxOutputParser = linuxOutputParser;
            _binaryLocator = binaryLocator;
            _processWrapper = processWrapper;
        }

        public IUsbIpDriver Create(CliOptions options)
        {
            var usbIpPath = options.UsbIpPath;

            if (string.IsNullOrWhiteSpace(usbIpPath)
                && _binaryLocator.TryLocateIpUsb(out var foundPath))
            {
                usbIpPath = foundPath;
            }

            if (string.IsNullOrWhiteSpace(usbIpPath))
            {
                throw new Exception("Failed to locate usbip.");
            }

            var remote = new Uri($"usbip://{options.Host}");

            return new UsbIpDriver(remote, usbIpPath, _linuxOutputParser, _processWrapper);
        }
    }
}
