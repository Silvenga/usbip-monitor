using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using UsbIpMonitor.Core.Cli;
using UsbIpMonitor.Core.Linux.Grammars;

namespace UsbIpMonitor.Core
{
    public class Executor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ICliParser _cliParser;
        private readonly IUsbIpDriverFactory _factory;

        public Executor(ICliParser cliParser, IUsbIpDriverFactory factory)
        {
            _cliParser = cliParser;
            _factory = factory;
        }

        public async Task<int> Run(IEnumerable<string> args, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_cliParser.TryParse(args, out var options, out var errors))
            {
                await RunImpl(options, cancellationToken);
                return 0;
            }

            Logger.Error("Failed to validate options.");
            foreach (var error in errors)
            {
                Logger.Error(error);
            }

            return 1;
        }

        private async Task RunImpl(CliOptions options, CancellationToken cancellationToken = default)
        {
            var driver = _factory.Create(options);

            Logger.Info("Verifying the existance of the remote exported USB device...");
            var (busId, deviceId) = await GetRemoteDevice(driver, options, cancellationToken);

            Logger.Info($"Attaching the device {deviceId ?? "<root>"}/{busId}...");
            await driver.Attach(busId, deviceId, cancellationToken);

            Logger.Info("Attempting to map the attached device to a local port...");
            var myPort = await GetMyPortOrThrow(driver, busId, cancellationToken);

            Logger.Info($"Mapped attached device to local port '{myPort}', "
                        + "waiting for termination signal before detaching port...");

            try
            {
                var poolingInterval = TimeSpan.FromSeconds(3);
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(poolingInterval, cancellationToken);
                    myPort = await GetMyPortOrThrow(driver, busId, cancellationToken);
                }
            }
            finally
            {
                Logger.Info($"Recieved termination signal, attempting to gracefully detach monitored port '{myPort}'...");

                // Wait at most 1 second before moving on.
                using var detachTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
                await driver.Detach(myPort, detachTokenSource.Token);

                Logger.Info("Graceful detach completed.");
            }
        }

        private static async Task<(string BusId, string? DeviceId)> GetRemoteDevice(IUsbIpDriver driver,
                                                                                    CliOptions options,
                                                                                    CancellationToken cancellationToken = default)
        {
            var host = (await driver.List(cancellationToken)).Single();

            Logger.Debug($"Listed exported usb devices on host '{host.Name}'.");
            foreach (var device in host.Devices)
            {
                Logger.Debug($"Found USB device '{device.BusId}' "
                             + $"with identity '{device.VendorId}:{device.ProductId}' "
                             + $"(Vendor: '{device.Vendor}', Product: '{device.Product}').");
            }

            if (!string.IsNullOrWhiteSpace(options.FindId))
            {
                // Should attempt to find by id.
                var devices = host.Devices
                                  .Where(x => string.Equals($"{x.VendorId}:{x.ProductId}", options.FindId, StringComparison.Ordinal))
                                  .ToList();

                if (devices.Count == 0)
                {
                    throw new Exception($"Failed to locate any device by id '{options.FindId}'.");
                }

                if (devices.Count > 1)
                {
                    throw new Exception($"Found {devices.Count} by id '{options.FindId}', this isn't supported.");
                }

                var device = devices.Single();
                return (device.BusId, null);
            }

            if (!string.IsNullOrWhiteSpace(options.BusId))
            {
                Logger.Debug("Blindly trusting device id/bus id provided by input.");
                return (options.BusId, options.DeviceId);
            }

            throw new Exception("Something impossible just happened...");
        }

        private static async Task<string> GetMyPortOrThrow(IUsbIpDriver driver, string busId, CancellationToken cancellationToken)
        {
            string? port;
            var getPortAttempt = 1;
            const int maxGetPortAttempts = 10;
            while ((port = await MapRemoteToLocalPort(driver, busId, cancellationToken)) == null)
            {
                if (getPortAttempt >= maxGetPortAttempts)
                {
                    throw new Exception("Failed to map port, bailing...");
                }

                Logger.Info($"Unable to map to a locally attached service, waiting 1s to try again (Attempt: {getPortAttempt}/{maxGetPortAttempts}).");
                await Task.Delay(1000, cancellationToken);
                getPortAttempt++;
            }

            return port;
        }

        private static async Task<string?> MapRemoteToLocalPort(IUsbIpDriver driver,
                                                                string busId,
                                                                CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ImportedDevice> importedDevices = (await driver.Port(cancellationToken)).ToList();

            Logger.Debug("Listed locally imported usb devices.");
            foreach (var device in importedDevices)
            {
                Logger.Debug($"Found USB device '{device.Metadata.VendorId}:{device.Metadata.ProductId}' "
                             + $"on port '{device.Status.Port}' "
                             + $"from host '{device.Remote.RemoteHost}' "
                             + $"(BusId: {device.Remote.RemoteBusId}, InUse: {device.Status.InUse}, Speed: {device.Status.Speed}).");
            }

            var attachedDevice = importedDevices.SingleOrDefault(x => x.Remote.RemoteBusId == busId && x.Remote.RemoteHost.Host == driver.RemoteHost);
            return attachedDevice?.Status.Port;
        }
    }
}
