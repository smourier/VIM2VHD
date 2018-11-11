using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;

namespace VIM2VHD
{
    public sealed class WimImage : IDisposable
    {
        private XElement _xmlElement;
        private IntPtr _handle;

        public WimImage(WimFile file, int imageIndex)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (imageIndex > file.ImagesCount)
                throw new ArgumentOutOfRangeException(nameof(imageIndex), "The index does not exist in the specified WIM file.");

            _handle = NativeMethods.WIMLoadImage(file.CheckDisposed(), imageIndex);

            if (!NativeMethods.WIMGetImageInformation(_handle, out var builder, out var bytes))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            // Ensure the length of the returned bytes to avoid garbage characters at the end.
            var count = bytes / sizeof(char);
            if (null != builder)
            {
                // Get rid of the unicode file marker at the beginning of the XML.
                builder.Remove(0, 1);
                builder.EnsureCapacity(count - 1);
                builder.Length = count - 1;

                // This isn't likely to change while we have the image open, so cache it.
                var xml = XDocument.Parse(builder.ToString().Trim());
                _xmlElement = xml.Root;
            }
       }

        public string Index => _xmlElement?.Attribute("INDEX")?.Value;
        public string Name => _xmlElement?.XPathSelectElement("NAME")?.Value;
        public string EditionId => _xmlElement?.XPathSelectElement("WINDOWS/EDITIONID")?.Value;
        public string Flags => _xmlElement?.XPathSelectElement("FLAGS")?.Value;
        public string ProductType => _xmlElement?.XPathSelectElement("WINDOWS/PRODUCTTYPE")?.Value;
        public string InstallationType => _xmlElement?.XPathSelectElement("WINDOWS/INSTALLATIONTYPE")?.Value;
        public string Description => _xmlElement?.XPathSelectElement("DESCRIPTION")?.Value;
        public ulong Size => ulong.Parse(_xmlElement?.XPathSelectElement("TOTALBYTES")?.Value ?? "0");
        public string DefaultLanguage => _xmlElement?.XPathSelectElement("WINDOWS/LANGUAGES/DEFAULT")?.Value;
        public string DisplayName => _xmlElement?.XPathSelectElement("DISPLAYNAME")?.Value;
        public string DisplayDescription => _xmlElement?.XPathSelectElement("DISPLAYDESCRIPTION")?.Value;
        public Architecture ImageArchitecture => (Architecture)int.Parse(_xmlElement?.XPathSelectElement("WINDOWS/ARCH")?.Value ?? "-1");

        public Version Version
        {
            get
            {
                var major = int.Parse(_xmlElement?.XPathSelectElement("WINDOWS/VERSION/MAJOR")?.Value ?? "0");
                var minor = int.Parse(_xmlElement?.XPathSelectElement("WINDOWS/VERSION/MINOR")?.Value ?? "0");
                var build = int.Parse(_xmlElement?.XPathSelectElement("WINDOWS/VERSION/BUILD")?.Value ?? "0");
                var revision = int.Parse(_xmlElement?.XPathSelectElement("WINDOWS/VERSION/SPBUILD")?.Value ?? "0");
                return new Version(major, minor, build, revision);
            }
        }

        public string ApplyingPath { get; private set; }

        public void ExtractPath(string inputPath, string outputPath, WIM_FLAG flags = WIM_FLAG.WIM_FLAG_NONE)
        {
            Extensions.FileCreateDirectory(outputPath);
            if (!NativeMethods.WIMExtractImagePath(CheckDisposed(), inputPath, outputPath, flags))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public void Apply(string applyToPath, WIM_FLAG flags = WIM_FLAG.WIM_FLAG_NONE)
        {
            if (applyToPath == null)
                throw new ArgumentNullException(nameof(applyToPath));

            ApplyingPath = Path.GetFullPath(applyToPath);
            bool b = NativeMethods.WIMApplyImage(_handle, ApplyingPath, flags);
            ApplyingPath = null;
            if (!b)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        private IntPtr CheckDisposed()
        {
            var handle = _handle;
            if (handle == null)
                throw new ObjectDisposedException("Handle");

            return handle;
        }

        public void Dispose()
        {
            var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
            if (handle != IntPtr.Zero)
            {
                NativeMethods.WIMCloseHandle(handle);
            }
        }
    }
}
