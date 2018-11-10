using System;

namespace VIM2VHD
{
    [Flags]
    public enum CompactVirtualDiskFlags
    {
        None = 0x00000000,
        NoZeroScan = 0x00000001,
        NoBlockMoves = 0x00000002
    }
}
