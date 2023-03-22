using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using UsbIpMonitor.Core.Cli;
using Xunit;

namespace UsbIpMonitor.Tests.Core.Cli
{
    public class CliParserFacts
    {
        private static readonly Fixture AutoFixture = new();

        [Fact]
        public void When_no_arguments_are_passed_then_try_parse_should_return_false()
        {
            var input = Array.Empty<string>();
            ICliParser parser = new CliParser();

            // Act
            var result = parser.TryParse(input, out _, out _);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void When_only_host_is_specified_then_try_parse_should_return_false()
        {
            var hostFake = AutoFixture.Create<string>();

            var input = new List<string>
            {
                "-h",
                hostFake
            };
            ICliParser parser = new CliParser();

            // Act
            var result = parser.TryParse(input, out _, out _);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void When_both_device_and_find_are_specified_then_try_parse_should_return_false()
        {
            var input = new List<string>
            {
                "-h",
                AutoFixture.Create<string>(),
                "-d",
                AutoFixture.Create<string>(),
                "-f",
                AutoFixture.Create<string>(),
            };
            ICliParser parser = new CliParser();

            // Act
            var result = parser.TryParse(input, out _, out _);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void When_usb_path_is_invalid_then_try_parse_should_return_false()
        {
            var input = new List<string>
            {
                "-h",
                AutoFixture.Create<string>(),
                "-d",
                AutoFixture.Create<string>(),
                "-p",
                AutoFixture.Create<string>()
            };
            ICliParser parser = new CliParser();

            // Act
            var result = parser.TryParse(input, out var parsedOptions, out _);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void When_only_device_and_host_are_specified_then_try_parse_should_return_valid_options()
        {
            var hostFake = AutoFixture.Create<string>();
            var deviceIdFake = AutoFixture.Create<string>();

            var input = new List<string>
            {
                "-h",
                hostFake,
                "-d",
                deviceIdFake
            };
            ICliParser parser = new CliParser();

            // Act
            var result = parser.TryParse(input, out var parsedOptions, out _);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeTrue();
                parsedOptions!.DeviceId.Should().Be(deviceIdFake);
                parsedOptions.Host.Should().Be(hostFake);
            }
        }
    }
}
