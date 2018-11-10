using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace VIM2VHD
{
    public class WimImage
    {
        private XDocument _xmlInfo;
        private string lang;

        public WimImage(WimFile container, int imageIndex)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            if (container.Handle.IsClosed || container.Handle.IsInvalid)
                throw new ArgumentNullException("The handle to the WIM file has already been closed, or is invalid.", nameof(container));

            if (imageIndex > container.ImageCount)
                throw new ArgumentOutOfRangeException(nameof(imageIndex), "The index does not exist in the specified WIM file.");

            Handle = new WimImageHandle(container, imageIndex);
        }

        public WimImageHandle Handle { get; }
        public string ImageIndex => XmlInfo.Element("IMAGE").Attribute("INDEX").Value;
        public string ImageName => XmlInfo.XPathSelectElement("/IMAGE/NAME").Value;
        public string ImageEditionId => XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/EDITIONID").Value;
        public string ImageFlags => XmlInfo.XPathSelectElement("/IMAGE/FLAGS").Value;
        public string ImageProductType => XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/PRODUCTTYPE").Value;
        public string ImageInstallationType => XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/INSTALLATIONTYPE").Value;
        public string ImageDescription => XmlInfo.XPathSelectElement("/IMAGE/DESCRIPTION").Value;
        public ulong ImageSize => ulong.Parse(XmlInfo.XPathSelectElement("/IMAGE/TOTALBYTES").Value);
        public string ImageDefaultLanguage => lang = XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/LANGUAGES/DEFAULT")?.Value;
        public string ImageDisplayName => XmlInfo.XPathSelectElement("/IMAGE/DISPLAYNAME").Value;
        public string ImageDisplayDescription => XmlInfo.XPathSelectElement("/IMAGE/DISPLAYDESCRIPTION").Value;

        private XDocument XmlInfo
        {
            get
            {
                if (null == _xmlInfo)
                {
                    if (!NativeMethods.WimGetImageInformation(Handle, out StringBuilder builder, out uint bytes))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    // Ensure the length of the returned bytes to avoid garbage characters at the end.
                    int charCount = (int)bytes / sizeof(char);
                    if (null != builder)
                    {
                        // Get rid of the unicode file marker at the beginning of the XML.
                        builder.Remove(0, 1);
                        builder.EnsureCapacity(charCount - 1);
                        builder.Length = charCount - 1;

                        // This isn't likely to change while we have the image open, so cache it.
                        _xmlInfo = XDocument.Parse(builder.ToString().Trim());
                    }
                    else
                    {
                        _xmlInfo = null;
                    }
                }
                return _xmlInfo;
            }
        }

        public Architectures ImageArchitecture
        {
            get
            {
                try
                {
                    return (Architectures)int.Parse(XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/ARCH")?.Value);
                }
                catch
                {
                    return Architectures.Invalid;
                }
            }
        }

        public Version ImageVersion
        {
            get
            {
                int major = 0;
                int minor = 0;
                int build = 0;
                int revision = 0;

                try
                {
                    major = int.Parse(XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/VERSION/MAJOR").Value);
                    minor = int.Parse(XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/VERSION/MINOR").Value);
                    build = int.Parse(XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/VERSION/BUILD").Value);
                    revision = int.Parse(XmlInfo.XPathSelectElement("/IMAGE/WINDOWS/VERSION/SPBUILD").Value);
                }
                catch
                {
                }
                return new Version(major, minor, build, revision);
            }
        }

        public void Close()
        {
            if (!Handle.IsClosed && !Handle.IsInvalid)
            {
                Handle.Close();
            }
        }

        public void Apply(string applyToPath)
        {
            if (applyToPath == null)
                throw new ArgumentNullException(nameof(applyToPath));

            applyToPath = Path.GetFullPath(applyToPath);
            if (!Directory.Exists(applyToPath))
                throw new DirectoryNotFoundException("The WIM cannot be applied because the specified directory was not found.");

            if (!NativeMethods.WimApplyImage(Handle, applyToPath, NativeMethods.WimApplyFlags.WimApplyFlagsNone))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
