namespace VIM2VHD
{
    public enum WIM_MSG
    {
        WIM_MSG = NativeMethods.WM_APP + 0x1476, // 0x9476 / 38006

        WIM_MSG_TEXT,               // 0x9477

        ///<summary>
        /// Indicates an update in the progress of an image application.
        ///</summary>
        WIM_MSG_PROGRESS,           // 0x9478

        ///<summary>
        /// Enables the caller to prevent a file or a directory from being captured or applied.
        ///</summary>
        WIM_MSG_PROCESS,            // 0x9479

        ///<summary>
        /// Indicates that volume information is being gathered during an image capture.
        ///</summary>
        WIM_MSG_SCANNING,           // 0x947A

        ///<summary>
        /// Indicates the number of files that will be captured or applied.
        ///</summary>
        WIM_MSG_SETRANGE,           // 0x947B

        ///<summary>
        /// Indicates the number of files that have been captured or applied.
        ///</summary>
        WIM_MSG_SETPOS,             // 0x947C

        ///<summary>
        /// Indicates that a file has been either captured or applied.
        ///</summary>
        WIM_MSG_STEPIT,             // 0x947D

        ///<summary>
        /// Enables the caller to prevent a file resource from being compressed during a capture.
        ///</summary>
        WIM_MSG_COMPRESS,           // 0x947E

        ///<summary>
        /// Alerts the caller that an error has occurred while capturing or applying an image.
        ///</summary>
        WIM_MSG_ERROR,              // 0x947F

        ///<summary>
        /// Enables the caller to align a file resource on a particular alignment boundary.
        ///</summary>
        WIM_MSG_ALIGNMENT,          // 0x9480

        WIM_MSG_RETRY,              // 0x9481

        ///<summary>
        /// Enables the caller to align a file resource on a particular alignment boundary.
        ///</summary>
        WIM_MSG_SPLIT,              // 0x9482

        WIM_MSG_FILEINFO,           // 0x9483
        WIM_MSG_INFO,               // 0x9484
        WIM_MSG_WARNING,            // 0x9485
        WIM_MSG_CHK_PROCESS,        // 0x9486
        WIM_MSG_WARNING_OBJECTID,   // 0x9487
        WIM_MSG_STALE_MOUNT_DIR,    // 0x9488
        WIM_MSG_STALE_MOUNT_FILE,   // 0x9489
        WIM_MSG_MOUNT_CLEANUP_PROGRESS,     // 0x948A
        WIM_MSG_CLEANUP_SCANNING_DRIVE,     // 0x948B
        WIM_MSG_IMAGE_ALREADY_MOUNTED,      // 0x948C
        WIM_MSG_CLEANUP_UNMOUNTING_IMAGE,   // 0x948D
        WIM_MSG_QUERY_ABORT,                // 0x948E

        WIM_MSG_RESTORE_ALL_FILES_UNDOCUMENTED = 0x9490,
        WIM_MSG_CHECK_DATA_HASH_UNDOCUMENTED = 0x9491,
        WIM_MSG_RELEASE_DATA_RANGE_UNDOCUMENTED = 0x9492,
        WIM_MSG_VERIFY_INTEGRITY_CHUNK_UNDOCUMENTED = 0x9493,
        WIM_MSG_COPY_FILE_UNDOCUMENTED = 0x9494,
        WIM_MSG_CHECK_EXCLUDE_METADATA_UNDOCUMENTED = 0x9495,
        WIM_MSG_POPULATE_FIND_DATA_UNDOCUMENTED = 0x9496,
        WIM_MSG_GET_METADATA_PADDING_UNDOCUMENTED = 0x9497,
        WIM_MSG_RESTORE_REF_NODE_UNDOCUMENTED = 0x9498,
        WIM_MSG_CHECK_CIEA_SUPPORT_UNDOCUMENTED = 0x949A,
        WIM_MSG_CHECK_CIEA_SUPPORT2_UNDOCUMENTED = 0x949B,
    }
}
