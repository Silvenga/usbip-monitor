using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CommandLine;

namespace UsbIpMonitor.Core.Cli
{
    public interface ICliParser
    {
        bool TryParse(IEnumerable<string> args,
                      [NotNullWhen(true)] out CliOptions? options,
                      [NotNullWhen(false)] out IReadOnlyCollection<string>? errors);
    }

    public class CliParser : ICliParser
    {
        public bool TryParse(IEnumerable<string> args,
                             [NotNullWhen(true)] out CliOptions? options,
                             [NotNullWhen(false)] out IReadOnlyCollection<string>? errors)
        {
            var result = Parser.Default.ParseArguments<CliOptions>(args);
            if (result is Parsed<CliOptions>)
            {
                if (!IsValid(result.Value, out errors))
                {
                    options = default;
                    return false;
                }

                options = result.Value;
                errors = default;
                return true;
            }

            options = default;
            errors = Array.Empty<string>();
            return false;
        }

        private static bool IsValid(CliOptions options, out IReadOnlyCollection<string> errors)
        {
            var errorSet = new HashSet<string>();

            if (options.FindId == null)
            {
                if (options.DeviceId == null)
                {
                    errorSet.Add("--device-id is required if --find-by-id is not set.");
                }
            }
            else if (options.BusId != null || options.DeviceId != null)
            {
                errorSet.Add("--find-by-id is incompatible with --bus-id or --device-id.");
            }

            if (!string.IsNullOrWhiteSpace(options.UsbIpPath)
                && !File.Exists(options.UsbIpPath))
            {
                errorSet.Add($"--usb-ip-path was set to '{options.UsbIpPath}', but a file does does not exist at this path.");
            }

            errors = errorSet;
            return errors.Count == 0;
        }
    }
}
