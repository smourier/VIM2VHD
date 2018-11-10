using System;

namespace VIM2VHD
{
    /// <summary>
    /// Contains virtual hard disk (VHD) creation flags.
    /// </summary>
    [Flags]
    public enum CREATE_VIRTUAL_DISK_FLAG
    {
        /// <summary>
        /// No flags. Use system defaults.
        /// </summary>
        CREATE_VIRTUAL_DISK_FLAG_NONE = 0x0,

        /// <summary>
        /// Pre-allocate all physical space necessary for the size of the virtual disk.
        /// </summary>
        /// <remarks>
        /// The CREATE_VIRTUAL_DISK_FLAG_FULL_PHYSICAL_ALLOCATION flag is used for the creation of a fixed VHD.
        /// </remarks>
        CREATE_VIRTUAL_DISK_FLAG_FULL_PHYSICAL_ALLOCATION = 0x1,

        /// <summary>
        /// Take ownership of the source disk during create from source disk, to insure the source disk does not change during the create operation.
        /// </summary>
        CREATE_VIRTUAL_DISK_FLAG_PREVENT_WRITES_TO_SOURCE_DISK = 0x2,

        /// <summary>
        /// Do not copy initial virtual disk metadata or block states from the parent VHD; this is useful if the parent VHD is a stand-in file and the real parent will be explicitly set later.
        /// </summary>
        CREATE_VIRTUAL_DISK_FLAG_DO_NOT_COPY_METADATA_FROM_PARENT = 0x4,
        CREATE_VIRTUAL_DISK_FLAG_CREATE_BACKING_STORAGE = 0x8,
        CREATE_VIRTUAL_DISK_FLAG_USE_CHANGE_TRACKING_SOURCE_LIMIT = 0x10,
        CREATE_VIRTUAL_DISK_FLAG_PRESERVE_PARENT_CHANGE_TRACKING_STATE = 0x20,
        CREATE_VIRTUAL_DISK_FLAG_VHD_SET_USE_ORIGINAL_BACKING_STORAGE = 0x40,
        CREATE_VIRTUAL_DISK_FLAG_SPARSE_FILE = 0x80,
        CREATE_VIRTUAL_DISK_FLAG_PMEM_COMPATIBLE = 0x100,
    }
}
