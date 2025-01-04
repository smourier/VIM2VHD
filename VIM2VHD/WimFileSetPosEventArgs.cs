using System;

namespace VIM2VHD
{
    public class WimFileSetPosEventArgs : WimFileEventArgs
    {
        internal WimFileSetPosEventArgs(WIM_MSG message, IntPtr wParam, IntPtr lParam)
            : base(message, wParam, lParam)
        {
            FileCount = lParam.ToInt32();
        }

        public int FileCount { get; }
    }
}
