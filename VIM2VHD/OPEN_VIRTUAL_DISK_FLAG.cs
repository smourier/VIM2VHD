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
        ///  Open the backing store without opening any differencing chain parents. This allows one to fixup broken parent links.
        /// </summary>
        OPEN_VIRTUAL_DISK_FLAG_NO_PARENTS = 0x1,

        /// <summary>
        /// The backing store being opened is an empty file. Do not perform virtual disk verification.
        /// </summary>
        OPEN_VIRTUAL_DISK_FLAG_BLANK_FILE = 0x2,

        /// <summary>
        /// This flag is only specified at boot time to load the system disk during virtual disk boot. Must be kernel mode to specify this flag.
        /// </summary>
        OPEN_VIRTUAL_DISK_FLAG_BOOT_DRIVE = 0x4,

        /// <summary>
        /// This flag causes the backing file to be opened in cached mode.
        /// </summary>
        OPEN_VIRTUAL_DISK_FLAG_CACHED_IO = 0x8,

        /// <summary>
        /// Open the backing store without opening any differencing chain parents. This allows one to fixup broken parent links temporarily without updating the parent locator.
        /// </summary>
        OPEN_VIRTUAL_DISK_FLAG_CUSTOM_DIFF_CHAIN = 0x10,

        /// <summary>
        /// This flag causes all backing stores except the leaf backing store to be opened in cached mode.
        /// </summary>
        OPEN_VIRTUAL_DISK_FLAG_PARENT_CACHED_IO = 0x20,

        /// <summary>
        /// This flag causes a Vhd Set file to be opened without any virtual disk.
        /// </summary>
        OPEN_VIRTUAL_DISK_FLAG_VHDSET_FILE_ONLY = 0x40,

        /// <summary>
        /// For differencing disks, relative parent locators are not used when determining the path of a parent VHD.
        /// </summary>
        OPEN_VIRTUAL_DISK_FLAG_IGNORE_RELATIVE_PARENT_LOCATOR = 0x80,

        /// <summary>
        /// Disable flushing and FUA (both for payload data and for metadata) for backing files associated with this virtual disk.
        /// </summary>
        OPEN_VIRTUAL_DISK_FLAG_NO_WRITE_HARDENING = 0x100,
    }
}
