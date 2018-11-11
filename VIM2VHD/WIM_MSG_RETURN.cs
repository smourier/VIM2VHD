namespace VIM2VHD
{
    public enum WIM_MSG_RETURN
    {
        /// <summary>
        /// Used to indicate success and to enable other subscribers to process the message.
        /// </summary>
        WIM_MSG_SUCCESS = NativeMethods.ERROR_SUCCESS,

        /// <summary>
        /// Used to indicate success and to prevent other subscribers from receiving the message.
        /// </summary>
        WIM_MSG_DONE = unchecked((int)0xFFFFFFF0),

        WIM_MSG_SKIP_ERROR = unchecked((int)0xFFFFFFFE),

        /// <summary>
        /// Used to cancel an image apply or image capture.
        /// Do not use WIM_MSG_ABORT_IMAGE to cancel the process as a shortcut method of extracting a single file.
        /// Windows® 7 Imaging API is multi-threaded and aborting a process will cancel all background threads, which may include the single file you want to extract.
        /// </summary>
        WIM_MSG_ABORT_IMAGE = unchecked((int)0xFFFFFFFF)
    }
}
