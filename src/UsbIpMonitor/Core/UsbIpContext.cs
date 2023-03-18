using System;

namespace UsbIpMonitor.Core
{
    public record UsbIpContext(string KernelVersion, Uri RemoteHost, string UsrLibPrefix);
}
