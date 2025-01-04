namespace VIM2VHD
{
    /// <summary>
    /// Contains the type and provider (vendor) of the virtual storage device.
    /// </summary>
    public enum VIRTUAL_STORAGE_TYPE_DEVICE
    {
        /// <summary>
        /// The storage type is unknown or not valid.
        /// </summary>
        VIRTUAL_STORAGE_TYPE_DEVICE_UNKNOWN = 0,

        /// <summary>
        /// For internal use only.  This type is not supported.
        /// </summary>
        VIRTUAL_STORAGE_TYPE_DEVICE_ISO = 1,

        /// <summary>
        /// Virtual Hard Disk device type.
        /// </summary>
        VIRTUAL_STORAGE_TYPE_DEVICE_VHD = 2,

        /// <summary>
        /// Virtual Hard Disk v2 device type.
        /// </summary>
        VIRTUAL_STORAGE_TYPE_DEVICE_VHDX = 3,

        VIRTUAL_STORAGE_TYPE_DEVICE_VHDSET = 4
    }
}
