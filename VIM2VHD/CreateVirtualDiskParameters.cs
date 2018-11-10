using System;
using System.Runtime.InteropServices;

namespace VIM2VHD
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CreateVirtualDiskParameters
    {
        /// <summary>
        /// A CREATE_VIRTUAL_DISK_VERSION enumeration that specifies the version of the CREATE_VIRTUAL_DISK_PARAMETERS structure being passed to or from the virtual hard disk (VHD) functions.
        /// </summary>
        public CreateVirtualDiskVersion Version;

        /// <summary>
        /// Unique identifier to assign to the virtual disk object. If this member is set to zero, a unique identifier is created by the system.
        /// </summary>
        public Guid UniqueId;

        /// <summary>
        /// The maximum virtual size of the virtual disk object. Must be a multiple of 512.
        /// If a ParentPath is specified, this value must be zero.
        /// If a SourcePath is specified, this value can be zero to specify the size of the source VHD to be used, otherwise the size specified must be greater than or equal to the size of the source disk.
        /// </summary>
        public ulong MaximumSize;

        /// <summary>
        /// Internal size of the virtual disk object blocks.
        /// The following are predefined block sizes and their behaviors. For a fixed VHD type, this parameter must be zero.
        /// </summary>
        public uint BlockSizeInBytes;

        /// <summary>
        /// Internal size of the virtual disk object sectors. Must be set to 512.
        /// </summary>
        public uint SectorSizeInBytes;

        /// <summary>
        /// Optional path to a parent virtual disk object. Associates the new virtual disk with an existing virtual disk.
        /// If this parameter is not NULL, SourcePath must be NULL.
        /// </summary>
        public string ParentPath;

        /// <summary>
        /// Optional path to pre-populate the new virtual disk object with block data from an existing disk. This path may refer to a VHD or a physical disk.
        /// If this parameter is not NULL, ParentPath must be NULL.
        /// </summary>
        public string SourcePath;

        /// <summary>
        /// Flags for opening the VHD
        /// </summary>
        public OpenVirtualDiskFlags OpenFlags;

        /// <summary>
        /// GetInfoOnly flag for V2 handles
        /// </summary>
        public bool GetInfoOnly;

        /// <summary>
        /// Virtual Storage Type of the parent disk
        /// </summary>
        public VirtualStorageType ParentVirtualStorageType;

        /// <summary>
        /// Virtual Storage Type of the source disk
        /// </summary>
        public VirtualStorageType SourceVirtualStorageType;

        /// <summary>
        /// A GUID to use for fallback resiliency over SMB.
        /// </summary>
        public Guid ResiliencyGuid;
    }
}
