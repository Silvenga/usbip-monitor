using System;
using System.Collections.Generic;
using Pidgin;
using static Pidgin.Parser;
using static UsbIpMonitor.Core.Parsers.Grammars.CommonGrammar;

namespace UsbIpMonitor.Core.Parsers.Grammars
{
    public static class UsbIpPortGrammar
    {
        // usbip: error: failed to open /usr/share/hwdata//usb.ids
        // Imported USB devices
        // ====================
        // Port 00: <Port in Use> at Full Speed(12Mbps)
        //        unknown vendor : unknown product (0403:6001)
        //        1-1 -> usbip://192.168.0.121:3240/1-7
        //            -> remote bus/dev 001/002

        private static readonly Parser<char, Unit> Error = CIString("usbip: error:")
                                                           .Then(SkipRestOfLine)
                                                           .Labelled(nameof(Error));

        private static readonly Parser<char, Unit> Header = Error.Many()
                                                                 .Then(CIString("Imported USB devices"))
                                                                 .Then(EndOfLine)
                                                                 .Then(Char('=').Many())
                                                                 .Then(EndOfLine)
                                                                 .ThenReturn(Unit.Value)
                                                                 .Labelled(nameof(Header));

        private static readonly Parser<char, ImportedDeviceStatus> Status = Whitespaces
                                                                            .Then(String("Port "))
                                                                            .Then(
                                                                                Map(
                                                                                    (portNumber, inUse, speed) =>
                                                                                        new ImportedDeviceStatus(portNumber, inUse.HasValue, speed),
                                                                                    Digit.AtLeastOnceString().Before(String(": ")),
                                                                                    CIString("<Port in Use> ").Optional().Before(String("at ")),
                                                                                    AnyCharExceptEndOfLine.ManyString()
                                                                                )
                                                                            )
                                                                            .Before(EndOfLine)
                                                                            .Labelled(nameof(Status));

        private static readonly Parser<char, ImportedDeviceMetadata> Metadata = Whitespaces
                                                                                .Then(
                                                                                    Map(
                                                                                        (vendorStr, productStr, vendorId, productId) =>
                                                                                            new ImportedDeviceMetadata(
                                                                                                vendorStr.Trim(),
                                                                                                productStr.Trim(),
                                                                                                vendorId.Trim(),
                                                                                                productId.Trim()
                                                                                            ),
                                                                                        AsciiStringExcept(':').Before(Char(':')).Labelled("VendorStr"),
                                                                                        AsciiStringExcept('(').Before(Char('(')).Labelled("ProductStr"),
                                                                                        AsciiStringExcept(':').Before(Char(':')).Labelled("VendorId"),
                                                                                        AsciiStringExcept(')').Before(Char(')')).Labelled("ProductId")
                                                                                    )
                                                                                )
                                                                                .Before(EndOfLine)
                                                                                .Labelled(nameof(Metadata));

        private static readonly Parser<char, ImportedDeviceRemote> Remote = Whitespaces
                                                                            .Then(
                                                                                Map(
                                                                                    (_, uriStr) =>
                                                                                    {
                                                                                        var uri = new Uri(uriStr, UriKind.Absolute);
                                                                                        var cleanUri = new UriBuilder(uri)
                                                                                        {
                                                                                            Path = null
                                                                                        };
                                                                                        return new ImportedDeviceRemote(
                                                                                            cleanUri.Uri,
                                                                                            uri.AbsolutePath.TrimStart('/')
                                                                                        );
                                                                                    },
                                                                                    AnyCharExceptEndOfLine.Until(String(" -> ")),
                                                                                    AnyCharExceptEndOfLine.AtLeastOnceString()
                                                                                )
                                                                            )
                                                                            .Before(EndOfLine)
                                                                            .Labelled(nameof(Remote));

        private static readonly Parser<char, ImportedDevice> Device = Whitespaces
                                                                      .Then(
                                                                          Map(
                                                                              (status, metadata, remote) => new ImportedDevice(status, metadata, remote),
                                                                              Status,
                                                                              Metadata,
                                                                              Remote
                                                                          )
                                                                      )
                                                                      .Before(AnyCharExceptEndOfLine.Many().Then(EndOfLine.Optional()))
                                                                      .Labelled(nameof(Device));

        private static readonly Parser<char, IEnumerable<ImportedDevice>> Devices = Device
                                                                                    .Many()
                                                                                    .Labelled(nameof(Devices));

        public static readonly Parser<char, IEnumerable<ImportedDevice>> Grammar = Header.Then(Devices);
    }

    public record ImportedDevice(ImportedDeviceStatus Status, ImportedDeviceMetadata Metadata, ImportedDeviceRemote Remote);

    public record ImportedDeviceStatus(string Port, bool InUse, string Speed);

    public record ImportedDeviceMetadata(string Vendor, string Product, string VendorId, string ProductId);

    public record ImportedDeviceRemote(Uri RemoteHost, string RemoteBusId);
}
