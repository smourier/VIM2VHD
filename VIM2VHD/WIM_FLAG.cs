using System;

namespace VIM2VHD
{
    /// <summary>
    /// Specifies how the file is to be treated and what features are to be used.
    /// </summary>
    [Flags]
    public enum WIM_FLAG
    {
        /// <summary>
        /// None
        /// </summary>
        WIM_FLAG_NONE = 0x00000000,

        /// <summary>
        /// Reserved.
        /// </summary>
        WIM_FLAG_RESERVED = 0x00000001,

        /// <summary>
        /// Verifies that files match original data.
        /// </summary>
        WIM_FLAG_VERIFY = 0x00000002,

        /// <summary>
        /// Specifies that the image is to be sequentially read for caching or performance purposes.
        /// </summary>
        WIM_FLAG_INDEX = 0x00000004,

        /// <summary>
        /// Applies the image without physically creating directories or files. Useful for obtaining a list of files and directories in the image.
        /// </summary>
        WIM_FLAG_NO_APPLY = 0x00000008,

        /// <summary>
        /// Disables restoring security information for directories.
        /// </summary>
        WIM_FLAG_NO_DIRACL = 0x00000010,

        /// <summary>
        /// Disables restoring security information for files.
        /// </summary>
        WIM_FLAG_NO_FILEACL = 0x00000020,

        /// <summary>
        /// The .wim file is opened in a mode that enables simultaneous reading and writing.
        /// </summary>
        WIM_FLAG_SHARE_WRITE = 0x00000040,

        /// <summary>
        /// Sends a WIM_MSG_FILEINFO message during the apply operation.
        /// </summary>
        WIM_FLAG_FILEINFO = 0x00000080,

        /// <summary>
        /// Disables automatic path fixups for junctions and symbolic links.
        /// </summary>
        WIM_FLAG_NO_RP_FIX = 0x00000100,

        /// <summary>
        /// Returns a handle that cannot commit changes, regardless of the access level requested at mount time.
        /// </summary>
        WIM_FLAG_MOUNT_READONLY = 0x00000200
    }
}
