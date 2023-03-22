using System.IO;

namespace UsbIpMonitor.Core.Abstractions
{
    public interface IFileWrapper
    {
        bool Exists(string filePath);
    }

    public class FileWrapper : IFileWrapper
    {
        public bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}
