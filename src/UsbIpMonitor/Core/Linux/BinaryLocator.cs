using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NLog;
using UsbIpMonitor.Core.Abstractions;

namespace UsbIpMonitor.Core.Linux
{
    public interface IBinaryLocator
    {
        bool TryLocateIpUsb([NotNullWhen(true)] out string? fullPath);
    }

    public class BinaryLocator : IBinaryLocator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IEnvironmentWrapper _environmentWrapper;
        private readonly IRuntimeInformationWrapper _runtimeInformationWrapper;
        private readonly IFileWrapper _fileWrapper;
        private readonly IKernelVersionWrapper _kernelVersionWrapper;

        public BinaryLocator(IEnvironmentWrapper environmentWrapper,
                             IRuntimeInformationWrapper runtimeInformationWrapper,
                             IFileWrapper fileWrapper,
                             IKernelVersionWrapper kernelVersionWrapper)
        {
            _environmentWrapper = environmentWrapper;
            _runtimeInformationWrapper = runtimeInformationWrapper;
            _fileWrapper = fileWrapper;
            _kernelVersionWrapper = kernelVersionWrapper;
        }

        public bool TryLocateIpUsb([NotNullWhen(true)] out string? fullPath)
        {
            // Try kernel tools paths, then use PATH.
            // The usbip file in PATH is just a bash file that forwards to the tools paths.
            var paths = GetKernelToolsPaths().Concat(GetBinaryPaths());
            return TryLocate("usbip", paths, out fullPath);
        }

        private bool TryLocate(string binaryName, IEnumerable<string> searchPaths, [NotNullWhen(true)] out string? fullPath)
        {
            Logger.Trace($"Attempting lookup of '{binaryName}' via default paths.");

            // Deduplicate while streaming, Distinct() may not keep order (which we need here).
            var attemptedPaths = new HashSet<string>();

            foreach (var searchPath in searchPaths)
            {
                var tryPath = Path.GetFullPath(Path.Combine(searchPath, binaryName));
                if (!attemptedPaths.Contains(tryPath))
                {
                    attemptedPaths.Add(tryPath);

                    Logger.Trace($"Attempting to locate binary at '{tryPath}'.");
                    if (_fileWrapper.Exists(tryPath))
                    {
                        Logger.Debug($"Binary '{binaryName}' exists at path '{tryPath}'.");
                        fullPath = tryPath;
                        return true;
                    }
                }
            }

            Logger.Debug($"Attempted all search locations, failed to locate '{binaryName}'.");

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

        private IEnumerable<string> GetKernelToolsPaths()
        {
            if (!_runtimeInformationWrapper.IsOSPlatform(OSPlatform.Linux))
            {
                throw new ArgumentException("The current platform is unknown, therefore, not supported.");
            }

            // We can't request a mount at /usr/lib since that breaks everything.
            // We need to namespace the container fs and the host fs.
            var searchPrefix = Environment.GetEnvironmentVariable("USBIP_MONITOR_HOSTFSPREFIX") ?? "/";
            const string basePath = "/hostfs/usr/lib/linux-tools/";

            if (_kernelVersionWrapper.GetKernelVersion() is { } kernelVersion)
            {
                Logger.Trace($"Detected that the current kernel is version '{kernelVersion}', returning this tool path.");
                yield return Path.Combine(searchPrefix, basePath, kernelVersion.ToString());
            }

            foreach (var directory in Directory.EnumerateDirectories(basePath))
            {
                yield return Path.GetFullPath(directory);
            }
        }
    }
}
