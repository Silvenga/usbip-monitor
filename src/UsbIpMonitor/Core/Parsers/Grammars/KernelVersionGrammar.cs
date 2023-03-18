using Pidgin;
using static Pidgin.Parser;
using static UsbIpMonitor.Core.Parsers.Grammars.CommonGrammar;

namespace UsbIpMonitor.Core.Parsers.Grammars
{
    public static class KernelVersionGrammar
    {
        // Linux version
        // 5.4.0-1104-azure
        // (buildd@lcy02-amd64-102)
        // (gcc version 7.5.0 (Ubuntu 7.5.0-3ubuntu1~18.04))
        // #110~18.04.1-Ubuntu SMP
        // Sat Feb 11 17:41:21 UTC 2023

        private static readonly Parser<char, char> Dot = Char('.').Labelled(nameof(Dot));
        private static readonly Parser<char, string> Digits = Digit.AtLeastOnceString().Labelled(nameof(Digits));

        private static readonly Parser<char, string> VersionPrefix = CIString("Linux version ").Labelled(nameof(VersionPrefix));

        private static readonly Parser<char, LinuxKernelVersion> Version = Map(
            (a, b, c, d) => new LinuxKernelVersion(a, b, c, string.IsNullOrWhiteSpace(d) ? null : d),
            Digits,
            Dot.Then(Digits),
            Dot.Then(Digits),
            OneOf(
                Char('-').Then(AsciiExcept(' ').ManyString()),
                Space.ThenReturn("")
            )
        ).Labelled(nameof(Version));

        private static readonly Parser<char, string> VersionSuffix = Anything.ManyString();

        public static readonly Parser<char, LinuxKernelVersion> Grammar = VersionPrefix.Then(Version).Before(VersionSuffix);
    }

    public record LinuxKernelVersion(string Version, string PatchLevel, string SubLevel, string? ExtraVersion = null)
    {
        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(ExtraVersion)
                ? $"{Version}.{PatchLevel}.{SubLevel}"
                : $"{Version}.{PatchLevel}.{SubLevel}-{ExtraVersion}";
        }
    };
}
