using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Pidgin;
using UsbIpMonitor.Core.Linux.Grammars;
using Xunit;

namespace UsbIpMonitor.Tests.Core.Linux.Grammars
{
    public class UsbIpPortFacts
    {
        [Fact]
        public void When_ports_exist_then_device_should_be_parsed()
        {
            var input = ResourceHelper.GetFile("usbip-port-1.txt");

            // Act
            var result = UsbIpPortGrammar.Grammar.ParseOrThrow(input).ToList();

            // Assert
            var device = result.Should().ContainSingle().Subject;

            device.Status.InUse.Should().BeTrue();
            device.Status.Port.Should().Be("00");
            device.Status.Speed.Should().Be("Full Speed(12Mbps)");

            device.Metadata.Vendor.Should().Be("unknown vendor");
            device.Metadata.Product.Should().Be("unknown product");
            device.Metadata.VendorId.Should().Be("0403");
            device.Metadata.ProductId.Should().Be("6001");

            device.Remote!.RemoteBusId.Should().Be("1-7");
            device.Remote.RemoteHost.Should().Be(new Uri("usbip://192.168.0.121:3240"));
        }

        [Fact]
        public void When_multiple_ports_are_imported_then_multiple_devices_should_be_returned()
        {
            var input = ResourceHelper.GetFile("usbip-port-2.txt");

            // Act
            var result = UsbIpPortGrammar.Grammar.ParseOrThrow(input).ToList();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public void When_port_name_contains_parentheses_then_device_should_be_returned()
        {
            var input = ResourceHelper.GetFile("usbip-port-3.txt");

            // Act
            var result = UsbIpPortGrammar.Grammar.ParseOrThrow(input).ToList();

            // Assert
            using (new AssertionScope())
            {
                var device = result.Should().ContainSingle().Subject;

                device.Status.InUse.Should().BeTrue();
                device.Status.Port.Should().Be("00");
                device.Status.Speed.Should().Be("Full Speed(12Mbps)");

                device.Metadata.Vendor.Should().Be("Future Technology Devices International, Ltd");
                device.Metadata.Product.Should().Be("FT232 Serial (UART) IC");
                device.Metadata.VendorId.Should().Be("0403");
                device.Metadata.ProductId.Should().Be("6001");

                device.Remote!.RemoteBusId.Should().Be("1-7");
                device.Remote.RemoteHost.Should().Be(new Uri("usbip://br1:3240"));
            }
        }

        [Fact]
        public void A()
        {
            var input = ResourceHelper.GetFile("usbip-port-4.txt");

            // Act
            var result = UsbIpPortGrammar.Grammar.ParseOrThrow(input).ToList();

            // Assert
            using (new AssertionScope())
            {
                var device = result.Should().ContainSingle().Subject;

                device.Status.InUse.Should().BeTrue();
                device.Status.Port.Should().Be("00");
                device.Status.Speed.Should().Be("Full Speed(12Mbps)");

                device.Metadata.Vendor.Should().Be("Future Technology Devices International, Ltd");
                device.Metadata.Product.Should().Be("FT232 Serial (UART) IC");
                device.Metadata.VendorId.Should().Be("0403");
                device.Metadata.ProductId.Should().Be("6001");

                device.Remote.Should().BeNull();
            }
        }
    }
}
