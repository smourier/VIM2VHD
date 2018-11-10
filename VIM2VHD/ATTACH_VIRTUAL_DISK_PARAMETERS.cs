using System.Runtime.InteropServices;

namespace VIM2VHD
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ATTACH_VIRTUAL_DISK_PARAMETERS
    {
        public ATTACH_VIRTUAL_DISK_VERSION Version;
        public int Reserved;
    }
}
