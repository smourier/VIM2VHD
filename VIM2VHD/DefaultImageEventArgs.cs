using System;

namespace VIM2VHD
{
    ///<summary>
    ///Describes the file that is being processed for the ProcessFileEvent.
    ///</summary>
    public class DefaultImageEventArgs : EventArgs
    {
        public DefaultImageEventArgs(IntPtr wParam, IntPtr lParam, IntPtr userData)
        {
            WParam = wParam;
            LParam = lParam;
            UserData = userData;
        }

        public IntPtr WParam { get; }
        public IntPtr LParam { get; }
        public IntPtr UserData { get; }
    }
}
