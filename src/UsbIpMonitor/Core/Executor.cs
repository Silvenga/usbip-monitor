using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using UsbIpMonitor.Core.Cli;

namespace UsbIpMonitor.Core
{
    public class Executor
    {
        public async Task<int> Run(IEnumerable<string> args, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = Parser.Default.ParseArguments<CliOptions>(args);
            if (result is Parsed<CliOptions> parsed)
            {
                await RunImpl(parsed.Value);
                return 0;
            }

            return 1;
        }

        private Task RunImpl(CliOptions cliOptions)
        {
            throw new NotImplementedException();
        }
    }
}
