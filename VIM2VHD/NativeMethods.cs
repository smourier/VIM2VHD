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
    public class NativeMethods
    {
        public static void RegisterMessageCallback(WimFileHandle hWim, WimMessageCallback callback)
        {
            WimRegisterMessageCallback(hWim, callback, IntPtr.Zero);
            int gle = Marshal.GetLastWin32Error();
            if (gle != 0)
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to register message callback."), new Win32Exception(gle));
        }

        public static void UnregisterMessageCallback(WimFileHandle hWim, WimMessageCallback registeredCallback)
        {
            if (!WimUnregisterMessageCallback(hWim, registeredCallback))
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to unregister message callback."), new Win32Exception(Marshal.GetLastWin32Error()));
        }

        /// <summary>
        /// The default depth in a VHD parent chain that this library will search through.
        /// If you want to go more than one disk deep into the parent chain, provide a different value.
        /// </summary>
        public const uint OPEN_VIRTUAL_DISK_RW_DEFAULT_DEPTH = 0x00000001;

        public const uint DEFAULT_BLOCK_SIZE = 0x00080000;
        public const uint DISK_SECTOR_SIZE = 0x00000200;

        internal const int ERROR_VIRTDISK_NOT_VIRTUAL_DISK = unchecked((int)0xC03A0015);
        internal const int ERROR_NOT_FOUND = 0x00000490;
        internal const int ERROR_IO_PENDING = 0x000003E5;
        internal const int ERROR_INSUFFICIENT_BUFFER = 0x0000007A;
        internal const int ERROR_ERROR_DEV_NOT_EXIST = 0x00000037;
        internal const int ERROR_BAD_COMMAND = 0x00000016;
        internal const int ERROR_SUCCESS = 0x00000000;

        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const short FILE_ATTRIBUTE_NORMAL = 0x00000080;
        public const uint CREATE_NEW = 0x00000001;
        public const uint CREATE_ALWAYS = 0x00000002;
        public const uint OPEN_EXISTING = 0x00000003;
        public const short INVALID_HANDLE_VALUE = -1;

        internal static Guid VirtualStorageTypeVendorUnknown = new Guid("00000000-0000-0000-0000-000000000000");
        internal static Guid VirtualStorageTypeVendorMicrosoft = new Guid("EC984AEC-A0F9-47e9-901F-71415A66345B");

        public const uint WIM_FLAG_VERIFY = 0x00000002;
        public const uint WIM_FLAG_INDEX = 0x00000004;


        [Flags]
        internal enum WimCreateFileDesiredAccess
        {
            WimQuery = 0x00000000,
            WimGenericRead = unchecked((int)0x80000000)
        }

        /// <summary>
        /// Specifies how the file is to be treated and what features are to be used.
        /// </summary>
        [Flags]
        internal enum WimApplyFlags
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

        internal enum WimCreationDisposition
        {
            WimOpenExisting = 0x00000003,
        }

        internal enum WimActionFlags
        {
            WimIgnored = 0x00000000
        }

        internal enum WimCompressionType
        {
            WimIgnored = 0x00000000
        }

        internal enum WimCreationResult
        {
            WimCreatedNew = 0x00000000,
            WimOpenedExisting = 0x00000001
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
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

        [DllImport("virtdisk", CharSet = CharSet.Unicode)]
        public static extern uint CreateVirtualDisk(
            [In, Out] ref VirtualStorageType VirtualStorageType,
            [In]          string Path,
            [In]          VirtualDiskAccessMask VirtualDiskAccessMask,
            [In, Out] ref SecurityDescriptor SecurityDescriptor,
            [In]          CreateVirtualDiskFlags Flags,
            [In]          uint ProviderSpecificFlags,
            [In, Out] ref CreateVirtualDiskParameters Parameters,
            [In]          IntPtr Overlapped,
            [Out]     out SafeFileHandle Handle);

        [DllImport("virtdisk", CharSet = CharSet.Unicode)]
        internal static extern uint OpenVirtualDisk(
            [In, Out] ref VirtualStorageType VirtualStorageType,
            [In]          string Path,
            [In]          VirtualDiskAccessMask VirtualDiskAccessMask,
            [In]          OpenVirtualDiskFlags Flags,
            [In, Out] ref OpenVirtualDiskParameters Parameters,
            [Out]     out SafeFileHandle Handle);

        /// <summary>
        /// GetVirtualDiskOperationProgress API allows getting progress info for the async virtual disk operations (ie. Online Mirror)
        /// </summary>
        /// <param name="VirtualDiskHandle"></param>
        /// <param name="Overlapped"></param>
        /// <param name="Progress"></param>
        /// <returns></returns>
        [DllImport("virtdisk", CharSet = CharSet.Unicode)]
        internal static extern uint GetVirtualDiskOperationProgress(
            [In]          SafeFileHandle VirtualDiskHandle,
            [In]          IntPtr Overlapped,
            [In, Out] ref VirtualDiskProgress Progress);

        [DllImport("virtdisk", CharSet = CharSet.Unicode)]
        public static extern uint AttachVirtualDisk(
            [In]          SafeFileHandle VirtualDiskHandle,
            [In, Out] ref SecurityDescriptor SecurityDescriptor,
            [In]          AttachVirtualDiskFlags Flags,
            [In]          uint ProviderSpecificFlags,
            [In, Out] ref AttachVirtualDiskParameters Parameters,
            [In]          IntPtr Overlapped);

        [DllImport("virtdisk")]
        public static extern int DetachVirtualDisk(SafeFileHandle VirtualDiskHandle, DetachVirtualDiskFlag Flags, uint ProviderSpecificFlags);

        [DllImport("virtdisk")]
        public static extern int CompactVirtualDisk(SafeFileHandle VirtualDiskHandle, CompactVirtualDiskFlags Flags, ref CompactVirtualDiskParameters Parameters, IntPtr Overlapped);

        [DllImport("virtdisk")]
        public static extern int GetVirtualDiskPhysicalPath(SafeFileHandle VirtualDiskHandle, ref int DiskPathSizeInBytes, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder DiskPath);

        [DllImport("advapi32", SetLastError = true)]
        public static extern bool InitializeSecurityDescriptor(out SecurityDescriptor pSecurityDescriptor, uint dwRevision);

        /// <summary>
        /// CreateEvent API is used while calling async Online Mirror API
        /// </summary>
        /// <param name="lpEventAttributes"></param>
        /// <param name="bManualReset"></param>
        /// <param name="bInitialState"></param>
        /// <param name="lpName"></param>
        /// <returns></returns>
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateEvent(
            [In, Optional]  IntPtr lpEventAttributes,
            [In]            bool bManualReset,
            [In]            bool bInitialState,
            [In, Optional]  string lpName);

        [DllImport("wimgapi", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WIMCreateFile")]
        internal static extern IntPtr WimCreateFile(
            [In, MarshalAs(UnmanagedType.LPWStr)] string WimPath,
            [In]    WimCreateFileDesiredAccess DesiredAccess,
            [In]    WimCreationDisposition CreationDisposition,
            [In]    WimActionFlags FlagsAndAttributes,
            [In]    WimCompressionType CompressionType,
            [Out, Optional] out WimCreationResult CreationResult
        );

        [DllImport("wimgapi", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WIMCloseHandle")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WimCloseHandle(
            [In]    IntPtr Handle
        );

        [DllImport("wimgapi", SetLastError = true)]
        internal static extern IntPtr WimLoadImage(IntPtr Handle, int ImageIndex);

        [DllImport("wimgapi")]
        internal static extern int WimGetImageCount(WimFileHandle Handle);

        [DllImport("wimgapi", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WIMApplyImage")]
        internal static extern bool WimApplyImage(
            [In]    WimImageHandle Handle,
            [In, Optional, MarshalAs(UnmanagedType.LPWStr)] string Path,
            [In]    WimApplyFlags Flags
        );

        [DllImport("wimgapi", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WIMGetImageInformation")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WimGetImageInformation(
            [In]        SafeHandle Handle,
            [Out]   out StringBuilder ImageInfo,
            [Out]   out uint SizeOfImageInfo
        );

        [DllImport("wimgapi", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WIMSetTemporaryPath")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WimSetTemporaryPath(
            [In]    WimFileHandle Handle,
            [In]    string TempPath
        );

        [DllImport("wimgapi", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WIMRegisterMessageCallback", CallingConvention = CallingConvention.StdCall)]
        internal static extern uint WimRegisterMessageCallback(
            [In, Optional] WimFileHandle hWim,
            [In]           WimMessageCallback MessageProc,
            [In, Optional] IntPtr ImageInfo
        );

        [DllImport("wimgapi", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "WIMUnregisterMessageCallback", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WimUnregisterMessageCallback(
            [In, Optional] WimFileHandle hWim,
            [In]           WimMessageCallback MessageProc
        );
    }
}
