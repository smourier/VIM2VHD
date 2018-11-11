using System;

namespace VIM2VHD
{
    public class WimFileEventArgs : EventArgs
    {
        internal WimFileEventArgs(WIM_MSG message, IntPtr wParam, IntPtr lParam)
        {
            Message = message;
            WParam = wParam;
            LParam = lParam;
        }

        public WIM_MSG Message { get; }
        public IntPtr WParam { get; }
        public IntPtr LParam { get; }
        public WIM_MSG_RETURN ReturnValue { get; set; }
    }
}
