using System;
using System.Runtime.InteropServices;
using System.Text;

namespace VIM2VHD
{
    internal class NativeMethods
    {
        public delegate WIM_MSG_RETURN WIMMessageCallback(WIM_MSG dwMessageId, IntPtr wParam, IntPtr lParam, IntPtr pvUserData);

        public const int WM_APP = 0x00008000;

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
        public const int ERROR_ERROR_DEV_NOT_EXIST = 0x00000037;
        public const int ERROR_SUCCESS = 0x00000000;

        public static Guid VirtualStorageTypeVendorUnknown = new Guid("00000000-0000-0000-0000-000000000000");
        public static Guid VirtualStorageTypeVendorMicrosoft = new Guid("EC984AEC-A0F9-47e9-901F-71415A66345B");

        public enum WIM_CREATION_RESULT
        {
            WIM_CREATED_NEW = 0,
            WIM_OPENED_EXISTING
        }

        [Flags]
        public enum WIM_ACCESS
        {
            WIM_GENERIC_READ = unchecked((int)0x80000000),
            WIM_GENERIC_WRITE = 0x40000000,
            WIM_GENERIC_MOUNT = 0x20000000,
        }

        public enum WIM_CREATION_DISPOSITION
        {
            WIM_CREATE_NEW = 1,
            WIM_CREATE_ALWAYS = 2,
            WIM_OPEN_EXISTING = 3,
            WIM_OPEN_ALWAYS = 4
        }

        public enum COMPACT_VIRTUAL_DISK_VERSION
        {
            COMPACT_VIRTUAL_DISK_VERSION_UNSPECIFIED = 0x00000000,
            COMPACT_VIRTUAL_DISK_VERSION_1 = 0x00000001
        }

        public enum OPEN_VIRTUAL_DISK_VERSION
        {
            OPEN_VIRTUAL_DISK_VERSION_UNSPECIFIED = 0,
            OPEN_VIRTUAL_DISK_VERSION_1 = 1,
            OPEN_VIRTUAL_DISK_VERSION_2 = 2,
            OPEN_VIRTUAL_DISK_VERSION_3 = 3,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct VIRTUAL_DISK_PROGRESS
        {
            public int OperationStatus;
            public ulong CurrentValue;
            public ulong CompletionValue;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct VIRTUAL_STORAGE_TYPE
        {
            public VIRTUAL_STORAGE_TYPE_DEVICE DeviceId;
            public Guid VendorId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct COMPACT_VIRTUAL_DISK_PARAMETERS
        {
            public COMPACT_VIRTUAL_DISK_VERSION Version;
            public int Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct OPEN_VIRTUAL_DISK_PARAMETERS
        {
            public OPEN_VIRTUAL_DISK_VERSION Version;
            public bool GetInfoOnly;
            public Guid ResiliencyGuid;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_DESCRIPTOR
        {
            public byte revision;
            public byte size;
            public short control;
            public IntPtr owner;
            public IntPtr group;
            public IntPtr sacl;
            public IntPtr dacl;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct ATTACH_VIRTUAL_DISK_PARAMETERS
        {
            public ATTACH_VIRTUAL_DISK_VERSION Version;
            public int Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CREATE_VIRTUAL_DISK_PARAMETERS_VERSION2
        {
            /// <summary>
            /// A CREATE_VIRTUAL_DISK_VERSION enumeration that specifies the version of the CREATE_VIRTUAL_DISK_PARAMETERS structure being passed to or from the virtual hard disk (VHD) functions.
            /// </summary>
            public CREATE_VIRTUAL_DISK_VERSION Version;

            /// <summary>
            /// Unique identifier to assign to the virtual disk object. If this member is set to zero, a unique identifier is created by the system.
            /// </summary>
            public Guid UniqueId;

            /// <summary>
            /// The maximum virtual size of the virtual disk object. Must be a multiple of 512.
            /// If a ParentPath is specified, this value must be zero.
            /// If a SourcePath is specified, this value can be zero to specify the size of the source VHD to be used, otherwise the size specified must be greater than or equal to the size of the source disk.
            /// </summary>
            public ulong MaximumSize;

            /// <summary>
            /// Internal size of the virtual disk object blocks.
            /// The following are predefined block sizes and their behaviors. For a fixed VHD type, this parameter must be zero.
            /// </summary>
            public int BlockSizeInBytes;

            /// <summary>
            /// Internal size of the virtual disk object sectors. Must be set to 512.
            /// </summary>
            public int SectorSizeInBytes;

            public int PhysicalSectorSizeInBytes;

            /// <summary>
            /// Optional path to a parent virtual disk object. Associates the new virtual disk with an existing virtual disk.
            /// If this parameter is not NULL, SourcePath must be NULL.
            /// </summary>
            public string ParentPath;

            /// <summary>
            /// Optional path to pre-populate the new virtual disk object with block data from an existing disk. This path may refer to a VHD or a physical disk.
            /// If this parameter is not NULL, ParentPath must be NULL.
            /// </summary>
            public string SourcePath;

            /// <summary>
            /// Flags for opening the VHD
            /// </summary>
            public OPEN_VIRTUAL_DISK_FLAG OpenFlags;

            /// <summary>
            /// Virtual Storage Type of the parent disk
            /// </summary>
            public VIRTUAL_STORAGE_TYPE ParentVirtualStorageType;

            /// <summary>
            /// Virtual Storage Type of the source disk
            /// </summary>
            public VIRTUAL_STORAGE_TYPE SourceVirtualStorageType;

            /// <summary>
            /// A GUID to use for fallback resiliency over SMB.
            /// </summary>
            public Guid ResiliencyGuid;
        }

        public enum CREATE_VIRTUAL_DISK_VERSION
        {
            CREATE_VIRTUAL_DISK_VERSION_UNSPECIFIED = 0,
            CREATE_VIRTUAL_DISK_VERSION_1 = 1,
            CREATE_VIRTUAL_DISK_VERSION_2 = 2,
            //CREATE_VIRTUAL_DISK_VERSION_3 = 3,
            //CREATE_VIRTUAL_DISK_VERSION_4 = 4,
        }

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("virtdisk")]
        public static extern int CreateVirtualDisk(
            ref VIRTUAL_STORAGE_TYPE VirtualStorageType,
            [MarshalAs(UnmanagedType.LPWStr)] string Path,
            VIRTUAL_DISK_ACCESS_MASK VirtualDiskAccessMask,
            ref SECURITY_DESCRIPTOR SecurityDescriptor,
            CREATE_VIRTUAL_DISK_FLAG Flags,
            int ProviderSpecificFlags,
            ref CREATE_VIRTUAL_DISK_PARAMETERS_VERSION2 Parameters,
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
            ref SECURITY_DESCRIPTOR SecurityDescriptor,
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
        public static extern bool InitializeSecurityDescriptor(out SECURITY_DESCRIPTOR pSecurityDescriptor, int dwRevision);

        // CreateEvent API is used while calling async Online Mirror API
        [DllImport("kernel32")]
        public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, [MarshalAs(UnmanagedType.LPWStr)] string lpName);

        [DllImport("wimgapi", SetLastError = true)]
        public static extern IntPtr WIMCreateFile(
            [MarshalAs(UnmanagedType.LPWStr)] string pszWimPath,
            WIM_ACCESS dwDesiredAccess,
            WIM_CREATION_DISPOSITION dwCreationDisposition,
            WIM_FLAG dwFlagsAndAttributes,
            WIM_COMPRESSION_TYPE dwCompressionType,
            out WIM_CREATION_RESULT pdwCreationResult
        );

        [DllImport("wimgapi", SetLastError = true)]
        public static extern bool WIMCloseHandle(IntPtr hObject);

        [DllImport("wimgapi", SetLastError = true)]
        public static extern IntPtr WIMLoadImage(IntPtr hWim, int dwImageIndex);

        [DllImport("wimgapi")]
        public static extern int WIMGetImageCount(IntPtr hWim);

        [DllImport("wimgapi", SetLastError = true)]
        public static extern bool WIMApplyImage(IntPtr hImage, [MarshalAs(UnmanagedType.LPWStr)] string pszPath, WIM_FLAG dwApplyFlags);

        [DllImport("wimgapi", SetLastError = true)]
        public static extern bool WIMGetImageInformation(IntPtr hImage, [MarshalAs(UnmanagedType.LPWStr)] out StringBuilder ppvImageInfo, out int pcbImageInfo);

        [DllImport("wimgapi", SetLastError = true)]
        public static extern bool WIMSetTemporaryPath(IntPtr hWim, [MarshalAs(UnmanagedType.LPWStr)] string pszPath);

        [DllImport("wimgapi", SetLastError = true)]
        public static extern int WIMRegisterMessageCallback(IntPtr hWim, WIMMessageCallback fpMessageProc, IntPtr pvUserData);

        [DllImport("wimgapi", SetLastError = true)]
        public static extern bool WIMUnregisterMessageCallback(IntPtr hWim, WIMMessageCallback fpMessageProc);

        [DllImport("wimgapi", SetLastError = true)]
        public static extern bool WIMRegisterLogFile([MarshalAs(UnmanagedType.LPWStr)] string pszLogFile, int dwFlags);

        [DllImport("wimgapi", SetLastError = true)]
        public static extern bool WIMUnregisterLogFile([MarshalAs(UnmanagedType.LPWStr)] string pszLogFile);

        [DllImport("wimgapi", SetLastError = true)]
        public static extern bool WIMExtractImagePath(IntPtr hImage, [MarshalAs(UnmanagedType.LPWStr)] string pszImagePath, [MarshalAs(UnmanagedType.LPWStr)] string pszDestinationPath, WIM_FLAG dwExtractFlags);
    }
}
