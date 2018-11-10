namespace VIM2VHD
{
    public enum WimMessage
    {
        WM_APP = 0x00008000,

        WIM_MSG = WM_APP + 0x1476,

        WIM_MSG_TEXT,

        ///<summary>
        ///Indicates an update in the progress of an image application.
        ///</summary>
        WIM_MSG_PROGRESS,

        ///<summary>
        ///Enables the caller to prevent a file or a directory from being captured or applied.
        ///</summary>
        WIM_MSG_PROCESS,

        ///<summary>
        ///Indicates that volume information is being gathered during an image capture.
        ///</summary>
        WIM_MSG_SCANNING,

        ///<summary>
        ///Indicates the number of files that will be captured or applied.
        ///</summary>
        WIM_MSG_SETRANGE,

        ///<summary>
        ///Indicates the number of files that have been captured or applied.
        ///</summary>
        WIM_MSG_SETPOS,

        ///<summary>
        ///Indicates that a file has been either captured or applied.
        ///</summary>
        WIM_MSG_STEPIT,

        ///<summary>
        ///Enables the caller to prevent a file resource from being compressed during a capture.
        ///</summary>
        WIM_MSG_COMPRESS,

        ///<summary>
        ///Alerts the caller that an error has occurred while capturing or applying an image.
        ///</summary>
        WIM_MSG_ERROR,

        ///<summary>
        ///Enables the caller to align a file resource on a particular alignment boundary.
        ///</summary>
        WIM_MSG_ALIGNMENT,

        WIM_MSG_RETRY,

        ///<summary>
        ///Enables the caller to align a file resource on a particular alignment boundary.
        ///</summary>
        WIM_MSG_SPLIT,
        WIM_MSG_SUCCESS = 0x00000000,
        WIM_MSG_ABORT_IMAGE = unchecked((int)0xFFFFFFFF)
    }
}
