using System;

namespace VIM2VHD
{
    public class WimFileProgressEventArgs : WimFileEventArgs
    {
        internal WimFileProgressEventArgs(WIM_MSG message, IntPtr wParam, IntPtr lParam)
            : base(message, wParam, lParam)
        {
            Percent = wParam.ToInt32();
            MillisecondsRemaining = LParam.ToInt32();
        }

        public int Percent { get; }
        public int MillisecondsRemaining { get; }
    }
}
