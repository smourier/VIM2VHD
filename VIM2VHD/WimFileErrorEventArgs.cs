﻿using System;
using System.Runtime.InteropServices;

namespace VIM2VHD
{
    public class WimFileErrorEventArgs : WimFileEventArgs
    {
        internal WimFileErrorEventArgs(WimFile file, WIM_MSG message, IntPtr wParam, IntPtr lParam)
            : base(message, wParam, lParam)
        {
            Path = Marshal.PtrToStringUni(wParam);
            RelativePath = file.GetRelativePath(Path);
            ErrorCode = lParam.ToInt32();
        }

        public string Path { get; }
        public string RelativePath { get; }
        public int ErrorCode { get; }
    }
}
