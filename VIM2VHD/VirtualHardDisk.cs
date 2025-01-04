using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace VIM2VHD
{
    // Based on code written by the Hyper-V Test team.
    /// <summary>
    /// The Virtual Hard Disk class provides methods for creating and manipulating Virtual Hard Disk files.
    /// </summary>
    public sealed class VirtualHardDisk : IDisposable
    {
        private IntPtr _handle;

        private VirtualHardDisk(IntPtr handle, string filePath)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("The handle to the Virtual Hard Disk is invalid.", nameof(handle));

            _handle = handle;
            FilePath = filePath;
        }

        public string FilePath { get; }

        private IntPtr CheckDisposed()
        {
            var handle = _handle;
            if (handle == null)
                throw new ObjectDisposedException("Handle");

            return handle;
        }

        public void Dispose()
        {
            Detach();
            var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
            if (handle != IntPtr.Zero)
            {
                NativeMethods.CloseHandle(handle);
            }
        }

        //
        // CREATE_VIRTUAL_DISK_VERSION_2 allows specifying a richer set a values and returns
        // a V2 handle.
        //
        // VIRTUAL_DISK_ACCESS_NONE is the only acceptable access mask for V2 handle opens.
        //
        // Valid BlockSize values are as follows (use 0 to indicate default value):
        //      Fixed VHD: 0
        //      Dynamic VHD: 512kb, 2mb (default)
        //      Differencing VHD: 512kb, 2mb (if parent is fixed, default is 2mb; if parent is dynamic or differencing, default is parent blocksize)
        //      Fixed VHDX: 0
        //      Dynamic VHDX: 1mb, 2mb, 4mb, 8mb, 16mb, 32mb (default), 64mb, 128mb, 256mb
        //      Differencing VHDX: 1mb, 2mb (default), 4mb, 8mb, 16mb, 32mb, 64mb, 128mb, 256mb
        //
        // Valid LogicalSectorSize values are as follows (use 0 to indicate default value):
        //      VHD: 512 (default)
        //      VHDX: 512 (for fixed or dynamic, default is 512; for differencing, default is parent logicalsectorsize), 4096
        //
        // Valid PhysicalSectorSize values are as follows (use 0 to indicate default value):
        //      VHD: 512 (default)
        //      VHDX: 512, 4096 (for fixed or dynamic, default is 4096; for differencing, default is parent physicalsectorsize)
        //
        public static VirtualHardDisk CreateDisk(
            string filePath,
            ulong maximumSize,
            IntPtr overlapped,
            bool overwrite = false,
            string source = null,
            int blockSizeInBytes = 0,
            int sectorSizeInBytes = 0,
            int physicalSectorSizeInBytes = 0,
            CREATE_VIRTUAL_DISK_FLAG flags = CREATE_VIRTUAL_DISK_FLAG.CREATE_VIRTUAL_DISK_FLAG_NONE)
        {
            if (overwrite && File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var createParams = new NativeMethods.CREATE_VIRTUAL_DISK_PARAMETERS_VERSION2();
            createParams.Version = NativeMethods.CREATE_VIRTUAL_DISK_VERSION.CREATE_VIRTUAL_DISK_VERSION_2;
            //createParams.UniqueId = Guid.NewGuid();
            createParams.MaximumSize = maximumSize;
            createParams.BlockSizeInBytes = blockSizeInBytes;
            createParams.SectorSizeInBytes = sectorSizeInBytes;
            createParams.PhysicalSectorSizeInBytes = physicalSectorSizeInBytes;
            createParams.ParentPath = null;
            createParams.SourcePath = source;
            createParams.OpenFlags = OPEN_VIRTUAL_DISK_FLAG.OPEN_VIRTUAL_DISK_FLAG_NONE;
            //createParams.ParentVirtualStorageType = new NativeMethods.VIRTUAL_STORAGE_TYPE();
            //createParams.SourceVirtualStorageType = new NativeMethods.VIRTUAL_STORAGE_TYPE();

            if (!NativeMethods.InitializeSecurityDescriptor(out var securityDescriptor, 1))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            var virtualStorageType = new NativeMethods.VIRTUAL_STORAGE_TYPE();
            virtualStorageType.DeviceId = VIRTUAL_STORAGE_TYPE_DEVICE.VIRTUAL_STORAGE_TYPE_DEVICE_VHDX;

            int ret = NativeMethods.CreateVirtualDisk(
                ref virtualStorageType,
                filePath,
                VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_NONE,
                ref securityDescriptor,
                flags,
                0,
                ref createParams,
                overlapped,
                out var handle);

            if (NativeMethods.ERROR_SUCCESS != ret && NativeMethods.ERROR_IO_PENDING != ret)
                throw new Win32Exception(ret);

            return new VirtualHardDisk(handle, filePath);
        }

        /// <summary>
        /// Opens a virtual hard disk (VHD) using the V2 of OpenVirtualDisk Win32 API for use, allowing you to explicitly specify OpenVirtualDiskFlags, 
        /// Read/Write depth, and Access Mask information.
        /// </summary>
        /// <param name="filePath">The path and name of the Virtual Hard Disk file to open.</param>
        /// <param name="accessMask">Contains the bit mask for specifying access rights to a virtual hard disk (VHD).  Default is All.</param>
        /// <param name="flags">An OpenVirtualDiskFlags object to modify the way the Virtual Hard Disk is opened.  Default is Unknown.</param>
        /// <param name="virtualStorageDeviceType">VHD Format Version (VHD1 or VHD2)</param>
        /// <returns>VirtualHardDisk object</returns>
        public static VirtualHardDisk Open(
            string filePath,
            VIRTUAL_STORAGE_TYPE_DEVICE virtualStorageDeviceType,
            VIRTUAL_DISK_ACCESS_MASK accessMask = VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_ALL,
            OPEN_VIRTUAL_DISK_FLAG flags = OPEN_VIRTUAL_DISK_FLAG.OPEN_VIRTUAL_DISK_FLAG_NONE)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("The specified VHD was not found.  Please check your path and try again.", filePath);

            var openParams = new NativeMethods.OPEN_VIRTUAL_DISK_PARAMETERS();
            openParams.Version = virtualStorageDeviceType == VIRTUAL_STORAGE_TYPE_DEVICE.VIRTUAL_STORAGE_TYPE_DEVICE_VHD ? NativeMethods.OPEN_VIRTUAL_DISK_VERSION.OPEN_VIRTUAL_DISK_VERSION_1 : NativeMethods.OPEN_VIRTUAL_DISK_VERSION.OPEN_VIRTUAL_DISK_VERSION_2;
            openParams.GetInfoOnly = false;

            var virtualStorageType = new NativeMethods.VIRTUAL_STORAGE_TYPE();
            virtualStorageType.DeviceId = virtualStorageDeviceType;
            virtualStorageType.VendorId = virtualStorageDeviceType == VIRTUAL_STORAGE_TYPE_DEVICE.VIRTUAL_STORAGE_TYPE_DEVICE_UNKNOWN ? NativeMethods.VirtualStorageTypeVendorUnknown : NativeMethods.VirtualStorageTypeVendorMicrosoft;
            int ret = NativeMethods.OpenVirtualDisk(ref virtualStorageType, filePath, accessMask, flags, ref openParams, out var handle);
            if (NativeMethods.ERROR_SUCCESS != ret)
                throw new Win32Exception(ret);

            return new VirtualHardDisk(handle, filePath);
        }

        /// <summary>
        /// Retrieves a collection of drive letters that are currently available on the system.
        /// </summary>
        /// <remarks>Drives A and B are not included in the collection, even if they are available.</remarks>
        /// <returns>A collection of drive letters that are currently available on the system.</returns>
        public static IReadOnlyList<char> GetAvailableDriveLetters()
        {
            var availableDrives = new List<char>();
            for (int i = (byte)'C'; i <= (byte)'Z'; i++)
            {
                availableDrives.Add((char)i);
            }
            foreach (string drive in Environment.GetLogicalDrives())
            {
                availableDrives.Remove(drive.ToUpper(CultureInfo.InvariantCulture)[0]);
            }
            return availableDrives;
        }

        /// <summary>
        /// Gets the first available drive letter on the current system.
        /// </summary>
        /// <remarks>Drives A and B will not be returned, even if they are available.</remarks>
        /// <returns>Char representing the first available drive letter.</returns>
        public static char GetFirstAvailableDriveLetter() => GetAvailableDriveLetters()[0];

        /// <summary>
        /// Creates a NativeOverlapped object, initializes its EventHandle property, and pins the object to the memory.
        /// This overlapped objects are useful when executing VHD meta-ops in async mode.
        /// </summary>
        /// <returns>Returns the GCHandle for the pinned overlapped structure</returns>
        public static GCHandle CreatePinnedOverlappedObject()
        {
            var overlapped = new NativeOverlapped();
            overlapped.EventHandle = NativeMethods.CreateEvent(IntPtr.Zero, true, false, null);
            return GCHandle.Alloc(overlapped, GCHandleType.Pinned);
        }

        /// <summary>
        /// GetVirtualDiskOperationProgress API allows getting progress info for the async virtual disk operations (ie. Online Mirror)
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="overlapped"></param>
        /// <returns></returns>
        private void GetVirtualDiskOperationProgress(ref NativeMethods.VIRTUAL_DISK_PROGRESS progress, IntPtr overlapped)
        {
            int ret = NativeMethods.GetVirtualDiskOperationProgress(CheckDisposed(), overlapped, ref progress);
            if (ret != NativeMethods.ERROR_SUCCESS)
                throw new Win32Exception(ret);
        }

        /// <summary>
        /// Attaches a virtual hard disk (VHD) by locating an appropriate VHD provider to accomplish the attachment.
        /// </summary>
        /// <param name="flags">
        /// A combination of values from the attachVirtualDiskFlags enumeration which will dictate how the behavior of the VHD once mounted.
        /// </param>
        public void Attach(
            ATTACH_VIRTUAL_DISK_FLAG flags = ATTACH_VIRTUAL_DISK_FLAG.ATTACH_VIRTUAL_DISK_FLAG_NONE,
            ATTACH_VIRTUAL_DISK_VERSION version = ATTACH_VIRTUAL_DISK_VERSION.ATTACH_VIRTUAL_DISK_VERSION_1)
        {
            if (IsAttached)
                return;

            // Get the current disk index.  We need it later.
            int diskIndex = DiskIndex;
            var attachParameters = new NativeMethods.ATTACH_VIRTUAL_DISK_PARAMETERS();

            // For attach, the correct version is always Version1 for Win7 and Win8.
            attachParameters.Version = version;
            attachParameters.Reserved = 0;

            if (!NativeMethods.InitializeSecurityDescriptor(out NativeMethods.SECURITY_DESCRIPTOR securityDescriptor, 1))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            int ret = NativeMethods.AttachVirtualDisk(CheckDisposed(), ref securityDescriptor, flags, 0, ref attachParameters, IntPtr.Zero);
            if (ret != NativeMethods.ERROR_SUCCESS)
                throw new Win32Exception(ret);

            // There's apparently a bit of a timing issue here on some systems.
            // If the disk index isn't updated, keep checking once per second for five seconds.
            // If it's not updated after that, it's probably not our fault.
            var attempts = 5;
            while (attempts-- >= 0 && diskIndex == DiskIndex)
            {
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Unsurfaces (detaches) a virtual hard disk (VHD) by locating an appropriate VHD provider to accomplish the operation.
        /// </summary>
        public void Detach(
            DETACH_VIRTUAL_DISK_FLAG flags = DETACH_VIRTUAL_DISK_FLAG.DETACH_VIRTUAL_DISK_FLAG_NONE)
        {
            if (!IsAttached)
                return;

            var ret = NativeMethods.DetachVirtualDisk(CheckDisposed(), flags, 0);
            switch (ret)
            {
                case NativeMethods.ERROR_NOT_FOUND:
                // There's nothing to do here.  The device wasn't found, which means there's a 
                // really good chance that it wasn't attached to begin with.
                // And, since we were asked to detach it anyway, we can assume that the system
                // is already in the desired state.
                case NativeMethods.ERROR_SUCCESS:
                    break;

                default:
                    throw new Win32Exception(ret);
            }
        }

        /// <summary>
        /// Reduces the size of the virtual hard disk (VHD) backing store file.
        /// </summary>
        /// <param name="flags">Flags for Compact operation</param>
        public void Compact(COMPACT_VIRTUAL_DISK_FLAG flags = COMPACT_VIRTUAL_DISK_FLAG.COMPACT_VIRTUAL_DISK_FLAG_NONE) => Compact(IntPtr.Zero, flags);

        /// <summary>
        /// Reduces the size of the virtual hard disk (VHD) backing store file. Supports both sync and async modes.
        /// </summary>
        /// <param name="overlapped">If not null, the operation runs in async mode</param>
        /// <param name="flags">Flags for Compact operation</param>
        public void Compact(
            IntPtr overlapped,
            COMPACT_VIRTUAL_DISK_FLAG flags = COMPACT_VIRTUAL_DISK_FLAG.COMPACT_VIRTUAL_DISK_FLAG_NONE)
        {
            var compactParams = new NativeMethods.COMPACT_VIRTUAL_DISK_PARAMETERS();
            compactParams.Version = NativeMethods.COMPACT_VIRTUAL_DISK_VERSION.COMPACT_VIRTUAL_DISK_VERSION_1;
            var ret = NativeMethods.CompactVirtualDisk(CheckDisposed(), flags, ref compactParams, overlapped);
            if ((overlapped == IntPtr.Zero && ret != NativeMethods.ERROR_SUCCESS) ||
                (overlapped != IntPtr.Zero && ret != NativeMethods.ERROR_IO_PENDING))
                throw new Win32Exception(ret);
        }

        /// <summary>
        /// Indicates the index of the disk when attached.
        /// If the virtual hard disk is not currently attached, -1 will be returned.
        /// </summary>
        public int DiskIndex
        {
            get
            {
                string path = PhysicalPath;
                if (path == null)
                    return -1;

                // look for the last digits in the path
                return int.Parse(Regex.Match(path, @"\d+$").Value);
            }
        }

        /// <summary>
        /// Indicates whether the current Virtual Hard Disk is attached to the system.
        /// </summary>
        public bool IsAttached => DiskIndex != -1;

        /// <summary>
        /// Retrieves the path to the physical device object that contains a virtual hard disk (VHD), if the VHD is attached.
        /// If it is not attached, NULL will be returned.
        /// </summary>
        public string PhysicalPath
        {
            get
            {
                int pathSize = 1024;
                var path = new StringBuilder(pathSize);
                var ret = NativeMethods.GetVirtualDiskPhysicalPath(CheckDisposed(), ref pathSize, path);
                if (ret == NativeMethods.ERROR_ERROR_DEV_NOT_EXIST)
                    return null;

                if (ret != NativeMethods.ERROR_SUCCESS)
                    throw new Win32Exception(ret);

                return path.ToString();
            }
        }
    }
}
