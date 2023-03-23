using System.IO;

namespace UsbIpMonitor.Core.Abstractions
{
    public interface IFileWrapper
    {
        bool Exists(string filePath);
        string ReadAllText(string filePath);
    }

    public class FileWrapper : IFileWrapper
    {
        public bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }

        public string ReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }
    }
}
