using System;

namespace VIM2VHD
{
    /// <summary>
    /// Contains virtual hard disk (VHD) creation flags.
    /// </summary>
    [Flags]
    public enum CreateVirtualDiskFlags
    {
        /// <summary>
        /// Contains virtual hard disk (VHD) creation flags.
        /// </summary>
        None = 0x00000000,
        /// <summary>
        /// Pre-allocate all physical space necessary for the size of the virtual disk.
        /// </summary>
        /// <remarks>
        /// The CREATE_VIRTUAL_DISK_FLAG_FULL_PHYSICAL_ALLOCATION flag is used for the creation of a fixed VHD.
        /// </remarks>
        FullPhysicalAllocation = 0x00000001
    }
}
