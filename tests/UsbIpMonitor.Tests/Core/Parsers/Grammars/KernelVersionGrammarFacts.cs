using FluentAssertions;
using Pidgin;
using UsbIpMonitor.Core.Parsers.Grammars;
using Xunit;

namespace UsbIpMonitor.Tests.Core.Parsers.Grammars
{
    public class KernelVersionGrammarFacts
    {
        [Fact]
        public void When_kernel_version_has_extra_version_then_extra_version_should_be_parsed()
        {
            const string input =
                "Linux version 5.4.0-1104-azure (buildd@lcy02-amd64-102) (gcc version 7.5.0 (Ubuntu 7.5.0-3ubuntu1~18.04)) #110~18.04.1-Ubuntu SMP Sat Feb 11 17:41:21 UTC 2023";

            // Act
            var result = KernelVersionGrammar.Grammar.ParseOrThrow(input);

            // Assert
            result.Should().Be(new LinuxKernelVersion("5", "4", "0", "1104-azure"));
        }

        [Fact]
        public void When_kernel_version_has_no_extra_version_then_version_should_still_be_parsed()
        {
            const string input =
                "Linux version 5.4.0 (buildd@lcy02-amd64-102) (gcc version 7.5.0 (Ubuntu 7.5.0-3ubuntu1~18.04)) #110~18.04.1-Ubuntu SMP Sat Feb 11 17:41:21 UTC 2023";

            // Act
            var result = KernelVersionGrammar.Grammar.ParseOrThrow(input);

            // Assert
            result.Should().Be(new LinuxKernelVersion("5", "4", "0"));
        }

        [Fact]
        public void When_kernel_has_multiple_digits_then_version_should_be_parsed()
        {
            const string input =
                "Linux version 500.400.100 (buildd@lcy02-amd64-102) (gcc version 7.5.0 (Ubuntu 7.5.0-3ubuntu1~18.04)) #110~18.04.1-Ubuntu SMP Sat Feb 11 17:41:21 UTC 2023";

            // Act
            var result = KernelVersionGrammar.Grammar.ParseOrThrow(input);

            // Assert
            result.Should().Be(new LinuxKernelVersion("500", "400", "100"));
        }
    }
}
