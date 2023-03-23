using System;
using System.Collections.Generic;
using Pidgin;
using UsbIpMonitor.Core.Linux.Grammars;

namespace UsbIpMonitor.Core.Linux
{
    public interface ILinuxOutputParser
    {
        LinuxKernelVersion ParseKernelVersion(string output);

        IEnumerable<UsbIpRemoteListResult> ParseUsbIpRemoteList(string output);
        IEnumerable<ImportedDevice> ParseUsbIpPort(string output);
    }

    public class LinuxOutputParser : ILinuxOutputParser
    {
        public LinuxKernelVersion ParseKernelVersion(string output)
        {
            try
            {
                return KernelVersionGrammar.Grammar.ParseOrThrow(output);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse output: {output}", e);
            }
        }

        public IEnumerable<UsbIpRemoteListResult> ParseUsbIpRemoteList(string output)
        {
            try
            {
                return UsbIpListGrammar.Grammar.ParseOrThrow(output);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse output: {output}", e);
            }
        }

        public IEnumerable<ImportedDevice> ParseUsbIpPort(string output)
        {
            try
            {
                return UsbIpPortGrammar.Grammar.ParseOrThrow(output);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse output: {output}", e);
            }
        }
    }
}
