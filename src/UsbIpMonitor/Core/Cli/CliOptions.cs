using CommandLine;

namespace UsbIpMonitor.Core.Cli
{
    public class CliOptions
    {
        // Standard options.

        [Option('d', "device-id", HelpText = "Id of the virtual UDC on the host specified by --host. This matches the option in usbip.")]
        public string? DeviceId { get; set; }

        [Option('b', "bus-id",
            HelpText = "Bus Id of the device on the host specified by --host. This matches the option in usbip. Required if --find-id is not specified.")]
        public string? BusId { get; set; }

        [Option('f', "find-by-id", HelpText = "Attempt to search for a single device that matches the format: '<vendor id>:<product id>'.")]
        public string? FindId { get; set; }

        [Option('h', "host", Required = true, HelpText = "The remote host exporting devices via usbipd. This is required.")]
        public string Host { get; set; } = null!;

        // Advanced options.

        [Option('p', "usb-ip-path", HelpText = "Advanced: The path to a usbip binary. If not specified, a usbip binary will be discovered.")]
        public string? UsbIpPath { get; set; }
    }
}
