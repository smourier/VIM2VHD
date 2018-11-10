using System;
using System.Runtime.InteropServices;

namespace VIM2VHD
{
    public sealed class WimImageHandle : SafeHandle
    {
        public WimImageHandle(WimFile container, int imageIndex)
            : base(IntPtr.Zero, true)
        {
            if (null == container)
                throw new ArgumentNullException(nameof(container));

            if (container.Handle.IsClosed || container.Handle.IsInvalid)
                throw new ArgumentNullException("The handle to the WIM file has already been closed, or is invalid.", nameof(container));

            if (imageIndex > container.ImageCount)
                throw new ArgumentOutOfRangeException(nameof(imageIndex), "The index does not exist in the specified WIM file.");

            handle = NativeMethods.WimLoadImage(container.Handle.DangerousGetHandle(), imageIndex);
        }

        protected override bool ReleaseHandle() => NativeMethods.WimCloseHandle(this.handle);
        public override bool IsInvalid => handle == IntPtr.Zero;
    }
}
