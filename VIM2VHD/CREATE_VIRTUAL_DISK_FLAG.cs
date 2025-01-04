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
        /// Pre-allocate all physical space necessary for the virtual size of the disk (e.g. a fixed VHD).
        /// </summary>
        CREATE_VIRTUAL_DISK_FLAG_FULL_PHYSICAL_ALLOCATION = 0x1,

        /// <summary>
        /// Take ownership of the source disk during create from source disk, to
        /// insure the source disk does not change during the create operation.  The
        /// source disk must also already be offline or read-only (or both).
        /// Ownership is released when create is done.  This also has a side-effect
        /// of disallowing concurrent create from same source disk.  Create will fail
        /// if ownership cannot be obtained or if the source disk is not already
        /// offline or read-only.  This flag is optional, but highly recommended for
        /// creates from source disk.  No effect for other types of create (no effect
        /// for create from source VHD; no effect for create without SourcePath).
        /// </summary>
        CREATE_VIRTUAL_DISK_FLAG_PREVENT_WRITES_TO_SOURCE_DISK = 0x2,

        /// <summary>
        /// Do not copy initial virtual disk metadata or block states from the
        /// parent VHD; this is useful if the parent VHD is a stand-in file and the
        /// real parent will be explicitly set later.
        /// </summary>
        CREATE_VIRTUAL_DISK_FLAG_DO_NOT_COPY_METADATA_FROM_PARENT = 0x4,

        /// <summary>
        /// Create the backing storage disk.
        /// </summary>
        CREATE_VIRTUAL_DISK_FLAG_CREATE_BACKING_STORAGE = 0x8,

        /// <summary>
        /// If set, the SourceLimitPath is an change tracking ID, and all data that has changed
        /// since that change tracking ID will be copied from the source. If clear, the
        /// SourceLimitPath is a VHD file path in the source VHD's chain, and
        /// all data that is present in the children of that VHD in the chain
        /// will be copied from the source.
        /// </summary>
        CREATE_VIRTUAL_DISK_FLAG_USE_CHANGE_TRACKING_SOURCE_LIMIT = 0x10,

        /// <summary>
        /// If set and the parent VHD has change tracking enabled, the child will
        /// have change tracking enabled and will recognize all change tracking
        /// IDs that currently exist in the parent. If clear or if the parent VHD
        /// does not have change tracking available, then change tracking will
        /// not be enabled in the new VHD.
        /// </summary>
        CREATE_VIRTUAL_DISK_FLAG_PRESERVE_PARENT_CHANGE_TRACKING_STATE = 0x20,

        /// <summary>
        /// When creating a VHD Set from source, don't copy the data in the original
        /// backing store, but intsead use the file as is. If this flag is not specified
        /// and a source file is passed to CreateVirtualDisk for a VHDSet file, the data
        /// in the source file is copied. If this flag is set the data is moved. The
        /// name of the file may change.
        /// </summary>
        CREATE_VIRTUAL_DISK_FLAG_VHD_SET_USE_ORIGINAL_BACKING_STORAGE = 0x40,

        /// <summary>
        /// When creating a fixed virtual disk, take advantage of an underlying sparse file.
        /// Only supported on file systems that support sparse VDLs.
        /// </summary>
        CREATE_VIRTUAL_DISK_FLAG_SPARSE_FILE = 0x80,

        /// <summary>
        /// Creates a VHD suitable as the backing store for a virtual persistent memory device.
        /// </summary>
        CREATE_VIRTUAL_DISK_FLAG_PMEM_COMPATIBLE = 0x100,
    }
}
