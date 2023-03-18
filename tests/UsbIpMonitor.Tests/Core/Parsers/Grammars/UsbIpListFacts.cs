using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Pidgin;
using UsbIpMonitor.Core.Parsers.Grammars;
using Xunit;

namespace UsbIpMonitor.Tests.Core.Parsers.Grammars
{
    public class UsbIpListFacts
    {
        [Fact]
        public void When_multiple_devices_exist_then_each_device_should_be_parsed()
        {
            var input = ResourceHelper.GetFile("usbip-list-1.txt");

            // Act
            var result = UsbIpListGrammar.Grammar.ParseOrThrow(input).ToList();

            // Assert
            var host = result.Should().ContainSingle().Subject;

            host.Name.Should().Be("192.168.0.121");
            host.Devices.Should().BeEquivalentTo(new List<RemoteExportedDevice>
            {
                new("1-7", "unknown vendor", "unknown product", "0403", "6001"),
                new("1-1.6", "Yubico.com", "Yubikey 4/5 OTP+U2F+CCID", "1050", "0407")
            });
        }

        [Fact]
        public void When_multiple_hosts_exist_then_each_should_be_parsed()
        {
            var input = ResourceHelper.GetFile("usbip-list-2.txt");

            // Act
            var result = UsbIpListGrammar.Grammar.ParseOrThrow(input).ToList();

            // Assert
            result.Should().HaveCount(2);
        }
    }
}
