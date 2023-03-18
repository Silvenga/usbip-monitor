using System.Collections.Generic;
using System.Linq;
using Pidgin;
using static Pidgin.Parser;
using static UsbIpMonitor.Core.Parsers.Grammars.CommonGrammar;

namespace UsbIpMonitor.Core.Parsers.Grammars
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
                                                                                .Then(
                                                                                    Map(
                                                                                        (busId, vendorStr, productStr, vendorId, productId) =>
                                                                                            new RemoteExportedDevice(
                                                                                                busId.Trim(),
                                                                                                vendorStr.Trim(),
                                                                                                productStr.Trim(),
                                                                                                vendorId.Trim(),
                                                                                                productId.Trim()
                                                                                            ),
                                                                                        AsciiStringExcept(':').Before(Char(':')).Labelled("BusId"),
                                                                                        AsciiStringExcept(':').Before(Char(':')).Labelled("VendorStr"),
                                                                                        AsciiStringExcept('(').Before(Char('(')).Labelled("ProductStr"),
                                                                                        AsciiStringExcept(':').Before(Char(':')).Labelled("VendorId"),
                                                                                        AsciiStringExcept(')').Before(Char(')')).Labelled("ProductId")
                                                                                    )
                                                                                )
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
                                                                                             (host, device) => new UsbIpRemoteListResult(host, device.ToList()),
                                                                                             HostLine,
                                                                                             Devices
                                                                                         )
                                                                                         .Many()
                                                                                         .Labelled(nameof(Hosts));

        public static readonly Parser<char, IEnumerable<UsbIpRemoteListResult>> Grammar = Header.Then(Hosts);
    }

    public record UsbIpRemoteListResult(string Name, IReadOnlyList<RemoteExportedDevice> Devices);

    public record RemoteExportedDevice(string BusId, string Vendor, string Product, string VendorId, string ProductId);

}
