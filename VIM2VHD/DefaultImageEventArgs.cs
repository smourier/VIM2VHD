using System;

namespace VIM2VHD
{
    ///<summary>
    ///Describes the file that is being processed for the ProcessFileEvent.
    ///</summary>
    public class
    DefaultImageEventArgs : EventArgs
    {
        ///<summary>
        ///Default constructor.
        ///</summary>
        public
        DefaultImageEventArgs(
            IntPtr wideParameter,
            IntPtr leftParameter,
            IntPtr userData)
        {

            WideParameter = wideParameter;
            LeftParameter = leftParameter;
            UserData = userData;
        }

        ///<summary>
        ///wParam
        ///</summary>
        public IntPtr WideParameter
        {
            get;
            private set;
        }

        ///<summary>
        ///lParam
        ///</summary>
        public IntPtr LeftParameter
        {
            get;
            private set;
        }

        ///<summary>
        ///UserData
        ///</summary>
        public IntPtr UserData
        {
            get;
            private set;
        }
    }
}
