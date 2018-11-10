namespace VIM2VHD
{
    /// <summary>
    /// Contains the version of the virtual hard disk (VHD) ATTACH_VIRTUAL_DISK_PARAMETERS structure to use in calls to VHD functions.
    /// </summary>
    public enum AttachVirtualDiskVersion
    {
        VersionUnspecified = 0x00000000,
        Version1 = 0x00000001,
        Version2 = 0x00000002
    }
}
