﻿using System.Runtime.InteropServices;

namespace VIM2VHD
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct AttachVirtualDiskParameters
    {
        public AttachVirtualDiskVersion Version;
        public int Reserved;
    }
}