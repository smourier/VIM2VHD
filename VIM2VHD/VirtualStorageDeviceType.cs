namespace VIM2VHD
{
    /// <summary>
    /// Contains the type and provider (vendor) of the virtual storage device.
    /// </summary>
    public enum VirtualStorageDeviceType
    {
        /// <summary>
        /// The storage type is unknown or not valid.
        /// </summary>
        Unknown = 0x00000000,

        /// <summary>
        /// For internal use only.  This type is not supported.
        /// </summary>
        ISO = 0x00000001,
        
        /// <summary>
        /// Virtual Hard Disk device type.
        /// </summary>
        VHD = 0x00000002,
        
        /// <summary>
        /// Virtual Hard Disk v2 device type.
        /// </summary>
        VHDX = 0x00000003
    }
}
