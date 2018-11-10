﻿using System;

namespace VIM2VHD
{
    /// <summary>
    /// Contains the bit mask for specifying access rights to a virtual hard disk (VHD).
    /// </summary>
    [Flags]
    public enum VirtualDiskAccessMask
    {
        /// <summary>
        /// Only Version2 of OpenVirtualDisk API accepts this parameter
        /// </summary>
        None = 0x00000000,

        /// <summary>
        /// Open the virtual disk for read-only attach access. The caller must have READ access to the virtual disk image file.
        /// </summary>
        /// <remarks>
        /// If used in a request to open a virtual disk that is already open, the other handles must be limited to either
        /// VIRTUAL_DISK_ACCESS_DETACH or VIRTUAL_DISK_ACCESS_GET_INFO access, otherwise the open request with this flag will fail.
        /// </remarks>
        AttachReadOnly = 0x00010000,
        
        /// <summary>
        /// Open the virtual disk for read-write attaching access. The caller must have (READ | WRITE) access to the virtual disk image file.
        /// </summary>
        /// <remarks>
        /// If used in a request to open a virtual disk that is already open, the other handles must be limited to either
        /// VIRTUAL_DISK_ACCESS_DETACH or VIRTUAL_DISK_ACCESS_GET_INFO access, otherwise the open request with this flag will fail.
        /// If the virtual disk is part of a differencing chain, the disk for this request cannot be less than the readWriteDepth specified
        /// during the prior open request for that differencing chain.
        /// </remarks>
        AttachReadWrite = 0x00020000,
        
        /// <summary>
        /// Open the virtual disk to allow detaching of an attached virtual disk. The caller must have
        /// (FILE_READ_ATTRIBUTES | FILE_READ_DATA) access to the virtual disk image file.
        /// </summary>
        Detach = 0x00040000,
        
        /// <summary>
        /// Information retrieval access to the virtual disk. The caller must have READ access to the virtual disk image file.
        /// </summary>
        GetInfo = 0x00080000,
        
        /// <summary>
        /// Virtual disk creation access.
        /// </summary>
        Create = 0x00100000,
        
        /// <summary>
        /// Open the virtual disk to perform offline meta-operations. The caller must have (READ | WRITE) access to the virtual
        /// disk image file, up to readWriteDepth if working with a differencing chain.
        /// </summary>
        /// <remarks>
        /// If the virtual disk is part of a differencing chain, the backing store (host volume) is opened in RW exclusive mode up to readWriteDepth.
        /// </remarks>
        MetaOperations = 0x00200000,
        
        /// <summary>
        /// Reserved.
        /// </summary>
        Read = 0x000D0000,
        
        /// <summary>
        /// Allows unrestricted access to the virtual disk. The caller must have unrestricted access rights to the virtual disk image file.
        /// </summary>
        All = 0x003F0000,
        
        /// <summary>
        /// Reserved.
        /// </summary>
        Writable = 0x00320000
    }
}
