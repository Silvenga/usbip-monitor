using System;

namespace UsbIpMonitor.Core.Abstractions
{
    public interface IEnvironmentWrapper
    {
        string? GetEnvironment(string name);
    }

    public class EnvironmentWrapper : IEnvironmentWrapper
    {
        public string? GetEnvironment(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }
    }
}
