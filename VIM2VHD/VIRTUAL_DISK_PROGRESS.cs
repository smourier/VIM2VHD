using System.Runtime.InteropServices;

namespace VIM2VHD
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct VIRTUAL_DISK_PROGRESS
    {
        public int OperationStatus;
        public ulong CurrentValue;
        public ulong CompletionValue;
    }
}
