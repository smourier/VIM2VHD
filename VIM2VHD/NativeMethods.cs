using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace VIM2VHD
{
    /// <summary>
    /// P/Invoke methods and associated enums, flags, and structs.
    /// </summary>
    internal class NativeMethods
    {
        public static void RegisterMessageCallback(IntPtr hWim, WimMessageCallback callback)
        {
            WIMRegisterMessageCallback(hWim, callback, IntPtr.Zero);
            int gle = Marshal.GetLastWin32Error();
            if (gle != 0)
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to register message callback."), new Win32Exception(gle));
        }

        public static void UnregisterMessageCallback(IntPtr hWim, WimMessageCallback registeredCallback)
        {
            if (!WIMUnregisterMessageCallback(hWim, registeredCallback))
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to unregister message callback."), new Win32Exception(Marshal.GetLastWin32Error()));
        }

        /// <summary>
        /// The default depth in a VHD parent chain that this library will search through.
        /// If you want to go more than one disk deep into the parent chain, provide a different value.
        /// </summary>
        public const int OPEN_VIRTUAL_DISK_RW_DEFAULT_DEPTH = 0x00000001;

        public const int DEFAULT_BLOCK_SIZE = 0x00080000;
        public const int DISK_SECTOR_SIZE = 0x00000200;

        public const int ERROR_VIRTDISK_NOT_VIRTUAL_DISK = unchecked((int)0xC03A0015);
        public const int ERROR_NOT_FOUND = 0x00000490;
        public const int ERROR_IO_PENDING = 0x000003E5;
        public const int ERROR_INSUFFICIENT_BUFFER = 0x0000007A;
        public const int ERROR_ERROR_DEV_NOT_EXIST = 0x00000037;
        public const int ERROR_BAD_COMMAND = 0x00000016;
        public const int ERROR_SUCCESS = 0x00000000;

        public static Guid VirtualStorageTypeVendorUnknown = new Guid("00000000-0000-0000-0000-000000000000");
        public static Guid VirtualStorageTypeVendorMicrosoft = new Guid("EC984AEC-A0F9-47e9-901F-71415A66345B");

        public const int WIM_FLAG_VERIFY = 0x00000002;
        public const int WIM_FLAG_INDEX = 0x00000004;

        [Flags]
        public enum WimCreateFileDesiredAccess
        {
            WimQuery = 0x00000000,
            WimGenericRead = unchecked((int)0x80000000)
        }

        /// <summary>
        /// Specifies how the file is to be treated and what features are to be used.
        /// </summary>
        [Flags]
        public enum WimApplyFlags
        {
            /// <summary>
            /// No flags.
            /// </summary>
            WimApplyFlagsNone = 0x00000000,

            /// <summary>
            /// Reserved.
            /// </summary>
            WimApplyFlagsReserved = 0x00000001,
            
            /// <summary>
            /// Verifies that files match original data.
            /// </summary>
            WimApplyFlagsVerify = 0x00000002,
            
            /// <summary>
            /// Specifies that the image is to be sequentially read for caching or performance purposes.
            /// </summary>
            WimApplyFlagsIndex = 0x00000004,
            
            /// <summary>
            /// Applies the image without physically creating directories or files. Useful for obtaining a list of files and directories in the image.
            /// </summary>
            WimApplyFlagsNoApply = 0x00000008,
            
            /// <summary>
            /// Disables restoring security information for directories.
            /// </summary>
            WimApplyFlagsNoDirAcl = 0x00000010,
            
            /// <summary>
            /// Disables restoring security information for files
            /// </summary>
            WimApplyFlagsNoFileAcl = 0x00000020,
            
            /// <summary>
            /// The .wim file is opened in a mode that enables simultaneous reading and writing.
            /// </summary>
            WimApplyFlagsShareWrite = 0x00000040,
            
            /// <summary>
            /// Sends a WIM_MSG_FILEINFO message during the apply operation.
            /// </summary>
            WimApplyFlagsFileInfo = 0x00000080,
            
            /// <summary>
            /// Disables automatic path fixups for junctions and symbolic links.
            /// </summary>
            WimApplyFlagsNoRpFix = 0x00000100,
            
            /// <summary>
            /// Returns a handle that cannot commit changes, regardless of the access level requested at mount time.
            /// </summary>
            WimApplyFlagsMountReadOnly = 0x00000200,
            
            /// <summary>
            /// Reserved.
            /// </summary>
            WimApplyFlagsMountFast = 0x00000400,
            
            /// <summary>
            /// Reserved.
            /// </summary>
            WimApplyFlagsMountLegacy = 0x00000800
        }

        public enum WimCreationDisposition
        {
            WimOpenExisting = 0x00000003,
        }

        public enum WimActionFlags
        {
            WimIgnored = 0x00000000
        }

        public enum WimCompressionType
        {
            WimIgnored = 0x00000000
        }

        public enum WimCreationResult
        {
            WimCreatedNew = 0x00000000,
            WimOpenedExisting = 0x00000001
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SecurityDescriptor
        {
            public byte revision;
            public byte size;
            public short control;
            public IntPtr owner;
            public IntPtr group;
            public IntPtr sacl;
            public IntPtr dacl;
        }

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("virtdisk")]
        public static extern int CreateVirtualDisk(
            ref VIRTUAL_STORAGE_TYPE VirtualStorageType,
            [MarshalAs(UnmanagedType.LPWStr)] string Path,
            VIRTUAL_DISK_ACCESS_MASK VirtualDiskAccessMask,
            ref SecurityDescriptor SecurityDescriptor,
            CREATE_VIRTUAL_DISK_FLAG Flags,
            int ProviderSpecificFlags,
            ref CREATE_VIRTUAL_DISK_PARAMETERS Parameters,
            IntPtr Overlapped,
            out IntPtr Handle);

        [DllImport("virtdisk")]
        public static extern int OpenVirtualDisk(
            ref VIRTUAL_STORAGE_TYPE VirtualStorageType,
            [MarshalAs(UnmanagedType.LPWStr)] string Path,
            VIRTUAL_DISK_ACCESS_MASK VirtualDiskAccessMask,
            OPEN_VIRTUAL_DISK_FLAG Flags,
            ref OPEN_VIRTUAL_DISK_PARAMETERS Parameters,
            out IntPtr Handle);

        // GetVirtualDiskOperationProgress API allows getting progress info for the async virtual disk operations (ie. Online Mirror)
        [DllImport("virtdisk")]
        public static extern int GetVirtualDiskOperationProgress(IntPtr VirtualDiskHandle, IntPtr Overlapped, ref VIRTUAL_DISK_PROGRESS Progress);

        [DllImport("virtdisk")]
        public static extern int AttachVirtualDisk(
            IntPtr VirtualDiskHandle,
            ref SecurityDescriptor SecurityDescriptor,
            ATTACH_VIRTUAL_DISK_FLAG Flags,
            int ProviderSpecificFlags,
            ref ATTACH_VIRTUAL_DISK_PARAMETERS Parameters,
            IntPtr Overlapped);

        [DllImport("virtdisk")]
        public static extern int DetachVirtualDisk(IntPtr VirtualDiskHandle, DETACH_VIRTUAL_DISK_FLAG Flags, int ProviderSpecificFlags);

        [DllImport("virtdisk")]
        public static extern int CompactVirtualDisk(IntPtr VirtualDiskHandle, COMPACT_VIRTUAL_DISK_FLAG Flags, ref COMPACT_VIRTUAL_DISK_PARAMETERS Parameters, IntPtr Overlapped);

        [DllImport("virtdisk")]
        public static extern int GetVirtualDiskPhysicalPath(IntPtr VirtualDiskHandle, ref int DiskPathSizeInBytes, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder DiskPath);

        [DllImport("advapi32", SetLastError = true)]
        public static extern bool InitializeSecurityDescriptor(out SecurityDescriptor pSecurityDescriptor, int dwRevision);

        // CreateEvent API is used while calling async Online Mirror API
        [DllImport("kernel32")]
        public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, [MarshalAs(UnmanagedType.LPWStr)] string lpName);

        [DllImport("wimgapi", SetLastError = true)]
        public static extern IntPtr WIMCreateFile(
            [MarshalAs(UnmanagedType.LPWStr)] string WimPath,
            WimCreateFileDesiredAccess DesiredAccess,
            WimCreationDisposition CreationDisposition,
            WimActionFlags FlagsAndAttributes,
            WimCompressionType CompressionType,
            out WimCreationResult CreationResult
        );

        [DllImport("wimgapi", SetLastError = true)]
        public static extern bool WIMCloseHandle(IntPtr Handle);

        [DllImport("wimgapi", SetLastError = true)]
        public static extern IntPtr WIMLoadImage(IntPtr Handle, int ImageIndex);

        [DllImport("wimgapi")]
        public static extern int WIMGetImageCount(IntPtr Handle);

        //[DllImport("wimgapi", SetLastError = true)]
        //public static extern bool WIMApplyImage(WimImageHandle Handle, [MarshalAs(UnmanagedType.LPWStr)] string Path, WimApplyFlags Flags);

        [DllImport("wimgapi", SetLastError = true)]
        public static extern bool WIMApplyImage(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string Path, WimApplyFlags Flags);

        [DllImport("wimgapi", SetLastError = true)]
        public static extern bool WIMGetImageInformation(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] out StringBuilder ImageInfo, out int SizeOfImageInfo);

        [DllImport("wimgapi", SetLastError = true)]
        public static extern bool WIMSetTemporaryPath(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string TempPath);

        [DllImport("wimgapi", SetLastError = true)]
        public static extern int WIMRegisterMessageCallback(IntPtr Handle, WimMessageCallback MessageProc, IntPtr ImageInfo);

        [DllImport("wimgapi", SetLastError = true)]
        public static extern bool WIMUnregisterMessageCallback(IntPtr Handle, WimMessageCallback MessageProc);
    }
}
