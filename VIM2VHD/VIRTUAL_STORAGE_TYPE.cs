using System;
using System.Runtime.InteropServices;

namespace VIM2VHD
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct VIRTUAL_STORAGE_TYPE
    {
        public VIRTUAL_STORAGE_TYPE_DEVICE DeviceId;
        public Guid VendorId;
    }
}
