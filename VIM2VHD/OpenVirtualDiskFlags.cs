using System;

namespace VIM2VHD
{
    /// <summary>
    /// Contains virtual hard disk (VHD) open request flags.
    /// </summary>
    [Flags]
    public enum OpenVirtualDiskFlags
    {
        /// <summary>
        /// No flags. Use system defaults.
        /// </summary>
        None = 0x00000000,
        /// <summary>
        /// Open the VHD file (backing store) without opening any differencing-chain parents. Used to correct broken parent links.
        /// </summary>
        NoParents = 0x00000001,
        /// <summary>
        /// Reserved.
        /// </summary>
        BlankFile = 0x00000002,
        /// <summary>
        /// Reserved.
        /// </summary>
        BootDrive = 0x00000004,
    }
}
