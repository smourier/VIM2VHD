using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace VIM2VHD
{
    // Based on code written by the Hyper-V Test team.
    /// <summary>
    /// The Virtual Hard Disk class provides methods for creating and manipulating Virtual Hard Disk files.
    /// </summary>
    public class VirtualHardDisk : IDisposable
    {
        private SafeFileHandle _virtualHardDiskHandle = null;
        private string _filePath = null;
        private bool _disposed;
        private VirtualStorageDeviceType _deviceType = VirtualStorageDeviceType.Unknown;

        /// <summary>
        /// Disposal method for Virtual Hard Disk objects.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposal method for Virtual Hard Disk objects.
        /// </summary>
        /// <param name="disposing"></param>
        public void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (DiskIndex != 0)
                    {
                        Close();
                    }
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                // Note disposing has been done.
                _disposed = true;
            }
        }

        private VirtualHardDisk(SafeFileHandle handle, string Path, VirtualStorageDeviceType DeviceType)
        {
            if (handle.IsInvalid || handle.IsClosed)
                throw new ArgumentException("The handle to the Virtual Hard Disk is invalid.", nameof(handle));

            _virtualHardDiskHandle = handle;
            _filePath = Path;
            _deviceType = DeviceType;
        }

        /// <summary>
        /// Destroys a VHD object.
        /// </summary>
        ~VirtualHardDisk()
        {
            Dispose(false);
        }

        /// <summary>
        /// Abbreviated signature of CreateSparseDisk so it's easier to use from WIM2VHD.
        /// </summary>
        /// <param name="virtualStorageDeviceType">The type of disk to create, VHD or VHDX.</param>
        /// <param name="path">The path of the disk to create.</param>
        /// <param name="size">The maximum size of the disk to create.</param>
        /// <param name="overwrite">Overwrite the VHD if it already exists.</param>
        /// <returns>Virtual Hard Disk object</returns>
        public static VirtualHardDisk CreateSparseDisk(VirtualStorageDeviceType virtualStorageDeviceType, string path, ulong size, bool overwrite)
        {
            return CreateSparseDisk(
                path,
                size,
                overwrite,
                null,
                IntPtr.Zero,
                (virtualStorageDeviceType == VirtualStorageDeviceType.VHD)
                    ? NativeMethods.DEFAULT_BLOCK_SIZE
                    : 0,
                virtualStorageDeviceType,
                NativeMethods.DISK_SECTOR_SIZE);
        }

        /// <summary>
        /// Creates a new sparse (dynamically expanding) virtual hard disk (.vhd). Supports both sync and async modes.
        /// The VHD image file uses only as much space on the backing store as needed to store the actual data the VHD currently contains. 
        /// </summary>
        /// <param name="path">The path and name of the VHD to create.</param>
        /// <param name="size">The size of the VHD to create in bytes.  
        /// When creating this type of VHD, the VHD API does not test for free space on the physical backing store based on the maximum size requested, 
        /// therefore it is possible to successfully create a dynamic VHD with a maximum size larger than the available physical disk free space.
        /// The maximum size of a dynamic VHD is 2,040 GB.  The minimum size is 3 MB.</param>
        /// <param name="source">Optional path to pre-populate the new virtual disk object with block data from an existing disk
        /// This path may refer to a VHD or a physical disk.  Use NULL if you don't want a source.</param>
        /// <param name="overwrite">If the VHD exists, setting this parameter to 'True' will delete it and create a new one.</param>
        /// <param name="overlapped">If not null, the operation runs in async mode</param>
        /// <param name="blockSizeInBytes">Block size for the VHD.</param>
        /// <param name="virtualStorageDeviceType">VHD format version (VHD1 or VHD2)</param>
        /// <param name="sectorSizeInBytes">Sector size for the VHD.</param>
        /// <returns>Returns a SafeFileHandle corresponding to the virtual hard disk that was created.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid size is specified</exception>
        /// <exception cref="FileNotFoundException">Thrown when source VHD is not found.</exception>
        /// <exception cref="SecurityException">Thrown when there was an error while creating the default security descriptor.</exception>
        /// <exception cref="Win32Exception">Thrown when an error occurred while creating the VHD.</exception>
        public static VirtualHardDisk
        CreateSparseDisk(
            string path,
            ulong size,
            bool overwrite,
            string source,
            IntPtr overlapped,
            uint blockSizeInBytes,
            VirtualStorageDeviceType virtualStorageDeviceType,
            uint sectorSizeInBytes)
        {

            // Validate the virtualStorageDeviceType
            if (virtualStorageDeviceType != VirtualStorageDeviceType.VHD && virtualStorageDeviceType != VirtualStorageDeviceType.VHDX)
            {
                throw (
                    new ArgumentOutOfRangeException(
                        "virtualStorageDeviceType",
                        virtualStorageDeviceType,
                        "VirtualStorageDeviceType must be VHD or VHDX."
                ));
            }

            // Validate size.  It needs to be a multiple of DISK_SECTOR_SIZE (512)...
            if ((size % NativeMethods.DISK_SECTOR_SIZE) != 0)
            {

                throw (
                    new ArgumentOutOfRangeException(
                        "size",
                        size,
                        "The size of the virtual disk must be a multiple of 512."
                ));
            }

            if ((!String.IsNullOrEmpty(source)) && (!System.IO.File.Exists(source)))
            {

                throw (
                    new System.IO.FileNotFoundException(
                        "Unable to find the source file.",
                        source
                ));
            }

            if ((overwrite) && (System.IO.File.Exists(path)))
            {

                System.IO.File.Delete(path);
            }

            var createParams = new CreateVirtualDiskParameters();

            // Select the correct version.
            createParams.Version = (virtualStorageDeviceType == VirtualStorageDeviceType.VHD) ? CreateVirtualDiskVersion.Version1 : CreateVirtualDiskVersion.Version2;
            createParams.UniqueId = Guid.NewGuid();
            createParams.MaximumSize = size;
            createParams.BlockSizeInBytes = blockSizeInBytes;
            createParams.SectorSizeInBytes = sectorSizeInBytes;
            createParams.ParentPath = null;
            createParams.SourcePath = source;
            createParams.OpenFlags = OpenVirtualDiskFlags.None;
            createParams.GetInfoOnly = false;
            createParams.ParentVirtualStorageType = new VirtualStorageType();
            createParams.SourceVirtualStorageType = new VirtualStorageType();

            //
            // Create and init a security descriptor.
            // Since we're creating an essentially blank SD to use with CreateVirtualDisk
            // the VHD will take on the security values from the parent directory.
            //

            if (!NativeMethods.InitializeSecurityDescriptor(out var securityDescriptor, 1))
                throw (new SecurityException("Unable to initialize the security descriptor for the virtual disk."));

            var virtualStorageType = new VirtualStorageType();
            virtualStorageType.DeviceId = virtualStorageDeviceType;
            virtualStorageType.VendorId = NativeMethods.VirtualStorageTypeVendorMicrosoft;

            SafeFileHandle vhdHandle;

            uint returnCode = NativeMethods.CreateVirtualDisk(
                    ref virtualStorageType,
                    path,
                    (virtualStorageDeviceType == VirtualStorageDeviceType.VHD) ? VirtualDiskAccessMask.All : VirtualDiskAccessMask.None,
                    ref securityDescriptor,
                    CreateVirtualDiskFlags.None,
                    0,
                    ref createParams,
                    overlapped,
                    out vhdHandle);

            if (NativeMethods.ERROR_SUCCESS != returnCode && NativeMethods.ERROR_IO_PENDING != returnCode)
                throw (new Win32Exception((int)returnCode));

            return new VirtualHardDisk(vhdHandle, path, virtualStorageDeviceType);
        }

        /// <summary>
        /// Abbreviated signature of CreateFixedDisk so it's easier to use from WIM2VHD.
        /// </summary>
        /// <param name="virtualStorageDeviceType">The type of disk to create, VHD or VHDX.</param>
        /// <param name="path">The path of the disk to create.</param>
        /// <param name="size">The maximum size of the disk to create.</param>
        /// <param name="overwrite">Overwrite the VHD if it already exists.</param>
        /// <returns>Virtual Hard Disk object</returns>
        public static VirtualHardDisk CreateFixedDisk(VirtualStorageDeviceType virtualStorageDeviceType,
            string path,
            ulong size,
            bool overwrite)
        {

            return CreateFixedDisk(
                path,
                size,
                overwrite,
                null,
                IntPtr.Zero,
                0,
                virtualStorageDeviceType,
                NativeMethods.DISK_SECTOR_SIZE);
        }

        /// <summary>
        /// Creates a fixed-size Virtual Hard Disk. Supports both sync and async modes. This methods always calls the V2 version of the 
        /// CreateVirtualDisk API, and creates VHD2. 
        /// </summary>
        /// <param name="path">The path and name of the VHD to create.</param>
        /// <param name="size">The size of the VHD to create in bytes.  
        /// The VHD image file is pre-allocated on the backing store for the maximum size requested.
        /// The maximum size of a dynamic VHD is 2,040 GB.  The minimum size is 3 MB.</param>
        /// <param name="source">Optional path to pre-populate the new virtual disk object with block data from an existing disk
        /// This path may refer to a VHD or a physical disk.  Use NULL if you don't want a source.</param>
        /// <param name="overwrite">If the VHD exists, setting this parameter to 'True' will delete it and create a new one.</param>
        /// <param name="overlapped">If not null, the operation runs in async mode</param>
        /// <param name="blockSizeInBytes">Block size for the VHD.</param>
        /// <param name="virtualStorageDeviceType">Virtual storage device type: VHD1 or VHD2.</param>
        /// <param name="sectorSizeInBytes">Sector size for the VHD.</param>
        /// <returns>Returns a SafeFileHandle corresponding to the virtual hard disk that was created.</returns>
        /// <remarks>Creating a fixed disk can be a time consuming process!</remarks>  
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid size or wrong virtual storage device type is specified.</exception>
        /// <exception cref="FileNotFoundException">Thrown when source VHD is not found.</exception>
        /// <exception cref="SecurityException">Thrown when there was an error while creating the default security descriptor.</exception>
        /// <exception cref="Win32Exception">Thrown when an error occurred while creating the VHD.</exception>
        public static VirtualHardDisk CreateFixedDisk(
            string path,
            ulong size,
            bool overwrite,
            string source,
            IntPtr overlapped,
            uint blockSizeInBytes,
            VirtualStorageDeviceType virtualStorageDeviceType,
            uint sectorSizeInBytes)
        {

            // Validate the virtualStorageDeviceType
            if (virtualStorageDeviceType != VirtualStorageDeviceType.VHD && virtualStorageDeviceType != VirtualStorageDeviceType.VHDX)
            {

                throw (
                    new ArgumentOutOfRangeException(
                        "virtualStorageDeviceType",
                        virtualStorageDeviceType,
                        "VirtualStorageDeviceType must be VHD or VHDX."
                ));
            }

            // Validate size.  It needs to be a multiple of DISK_SECTOR_SIZE (512)...
            if ((size % NativeMethods.DISK_SECTOR_SIZE) != 0)
            {

                throw (
                    new ArgumentOutOfRangeException(
                        "size",
                        size,
                        "The size of the virtual disk must be a multiple of 512."
                ));
            }

            if ((!String.IsNullOrEmpty(source)) && (!System.IO.File.Exists(source)))
            {

                throw (
                    new System.IO.FileNotFoundException(
                        "Unable to find the source file.",
                        source
                ));
            }

            if ((overwrite) && (System.IO.File.Exists(path)))
            {

                System.IO.File.Delete(path);
            }

            var createParams = new CreateVirtualDiskParameters();

            // Select the correct version.
            createParams.Version = (virtualStorageDeviceType == VirtualStorageDeviceType.VHD)
                ? CreateVirtualDiskVersion.Version1
                : CreateVirtualDiskVersion.Version2;

            createParams.UniqueId = Guid.NewGuid();
            createParams.MaximumSize = size;
            createParams.BlockSizeInBytes = blockSizeInBytes;
            createParams.SectorSizeInBytes = sectorSizeInBytes;
            createParams.ParentPath = null;
            createParams.SourcePath = source;
            createParams.OpenFlags = OpenVirtualDiskFlags.None;
            createParams.GetInfoOnly = false;
            createParams.ParentVirtualStorageType = new VirtualStorageType();
            createParams.SourceVirtualStorageType = new VirtualStorageType();

            //
            // Create and init a security descriptor.
            // Since we're creating an essentially blank SD to use with CreateVirtualDisk
            // the VHD will take on the security values from the parent directory.
            //

            NativeMethods.SecurityDescriptor securityDescriptor;
            if (!NativeMethods.InitializeSecurityDescriptor(out securityDescriptor, 1))
            {
                throw (
                    new SecurityException(
                        "Unable to initialize the security descriptor for the virtual disk."
                ));
            }

            var virtualStorageType = new VirtualStorageType();
            virtualStorageType.DeviceId = virtualStorageDeviceType;
            virtualStorageType.VendorId = NativeMethods.VirtualStorageTypeVendorMicrosoft;

            SafeFileHandle vhdHandle;

            uint returnCode = NativeMethods.CreateVirtualDisk(
                ref virtualStorageType,
                    path,
                    (virtualStorageDeviceType == VirtualStorageDeviceType.VHD) ? VirtualDiskAccessMask.All : VirtualDiskAccessMask.None,
                ref securityDescriptor,
                    CreateVirtualDiskFlags.FullPhysicalAllocation,
                    0,
                ref createParams,
                    overlapped,
                out vhdHandle);

            if (NativeMethods.ERROR_SUCCESS != returnCode && NativeMethods.ERROR_IO_PENDING != returnCode)
            {

                throw (
                    new Win32Exception(
                        (int)returnCode
                ));
            }

            return new VirtualHardDisk(vhdHandle, path, virtualStorageDeviceType);
        }

        /// <summary>
        /// Opens a virtual hard disk (VHD) using the V2 of OpenVirtualDisk Win32 API for use, allowing you to explicitly specify OpenVirtualDiskFlags, 
        /// Read/Write depth, and Access Mask information.
        /// </summary>
        /// <param name="path">The path and name of the Virtual Hard Disk file to open.</param>
        /// <param name="accessMask">Contains the bit mask for specifying access rights to a virtual hard disk (VHD).  Default is All.</param>
        /// <param name="readWriteDepth">Indicates the number of stores, beginning with the child, of the backing store chain to open as read/write. 
        /// The remaining stores in the differencing chain will be opened read-only. This is necessary for merge operations to succeed.  Default is 0x1.</param>
        /// <param name="flags">An OpenVirtualDiskFlags object to modify the way the Virtual Hard Disk is opened.  Default is Unknown.</param>
        /// <param name="virtualStorageDeviceType">VHD Format Version (VHD1 or VHD2)</param>
        /// <returns>VirtualHardDisk object</returns>
        /// <exception cref="FileNotFoundException">Thrown if the VHD at path is not found.</exception>
        /// <exception cref="Win32Exception">Thrown if an error occurred while opening the VHD.</exception>
        public static VirtualHardDisk Open(string path, VirtualDiskAccessMask accessMask, uint readWriteDepth, OpenVirtualDiskFlags flags, VirtualStorageDeviceType virtualStorageDeviceType)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("The specified VHD was not found.  Please check your path and try again.", path);
            }

            var openParams = new OpenVirtualDiskParameters();

            // Select the correct version.
            openParams.Version = (virtualStorageDeviceType == VirtualStorageDeviceType.VHD)
                ? OpenVirtualDiskVersion.Version1
                : OpenVirtualDiskVersion.Version2;

            openParams.GetInfoOnly = false;

            var virtualStorageType = new VirtualStorageType();
            virtualStorageType.DeviceId = virtualStorageDeviceType;

            virtualStorageType.VendorId = (virtualStorageDeviceType == VirtualStorageDeviceType.Unknown)
                ? virtualStorageType.VendorId = NativeMethods.VirtualStorageTypeVendorUnknown
                : virtualStorageType.VendorId = NativeMethods.VirtualStorageTypeVendorMicrosoft;

            SafeFileHandle vhdHandle;

            uint returnCode = NativeMethods.OpenVirtualDisk(
                ref virtualStorageType,
                    path,
                    accessMask,
                    flags,
                ref openParams,
                out vhdHandle);

            if (NativeMethods.ERROR_SUCCESS != returnCode)
            {
                throw new Win32Exception((int)returnCode);
            }

            return new VirtualHardDisk(vhdHandle, path, virtualStorageDeviceType);
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

            return new ReadOnlyCollection<char>(availableDrives);
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
            var handleForOverllapped = GCHandle.Alloc(overlapped, GCHandleType.Pinned);
            return handleForOverllapped;
        }

        /// <summary>
        /// GetVirtualDiskOperationProgress API allows getting progress info for the async virtual disk operations (ie. Online Mirror)
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="overlapped"></param>
        /// <returns></returns>
        /// <exception cref="Win32Exception">Thrown when an error occurred while mirroring the VHD.</exception>
        public uint GetVirtualDiskOperationProgress(ref VirtualDiskProgress progress, IntPtr overlapped)
        {
            uint returnCode = NativeMethods.GetVirtualDiskOperationProgress(
                    this._virtualHardDiskHandle,
                    overlapped,
                ref progress);

            return returnCode;
        }

        /// <summary>
        /// Closes all open handles to the Virtual Hard Disk object.
        /// If the VHD is currently attached, and the PermanentLifetime was not specified, this operation will detach it.
        /// </summary>
        public void Close()
        {
            _virtualHardDiskHandle.Close();
        }

        /// <summary>
        /// Attaches a virtual hard disk (VHD) by locating an appropriate VHD provider to accomplish the attachment.
        /// </summary>
        /// <param name="attachVirtualDiskFlags">
        /// A combination of values from the attachVirtualDiskFlags enumeration which will dictate how the behavior of the VHD once mounted.
        /// </param>
        /// <exception cref="Win32Exception">Thrown when an error occurred while attaching the VHD.</exception>
        /// <exception cref="SecurityException">Thrown when an error occurred while creating the default security descriptor.</exception>
        public void Attach(AttachVirtualDiskFlags attachVirtualDiskFlags)
        {
            if (!this.IsAttached)
            {
                // Get the current disk index.  We need it later.
                int diskIndex = this.DiskIndex;

                var attachParameters = new AttachVirtualDiskParameters();

                // For attach, the correct version is always Version1 for Win7 and Win8.
                attachParameters.Version = AttachVirtualDiskVersion.Version1;
                attachParameters.Reserved = 0;

                if (!NativeMethods.InitializeSecurityDescriptor(out NativeMethods.SecurityDescriptor securityDescriptor, 1))
                    throw (new SecurityException("Unable to initialize the security descriptor for the virtual disk."));

                uint returnCode = NativeMethods.AttachVirtualDisk(
                         _virtualHardDiskHandle,
                    ref securityDescriptor,
                         attachVirtualDiskFlags,
                         0,
                    ref attachParameters,
                         IntPtr.Zero);

                switch (returnCode)
                {

                    case NativeMethods.ERROR_SUCCESS:
                        break;

                    default:
                        throw new Win32Exception((int)returnCode);
                }

                // There's apparently a bit of a timing issue here on some systems.
                // If the disk index isn't updated, keep checking once per second for five seconds.
                // If it's not updated after that, it's probably not our fault.
                short attempts = 5;
                while ((attempts-- >= 0) && (diskIndex == this.DiskIndex))
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// Attaches a virtual hard disk (VHD) by locating an appropriate VHD provider to accomplish the attachment.
        /// </summary>
        /// <remarks>
        /// This method attaches the VHD with no flags.
        /// </remarks>
        /// <exception cref="Win32Exception">Thrown when an error occurred while attaching the VHD.</exception>
        /// <exception cref="SecurityException">Thrown when an error occurred while creating the default security descriptor.</exception>
        public void Attach() => Attach(AttachVirtualDiskFlags.None);

        /// <summary>
        /// Unsurfaces (detaches) a virtual hard disk (VHD) by locating an appropriate VHD provider to accomplish the operation.
        /// </summary>
        public void Detach()
        {
            if (!IsAttached)
                return;

            var ret = NativeMethods.DetachVirtualDisk(_virtualHardDiskHandle, DetachVirtualDiskFlag.None, 0);
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
        /// Reduces the size of the virtual hard disk (VHD) backing store file. Supports both sync and async modes.
        /// </summary>
        /// <param name="overlapped">If not null, the operation runs in async mode</param>
        public void Compact(IntPtr overlapped) => Compact(overlapped, CompactVirtualDiskFlags.None);

        /// <summary>
        /// Reduces the size of the virtual hard disk (VHD) backing store file. Supports both sync and async modes.
        /// </summary>
        /// <param name="overlapped">If not null, the operation runs in async mode</param>
        /// <param name="flags">Flags for Compact operation</param>
        public void Compact(IntPtr overlapped, CompactVirtualDiskFlags flags)
        {
            var compactParams = new CompactVirtualDiskParameters();
            compactParams.Version = CompactVirtualDiskVersion.Version1;
            var ret = NativeMethods.CompactVirtualDisk(_virtualHardDiskHandle, flags, ref compactParams, overlapped);

            if ((overlapped == IntPtr.Zero && NativeMethods.ERROR_SUCCESS != ret) ||
                (overlapped != IntPtr.Zero && NativeMethods.ERROR_IO_PENDING != ret))
                throw new Win32Exception(ret);
        }

        /// <summary>
        /// The SafeFileHandle object for the opened VHD.
        /// </summary>
        public SafeFileHandle VirtualHardDiskHandle => _virtualHardDiskHandle;

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

                var match = Regex.Match(path, @"\d+$"); // look for the last digits in the path
                return Convert.ToInt32(match.Value, CultureInfo.InvariantCulture);
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
                int pathSize = 1024;  // Isn't MAX_PATH 255?
                var path = new StringBuilder(pathSize);
                var ret = NativeMethods.GetVirtualDiskPhysicalPath(_virtualHardDiskHandle, ref pathSize, path);
                if (ret == NativeMethods.ERROR_ERROR_DEV_NOT_EXIST)
                    return null;

                if (ret != NativeMethods.ERROR_SUCCESS)
                    throw new Win32Exception(ret);

                return path.ToString();
            }
        }
    }
}
