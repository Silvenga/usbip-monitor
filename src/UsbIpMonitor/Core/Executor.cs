using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using UsbIpMonitor.Core.Cli;

namespace UsbIpMonitor.Core
{
    public class Executor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ICliParser _cliParser;

        public Executor(ICliParser cliParser)
        {
            _cliParser = cliParser;
        }

        public async Task<int> Run(IEnumerable<string> args, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_cliParser.TryParse(args, out var options, out var errors))
            {
                await RunImpl(options);
                return 0;
            }

            Logger.Error("Failed to validate options.");
            foreach (var error in errors)
            {
                Logger.Error(error);
            }

            return 1;
        }

        private Task RunImpl(CliOptions cliOptions)
        {
            throw new NotImplementedException();
        }
    }
}
