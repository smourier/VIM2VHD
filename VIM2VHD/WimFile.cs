using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace VIM2VHD
{
    public sealed class WimFile : IDisposable
    {
        private readonly static ConcurrentDictionary<IntPtr, WimFile> _sinks = new ConcurrentDictionary<IntPtr, WimFile>();
        private readonly static NativeMethods.WIMMessageCallback _messageCallback = MessageCallback;
        private IntPtr _handle;
        private List<WimImage> _imageList;
        public event EventHandler<WimFileEventArgs> Event;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filePath">Path to the WIM container.</param>
        public WimFile(string filePath, WimFileOpenOptions options = null)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            options = options ?? new WimFileOpenOptions();
            _handle = NativeMethods.WIMCreateFile(
                filePath,
                NativeMethods.WIM_ACCESS.WIM_GENERIC_READ,
                NativeMethods.WIM_CREATION_DISPOSITION.WIM_OPEN_EXISTING,
                options.Flags,
                options.CompressionType,
                out var creationResult
            );
            if (_handle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            if (creationResult != NativeMethods.WIM_CREATION_RESULT.WIM_OPENED_EXISTING)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            if (!NativeMethods.WIMGetImageInformation(CheckDisposed(), out StringBuilder builder, out int bytes))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            // Ensure the length of the returned bytes to avoid garbage characters at the end.
            int count = bytes / sizeof(char);
            if (builder != null)
            {
                // Get rid of the unicode file marker at the beginning of the XML.
                builder.Remove(0, 1);
                builder.EnsureCapacity(count - 1);
                builder.Length = count - 1;

                var xml = XDocument.Parse(builder.ToString().Trim());
                Size = ulong.Parse(xml.Root?.Element("TOTALBYTES")?.Value ?? "0");
            }

            if (string.IsNullOrWhiteSpace(options.TempDirectoryPath))
            {
                options.TempDirectoryPath = Environment.ExpandEnvironmentVariables("%TEMP%");
            }
            else
            {
                // ensure it exists
                Extensions.FileCreateDirectory(Path.Combine(options.TempDirectoryPath, "dummy"));
            }

            NativeMethods.WIMSetTemporaryPath(_handle, options.TempDirectoryPath);

            if (options.RegisterForEvents)
            {
                _sinks.AddOrUpdate(_handle, this, (k, old) => this);
                NativeMethods.WIMRegisterMessageCallback(_handle, _messageCallback, _handle);
            }
        }

        public ulong Size { get; }

        public string GetRelativePath(string filePath)
        {
            if (filePath == null)
                return null;

            foreach (var img in Images)
            {
                var path = img.ApplyingPath;
                if (path != null && filePath.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                    return filePath.Substring(path.Length);
            }
            return filePath;
        }

        private static WIM_MSG_RETURN MessageCallback(WIM_MSG dwMessageId, IntPtr wParam, IntPtr lParam, IntPtr pvUserData)
        {
            var ret = WIM_MSG_RETURN.WIM_MSG_SUCCESS;
            if (_sinks.TryGetValue(pvUserData, out var file))
            {
                var handler = file.Event;
                if (handler != null)
                {
                    WimFileEventArgs e;
                    switch (dwMessageId)
                    {
                        case WIM_MSG.WIM_MSG_PROCESS:
                            e = new WimFileProcessEventArgs(file, dwMessageId, wParam, lParam);
                            break;

                        case WIM_MSG.WIM_MSG_ERROR:
                            e = new WimFileErrorEventArgs(file, dwMessageId, wParam, lParam);
                            break;

                        case WIM_MSG.WIM_MSG_PROGRESS:
                            e = new WimFileProgressEventArgs(dwMessageId, wParam, lParam);
                            break;

                        case WIM_MSG.WIM_MSG_SETPOS:
                            e = new WimFileSetPosEventArgs(dwMessageId, wParam, lParam);
                            break;

                        case WIM_MSG.WIM_MSG_SETRANGE:
                            e = new WimFileSetRangeEventArgs(dwMessageId, wParam, lParam);
                            break;

                        default:
                            e = new WimFileEventArgs(dwMessageId, wParam, lParam);
                            break;
                    }

                    if (Debugger.IsAttached)
                    {
                        handler(file, e);
                    }
                    else
                    {
                        try
                        {
                            handler(file, e);
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine("An error occurred in the Event handler: " + exception);
                        }
                    }

                    ret = e.ReturnValue;
                }
            }
            return ret;
        }

        /// <summary>
        /// Indexer for WIM images inside the WIM container, indexed by the image number.
        /// The list of Images is 0-based, but the WIM container is 1-based, so we automatically compensate for that.
        /// this[1] returns the 0th image in the WIM container.
        /// </summary>
        /// <param name="ImageIndex">The 1-based index of the image to retrieve.</param>
        /// <returns>WinImage object.</returns>
        public WimImage this[int ImageIndex] => Images[ImageIndex - 1];

        /// <summary>
        /// Indexer for WIM images inside the WIM container, indexed by the image name.
        /// WIMs created by different processes sometimes contain different information - including the name.
        /// Some images have their name stored in the Name field, some in the Flags field, and some in the EditionID field.
        /// We take all of those into account in while searching the WIM.
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public WimImage this[string imageName]
        {
            get
            {
                if (imageName == null)
                    throw new ArgumentNullException(nameof(imageName));

                return Images.Where(i => i.Name.ToUpper() == imageName.ToUpper() || i.Flags.ToUpper() == imageName.ToUpper()).FirstOrDefault();
            }
        }

        internal IntPtr CheckDisposed()
        {
            var handle = _handle;
            if (handle == null)
                throw new ObjectDisposedException("Handle");

            return handle;
        }

        public void Dispose()
        {
            foreach (WimImage image in Images)
            {
                try
                {
                    image.Dispose();
                }
                catch
                {
                }
            }

            var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
            if (handle != IntPtr.Zero)
            {
                if (_sinks.TryRemove(handle, out var file))
                {
                    NativeMethods.WIMUnregisterMessageCallback(handle, _messageCallback);
                }
                NativeMethods.WIMCloseHandle(handle);
            }
        }

        public int ImagesCount => NativeMethods.WIMGetImageCount(_handle);

        /// <summary>
        /// Provides a list of WimImage objects, representing the images in the WIM container file.
        /// </summary>
        public IReadOnlyList<WimImage> Images
        {
            get
            {
                if (_imageList == null)
                {
                    int count = ImagesCount;
                    _imageList = new List<WimImage>(count);
                    for (int i = 0; i < count; i++)
                    {
                        _imageList.Add(new WimImage(this, i + 1));
                    }
                }
                return _imageList;
            }
        }

        public static void RegisterLogfile(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            Extensions.FileCreateDirectory(path);
            if (!NativeMethods.WIMRegisterLogFile(path, 0))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public static void UnregisterLogfile(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (!NativeMethods.WIMUnregisterLogFile(path))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
