﻿using System;

namespace VIM2VHD
{
    [Flags]
    public enum COMPACT_VIRTUAL_DISK_FLAG
    {
        /// <summary>
        /// No flags. Use system defaults.
        /// </summary>
        COMPACT_VIRTUAL_DISK_FLAG_NONE = 0x00000000,
        COMPACT_VIRTUAL_DISK_FLAG_NO_ZERO_SCAN = 0x00000001,
        COMPACT_VIRTUAL_DISK_FLAG_NO_BLOCK_MOVES = 0x00000002
    }
}