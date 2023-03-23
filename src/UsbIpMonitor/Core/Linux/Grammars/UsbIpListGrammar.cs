using System;
using System.Collections.Generic;
using System.Linq;
using Pidgin;
using static Pidgin.Parser;
using static UsbIpMonitor.Core.Linux.Grammars.CommonGrammar;

namespace UsbIpMonitor.Core.Linux.Grammars
{
    public static class UsbIpListGrammar
    {
        // usbip: error: failed to open /usr/share/hwdata//usb.ids
        // Exportable USB devices
        // ======================
        //  - 192.168.0.121
        //         1-7: unknown vendor : unknown product (0403:6001)
        //            : /sys/devices/pci0000:00/0000:00:14.0/usb1/1-7
        //            : (Defined at Interface level) (00/00/00)
        //            :  0 - unknown class / unknown subclass / unknown protocol (ff/ff/ff)
        //
        //       1-1.6: Yubico.com : Yubikey 4/5 OTP+U2F+CCID (1050:0407)
        //            : /sys/devices/pci0000:00/0000:00:1a.0/usb1/1-1/1-1.6
        //            : (Defined at Interface level) (00/00/00)

        private static readonly Parser<char, Unit> Error = CIString("usbip: error:")
                                                           .Then(SkipRestOfLine)
                                                           .Labelled(nameof(Error));

        private static readonly Parser<char, Unit> Header = Error.Many()
                                                                 .Then(CIString("Exportable USB devices"))
                                                                 .Then(EndOfLine)
                                                                 .Then(Char('=').Many())
                                                                 .Then(EndOfLine)
                                                                 .ThenReturn(Unit.Value)
                                                                 .Labelled(nameof(Header));

        private static readonly Parser<char, string> HostLine = Whitespaces
                                                                .Then(String("- "))
                                                                .Then(AnyCharExceptEndOfLine.AtLeastOnceString())
                                                                .Before(EndOfLine)
                                                                .Labelled(nameof(HostLine));

        private static readonly Parser<char, RemoteExportedDevice> DeviceLine = Whitespaces
                                                                                .Then(new DeviceLineParser())
                                                                                .Assert(x => !x.BusId.StartsWith("-"))
                                                                                .Before(EndOfLine)
                                                                                .Labelled(nameof(DeviceLine));

        private static readonly Parser<char, string> SubDeviceLine = Whitespaces
                                                                     .Then(Char(':'))
                                                                     .Then(AnyCharExceptEndOfLine.ManyString())
                                                                     .Before(EndOfLine)
                                                                     .Labelled(nameof(SubDeviceLine));

        private static readonly Parser<char, IEnumerable<RemoteExportedDevice>> Devices = Try(DeviceLine)
                                                                                          .Before(Try(SubDeviceLine).Many())
                                                                                          .Many()
                                                                                          .Labelled(nameof(Devices));

        private static readonly Parser<char, IEnumerable<UsbIpRemoteListResult>> Hosts = Map(
                                                                                             (host, device, _) => new UsbIpRemoteListResult(host, device.ToList()),
                                                                                             HostLine,
                                                                                             Devices,
                                                                                             Whitespace.Many()
                                                                                         )
                                                                                         .Many()
                                                                                         .Labelled(nameof(Hosts));

        public static readonly Parser<char, IEnumerable<UsbIpRemoteListResult>> Grammar = Header.Then(Hosts);

        private class DeviceLineParser : Parser<char, RemoteExportedDevice>
        {
            public override bool TryParse(ref ParseState<char> state,
                                          ref PooledList<Expected<char>> expecteds,
                                          out RemoteExportedDevice result)
            {
                if (AnyCharExceptEndOfLine.AtLeastOnceString().TryParse(ref state, ref expecteds, out var line))
                {
                    try
                    {
                        // 1-7: Future Technology Devices International, Ltd : FT232 Serial (UART) IC (0403:6001)

                        var span = line.AsSpan();

                        // 1-7:
                        var busIdStart = span.IndexOfAnyExcept(' ');
                        var busIdEnd = line.IndexOf(':');
                        var busId = new string(span[busIdStart..busIdEnd]);

                        // (0403
                        var vendorIdStart = span.LastIndexOf('(') + 1;
                        var vendorIdEnd = span.LastIndexOf(':');
                        var vendorId = new string(span[vendorIdStart..vendorIdEnd]);

                        // :6001)
                        var productIdStart = vendorIdEnd + 1;
                        var productIdEnd = span.LastIndexOf(')');
                        var productId = new string(span[productIdStart..productIdEnd]);

                        // Future Technology Devices International, Ltd : FT232 Serial
                        var vendorStart = busIdEnd + 2;
                        var productEnd = vendorIdStart - 2;

                        var vendorEnd = vendorStart + span[vendorStart..productEnd].IndexOf(':') - 1;
                        var productStart = vendorEnd + 3;

                        var vendor = new string(span[vendorStart..vendorEnd]);
                        var product = new string(span[productStart..productEnd]);

                        result = new RemoteExportedDevice(busId, vendor, product, vendorId, productId);
                        return true;
                    }
                    catch
                    {
                        // If we failed to parse (e.g. something was unexpected) return false to fall back to the parent parser.
                    }
                }

                result = default!;
                return false;
            }
        }
    }

    public record UsbIpRemoteListResult(string Name, IReadOnlyList<RemoteExportedDevice> Devices);

    public record RemoteExportedDevice(string BusId, string Vendor, string Product, string VendorId, string ProductId);
}
