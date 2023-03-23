using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace UsbIpMonitor.Core.Abstractions
{
    public interface IProcessWrapper
    {
        Task<string> RunAndGetOutput(string binaryPath,
                                     IEnumerable<string> arguments,
                                     CancellationToken cancellationToken = default);
    }

    public class ProcessWrapper : IProcessWrapper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public async Task<string> RunAndGetOutput(string binaryPath,
                                                  IEnumerable<string> arguments,
                                                  CancellationToken cancellationToken = default)
        {
            var stopWatch = Stopwatch.StartNew();

            var startInfo = new ProcessStartInfo(binaryPath)
            {
                RedirectStandardOutput = true
            };

            foreach (var argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            var argumentList = string.Join(" ", startInfo.ArgumentList);

            cancellationToken.ThrowIfCancellationRequested();

            using var process = Process.Start(startInfo);

            var output = await process!.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            Logger.Debug($"Command '{binaryPath} {argumentList}' exited with status {process.ExitCode} after {stopWatch.ElapsedMilliseconds}ms.");

            if (process.ExitCode != 0)
            {
                throw new Exception(
                    $"Failed to execute command '{binaryPath} {argumentList}', command exited with {process.ExitCode}.");
            }

            return output;
        }
    }
}
