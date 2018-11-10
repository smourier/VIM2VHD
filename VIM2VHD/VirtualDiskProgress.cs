using System.Runtime.InteropServices;

namespace VIM2VHD
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct VirtualDiskProgress
    {
        public int OperationStatus;
        public ulong CurrentValue;
        public ulong CompletionValue;
    }
}
