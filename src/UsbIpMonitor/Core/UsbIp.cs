using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace UsbIpMonitor.Core
{
    public interface IUsbIpDriver
    {
    }

    public class UsbIpDriver : IUsbIpDriver
    {
        private readonly Uri _remote;
        private readonly string _usbIpPath;

        public UsbIpDriver(Uri remote, string usbIpPath)
        {
            _remote = remote;
            _usbIpPath = usbIpPath;
        }

        public async Task List()
        {
        }

        private async Task<string> Run(IEnumerable<string> arguments, CancellationToken cancellationToken = default)
        {
            var startInfo = new ProcessStartInfo(_usbIpPath)
            {
                RedirectStandardOutput = true
            };

            foreach (var argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            using var process = Process.Start(startInfo);

            var output = await process!.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                throw new Exception(
                    $"Failed to execute command '{_usbIpPath} {string.Join(" ", startInfo.ArgumentList)}', command exited with {process.ExitCode}.");
            }

            return output;
        }
    }
}
