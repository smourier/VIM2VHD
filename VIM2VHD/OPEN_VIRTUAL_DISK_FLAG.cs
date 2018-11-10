using System;

namespace VIM2VHD
{
    /// <summary>
    /// Contains virtual hard disk (VHD) open request flags.
    /// </summary>
    [Flags]
    public enum OPEN_VIRTUAL_DISK_FLAG
    {
        /// <summary>
        /// No flag specified.
        /// </summary>
        OPEN_VIRTUAL_DISK_FLAG_NONE = 0x0,

        /// <summary>
        /// Open the VHD file (backing store) without opening any differencing-chain parents. Used to correct broken parent links.
        /// </summary>
        OPEN_VIRTUAL_DISK_FLAG_NO_PARENTS = 0x1,

        /// <summary>
        /// Reserved. This flag is not supported for ISO virtual disks.
        /// </summary>
        OPEN_VIRTUAL_DISK_FLAG_BLANK_FILE = 0x2,

        /// <summary>
        /// Reserved. This flag is not supported for ISO virtual disks.
        /// </summary>
        OPEN_VIRTUAL_DISK_FLAG_BOOT_DRIVE = 0x4,

        /// <summary>
        /// Indicates that the virtual disk should be opened in cached mode. By default the virtual disks are opened using FILE_FLAG_NO_BUFFERING and FILE_FLAG_WRITE_THROUGH.
        /// </summary>
        OPEN_VIRTUAL_DISK_FLAG_CACHED_IO = 0x8,

        /// <summary>
        /// Indicates the VHD file is to be opened without opening any differencing-chain parents and the parent chain is to be created manually using the AddVirtualDiskParent function.
        /// </summary>
        OPEN_VIRTUAL_DISK_FLAG_CUSTOM_DIFF_CHAIN = 0x10,
        OPEN_VIRTUAL_DISK_FLAG_PARENT_CACHED_IO = 0x20,
        OPEN_VIRTUAL_DISK_FLAG_VHDSET_FILE_ONLY = 0x40,
        OPEN_VIRTUAL_DISK_FLAG_IGNORE_RELATIVE_PARENT_LOCATOR = 0x80,
        OPEN_VIRTUAL_DISK_FLAG_NO_WRITE_HARDENING = 0x100,
    }
}
