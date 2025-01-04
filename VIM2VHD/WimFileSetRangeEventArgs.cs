using System;

namespace VIM2VHD
{
    public class WimFileSetRangeEventArgs : WimFileEventArgs
    {
        internal WimFileSetRangeEventArgs(WIM_MSG message, IntPtr wParam, IntPtr lParam)
            : base(message, wParam, lParam)
        {
            FileCount = lParam.ToInt32();
        }

        public int FileCount { get; }
    }
}
