using System.IO;
using System.Reflection;

namespace UsbIpMonitor.Tests
{
    public static class ResourceHelper
    {
        public static string GetFile(string file)
        {
            var resourceName = $"UsbIpMonitor.Tests.Examples.{file}";
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);
            var result = reader.ReadToEnd();
            return result;
        }
    }
}
