using System;
using System.Runtime.InteropServices;

namespace VIM2VHD
{
    public class WimFileProcessEventArgs : WimFileEventArgs
    {
        internal WimFileProcessEventArgs(WimFile file, WIM_MSG message, IntPtr wParam, IntPtr lParam)
            : base(message, wParam, lParam)
        {
            Path = Marshal.PtrToStringUni(wParam);
            RelativePath = file.GetRelativePath(Path);
        }

        public string Path { get; }
        public string RelativePath { get; }
        public bool Process { get => Marshal.ReadInt32(LParam) != 0; set => Marshal.WriteInt32(LParam, value ? 1 : 0); }
    }
}
