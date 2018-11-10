using System;

namespace VIM2VHD
{
    /// <summary>
    /// Contains virtual disk attach request flags.
    /// </summary>
    [Flags]
    public enum AttachVirtualDiskFlags
    {
        /// <summary>
        /// No flags. Use system defaults.
        /// </summary>
        None = 0x00000000,

        /// <summary>
        /// Attach the virtual disk as read-only.
        /// </summary>
        ReadOnly = 0x00000001,
        
        /// <summary>
        /// No drive letters are assigned to the disk's volumes.
        /// </summary>
        /// <remarks>Oddly enough, this doesn't apply to NTFS mount points.</remarks>
        NoDriveLetter = 0x00000002,
        
        /// <summary>
        /// Will decouple the virtual disk lifetime from that of the VirtualDiskHandle.
        /// The virtual disk will be attached until the Detach() function is called, even if all open handles to the virtual disk are closed.
        /// </summary>
        PermanentLifetime = 0x00000004,
        
        /// <summary>
        /// Reserved.
        /// </summary>
        NoLocalHost = 0x00000008
    }
}
