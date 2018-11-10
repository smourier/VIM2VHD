using System.Runtime.InteropServices;

namespace VIM2VHD
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CompactVirtualDiskParameters
    {
        public CompactVirtualDiskVersion Version;
        public int Reserved;
    }
}
