using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace VIM2VHD
{
    public sealed class WimFileHandle : SafeHandle
    {
        public WimFileHandle(string wimPath)
            : base(IntPtr.Zero, true)
        {
            if (string.IsNullOrEmpty(wimPath))
                throw new ArgumentNullException(nameof(wimPath));

            if (!File.Exists(Path.GetFullPath(wimPath)))
                throw new FileNotFoundException(new FileNotFoundException().Message, wimPath);

            handle = NativeMethods.WimCreateFile(
                wimPath,
                NativeMethods.WimCreateFileDesiredAccess.WimGenericRead,
                NativeMethods.WimCreationDisposition.WimOpenExisting,
                NativeMethods.WimActionFlags.WimIgnored,
                NativeMethods.WimCompressionType.WimIgnored,
                out var creationResult
            );
            if (handle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            if (creationResult != NativeMethods.WimCreationResult.WimOpenedExisting)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            NativeMethods.WimSetTemporaryPath(this, Environment.ExpandEnvironmentVariables("%TEMP%"));
        }

        protected override bool ReleaseHandle() => NativeMethods.WimCloseHandle(handle);
        public override bool IsInvalid => handle == IntPtr.Zero;
    }
}