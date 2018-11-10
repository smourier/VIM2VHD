namespace VIM2VHD
{
    /// <summary>
    /// Contains the version of the virtual hard disk (VHD) ATTACH_VIRTUAL_DISK_PARAMETERS structure to use in calls to VHD functions.
    /// </summary>
    public enum ATTACH_VIRTUAL_DISK_VERSION
    {
        ATTACH_VIRTUAL_DISK_VERSION_UNSPECIFIED = 0x00000000,
        ATTACH_VIRTUAL_DISK_VERSION_1 = 0x00000001,
        ATTACH_VIRTUAL_DISK_VERSION_2 = 0x00000002
    }
}
