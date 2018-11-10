using System;
using System.Runtime.InteropServices;

namespace VIM2VHD
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct OpenVirtualDiskParameters
    {
        public OpenVirtualDiskVersion Version;
        public bool GetInfoOnly;
        public Guid ResiliencyGuid;
    }
}
