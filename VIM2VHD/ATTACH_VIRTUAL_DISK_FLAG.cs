using System;

namespace VIM2VHD
{
    /// <summary>
    /// Contains virtual disk attach request flags.
    /// </summary>
    [Flags]
    public enum ATTACH_VIRTUAL_DISK_FLAG
    {
        /// <summary>
        /// No flags. Use system defaults.
        /// </summary>
        ATTACH_VIRTUAL_DISK_FLAG_NONE = 0x0,

        /// <summary>
        /// Attach the virtual disk as read-only.
        /// </summary>
        ATTACH_VIRTUAL_DISK_FLAG_READ_ONLY = 0x1,

        /// <summary>
        /// No drive letters are assigned to the disk's volumes.
        /// </summary>
        /// <remarks>Oddly enough, this doesn't apply to NTFS mount points.</remarks>
        ATTACH_VIRTUAL_DISK_FLAG_NO_DRIVE_LETTER = 0x2,

        /// <summary>
        /// Will decouple the virtual disk lifetime from that of the VirtualDiskHandle.
        /// The virtual disk will be attached until the Detach() function is called, even if all open handles to the virtual disk are closed.
        /// </summary>
        ATTACH_VIRTUAL_DISK_FLAG_PERMANENT_LIFETIME = 0x4,

        /// <summary>
        /// Reserved. This flag is not supported for ISO virtual disks.
        /// </summary>
        ATTACH_VIRTUAL_DISK_FLAG_NO_LOCAL_HOST = 0x8,
        ATTACH_VIRTUAL_DISK_FLAG_NO_SECURITY_DESCRIPTOR = 0x10,
        ATTACH_VIRTUAL_DISK_FLAG_BYPASS_DEFAULT_ENCRYPTION_POLICY = 0x20,
    }
}
