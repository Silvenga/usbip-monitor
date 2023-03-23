using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UsbIpMonitor.Core.Abstractions;
using UsbIpMonitor.Core.Linux;
using UsbIpMonitor.Core.Linux.Grammars;

namespace UsbIpMonitor.Core
{
    public interface IUsbIpDriver
    {
        string RemoteHost { get; }
        int RemotePort { get; }

        /// <summary>
        /// Runs: usbip list -r host
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<UsbIpRemoteListResult>> List(CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs: usbip port
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<ImportedDevice>> Port(CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs: usbip attach -r host
        /// </summary>
        /// <param name="busId"></param>
        /// <param name="deviceId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Attach(string busId, string? deviceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs: usbip detach -r host
        /// </summary>
        /// <param name="port"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Detach(string port, CancellationToken cancellationToken = default);
    }

    public class UsbIpDriver : IUsbIpDriver
    {
        private readonly string _remoteEndpoint;
        private readonly string _usbIpPath;
        private readonly ILinuxOutputParser _linuxOutputParser;
        private readonly IProcessWrapper _processWrapper;

        public string RemoteHost { get; }
        public int RemotePort { get; }

        public UsbIpDriver(Uri remote, string usbIpPath, ILinuxOutputParser linuxOutputParser, IProcessWrapper processWrapper)
        {
            // Make sure our string only contains host + port.

            RemoteHost = remote.Host;
            RemotePort = remote.IsDefaultPort ? 3240 : remote.Port;

            // usbip does not appear to support changing the port.
            // And the client side doesn't seem to support using a non-standard port.
            _remoteEndpoint = RemoteHost;

            _usbIpPath = usbIpPath;
            _linuxOutputParser = linuxOutputParser;
            _processWrapper = processWrapper;
        }

        /// <summary>
        /// Runs: usbip list -r host
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<UsbIpRemoteListResult>> List(CancellationToken cancellationToken = default)
        {
            var output = await _processWrapper.RunAndGetOutput(
                _usbIpPath,
                new List<string>
                {
                    "list",
                    $"--remote={_remoteEndpoint}"
                },
                cancellationToken
            );

            return _linuxOutputParser.ParseUsbIpRemoteList(output);
        }

        /// <summary>
        /// Runs: usbip port
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ImportedDevice>> Port(CancellationToken cancellationToken = default)
        {
            var output = await _processWrapper.RunAndGetOutput(
                _usbIpPath,
                new List<string>
                {
                    "port"
                },
                cancellationToken
            );

            return _linuxOutputParser.ParseUsbIpPort(output);
        }

        /// <summary>
        /// Runs: usbip attach -r host
        /// </summary>
        /// <param name="busId"></param>
        /// <param name="deviceId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Attach(string busId, string? deviceId, CancellationToken cancellationToken = default)
        {
            var args = new List<string>
            {
                "attach",
                $"--remote={_remoteEndpoint}",
                $"--busid={busId}"
            };

            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                args.Add($"--device={deviceId}");
            }

            await _processWrapper.RunAndGetOutput(
                _usbIpPath,
                args,
                cancellationToken
            );
        }

        /// <summary>
        /// Runs: usbip detach -r host
        /// </summary>
        /// <param name="port"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Detach(string port, CancellationToken cancellationToken = default)
        {
            var args = new List<string>
            {
                "detach",
                $"--port={port}"
            };

            await _processWrapper.RunAndGetOutput(
                _usbIpPath,
                args,
                cancellationToken
            );
        }
    }
}
