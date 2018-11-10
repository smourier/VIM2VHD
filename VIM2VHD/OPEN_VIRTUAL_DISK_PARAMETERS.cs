using System;
using System.Runtime.InteropServices;

namespace VIM2VHD
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct OPEN_VIRTUAL_DISK_PARAMETERS
    {
        public OPEN_VIRTUAL_DISK_VERSION Version;
        public bool GetInfoOnly;
        public Guid ResiliencyGuid;
    }
}
