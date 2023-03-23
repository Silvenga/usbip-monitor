using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using CommandLine;
using NLog;

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
            var parser = new Parser(ConfigureParser);
            var result = parser.ParseArguments<CliOptions>(args);
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

        private static void ConfigureParser(ParserSettings settings)
        {
            settings.HelpWriter = LoggingTextWriter.Default;
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

        private class LoggingTextWriter : TextWriter
        {
            public static LoggingTextWriter Default { get; } = new();

            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            public override Encoding Encoding { get; } = Encoding.UTF8;

            public override void Write(string? value)
            {
                if (value != null)
                {
                    Logger.Error(value.TrimEnd());
                }
            }
        }
    }
}
