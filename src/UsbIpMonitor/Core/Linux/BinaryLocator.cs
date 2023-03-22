using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using NLog;
using UsbIpMonitor.Core.Abstractions;

namespace UsbIpMonitor.Core.Linux
{
    public class BinaryLocator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IEnvironmentWrapper _environmentWrapper;
        private readonly IRuntimeInformationWrapper _runtimeInformationWrapper;
        private readonly IFileWrapper _fileWrapper;

        public BinaryLocator(IEnvironmentWrapper environmentWrapper,
                             IRuntimeInformationWrapper runtimeInformationWrapper,
                             IFileWrapper fileWrapper)
        {
            _environmentWrapper = environmentWrapper;
            _runtimeInformationWrapper = runtimeInformationWrapper;
            _fileWrapper = fileWrapper;
        }

        public bool TryLocateIpUsb([NotNullWhen(true)] out string? fullPath)
        {
            return TryLocate("usbip", out fullPath);
        }

        private bool TryLocate(string binaryName, [NotNullWhen(true)] out string? fullPath)
        {
            Logger.Trace($"Attempting lookup of '{binaryName}' via default paths.");

            foreach (var path in GetBinaryPaths())
            {
                var tryPath = Path.GetFullPath(Path.Combine(path, binaryName));
                Logger.Trace($"Attempting to locate binary at '{tryPath}'.");

                if (_fileWrapper.Exists(tryPath))
                {
                    Logger.Debug($"Binary '{binaryName}' exists at path '{tryPath}'.");
                    fullPath = tryPath;
                    return true;
                }
            }

            Logger.Debug($"Attempted all default path locations, failed to locate '{binaryName}'.");

            fullPath = default;
            return false;
        }

        private IEnumerable<string> GetBinaryPaths()
        {
            var pathSeparator = _runtimeInformationWrapper.IsOSPlatform(OSPlatform.Linux)
                ? ':'
                : throw new ArgumentException("The current platform is unknown, therefore, not supported.");

            var path = _environmentWrapper.GetEnvironment("PATH");
            if (!string.IsNullOrWhiteSpace(path))
            {
                var pathItems = path.Split(new[] { pathSeparator }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                foreach (var item in pathItems)
                {
                    yield return item;
                }
            }
        }
    }
}
