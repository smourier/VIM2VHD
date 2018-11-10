using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace VIM2VHD
{
    public class WimFile : IDisposable
    {
        internal IntPtr _handle;
        internal XDocument _xmlInfo;
        internal List<WimImage> _imageList;

        //private static WimMessageCallback _wimMessageCallback;

        ///<summary>
        ///Enable the caller to prevent a file resource from being compressed during a capture.
        ///</summary>
        public event EventHandler<ProcessFileEventArgs> ProcessFileEvent;

        ///<summary>
        ///Indicate an update in the progress of an image application.
        ///</summary>
        public event EventHandler<DefaultImageEventArgs> ProgressEvent;

        ///<summary>
        ///Alert the caller that an error has occurred while capturing or applying an image.
        ///</summary>
        public event EventHandler<DefaultImageEventArgs> ErrorEvent;

        ///<summary>
        ///Indicate that a file has been either captured or applied.
        ///</summary>
        public event EventHandler<DefaultImageEventArgs> StepItEvent;

        ///<summary>
        ///Indicate the number of files that will be captured or applied.
        ///</summary>
        public event EventHandler<DefaultImageEventArgs> SetRangeEvent;

        ///<summary>
        ///Indicate the number of files that have been captured or applied.
        ///</summary>
        public event EventHandler<DefaultImageEventArgs> SetPosEvent;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filePath">Path to the WIM container.</param>
        public WimFile(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            _handle = NativeMethods.WIMCreateFile(
                filePath,
                NativeMethods.WimCreateFileDesiredAccess.WimGenericRead,
                NativeMethods.WimCreationDisposition.WimOpenExisting,
                NativeMethods.WimActionFlags.WimIgnored,
                NativeMethods.WimCompressionType.WimIgnored,
                out var creationResult
            );
            if (_handle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            if (creationResult != NativeMethods.WimCreationResult.WimOpenedExisting)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            NativeMethods.WIMSetTemporaryPath(_handle, Environment.ExpandEnvironmentVariables("%TEMP%"));

            // Hook up the events before we return.
            //_wimMessageCallback = new WimMessageCallback(ImageEventMessagePump);
            //NativeMethods.RegisterMessageCallback(_handle, _wimMessageCallback);
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

                return Images.Where(i => i.ImageName.ToUpper() == imageName.ToUpper() || i.ImageFlags.ToUpper() == imageName.ToUpper()).FirstOrDefault();
            }
        }

        /// <summary>
        /// Returns an XDocument representation of the XML metadata for the WIM container and associated images.
        /// </summary>
        private XDocument XmlInfo
        {
            get
            {
                if (_xmlInfo == null)
                {
                    if (!NativeMethods.WIMGetImageInformation(CheckDisposed(), out StringBuilder builder, out int bytes))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    // Ensure the length of the returned bytes to avoid garbage characters at the end.
                    int charCount = bytes / sizeof(char);
                    if (null != builder)
                    {
                        // Get rid of the unicode file marker at the beginning of the XML.
                        builder.Remove(0, 1);
                        builder.EnsureCapacity(charCount - 1);
                        builder.Length = charCount - 1;

                        // This isn't likely to change while we have the image open, so cache it.
                        _xmlInfo = XDocument.Parse(builder.ToString().Trim());
                    }
                    else
                    {
                        _xmlInfo = null;
                    }
                }

                return _xmlInfo;
            }
        }

        ///<summary>
        ///Event callback to the Wimgapi events
        ///</summary>
        private int ImageEventMessagePump(int MessageId, IntPtr wParam, IntPtr lParam, IntPtr UserData)
        {
            int status = (int)WimMessage.WIM_MSG_SUCCESS;
            var eventArgs = new DefaultImageEventArgs(wParam, lParam, UserData);
            switch ((ImageEventMessage)MessageId)
            {
                case ImageEventMessage.Progress:
                    ProgressEvent(this, eventArgs);
                    break;

                case ImageEventMessage.Process:
                    if (ProcessFileEvent != null)
                    {
                        string fileToImage = Marshal.PtrToStringUni(wParam);
                        var fileToProcess = new ProcessFileEventArgs(fileToImage, lParam);
                        ProcessFileEvent(this, fileToProcess);

                        if (fileToProcess.Abort == true)
                        {
                            status = (int)ImageEventMessage.Abort;
                        }
                    }
                    break;

                case ImageEventMessage.Error:
                    ErrorEvent?.Invoke(this, eventArgs);
                    break;

                case ImageEventMessage.SetRange:
                    SetRangeEvent?.Invoke(this, eventArgs);
                    break;

                case ImageEventMessage.SetPos:
                    SetPosEvent?.Invoke(this, eventArgs);
                    break;

                case ImageEventMessage.StepIt:
                    StepItEvent?.Invoke(this, eventArgs);
                    break;

                default:
                    break;
            }
            return status;
        }

        private IntPtr CheckDisposed()
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
                        // Load up each image so it's ready for us.
                        _imageList.Add(new WimImage(this, i + 1));
                    }
                }
                return _imageList;
            }
        }

        private enum ImageEventMessage
        {
            ///<summary>
            ///Enables the caller to prevent a file or a directory from being captured or applied.
            ///</summary>
            Progress = WimMessage.WIM_MSG_PROGRESS,

            ///<summary>
            ///Notification sent to enable the caller to prevent a file or a directory from being captured or applied.
            ///To prevent a file or a directory from being captured or applied, call WindowsImageContainer.SkipFile().
            ///</summary>
            Process = WimMessage.WIM_MSG_PROCESS,

            ///<summary>
            ///Enables the caller to prevent a file resource from being compressed during a capture.
            ///</summary>
            Compress = WimMessage.WIM_MSG_COMPRESS,

            ///<summary>
            ///Alerts the caller that an error has occurred while capturing or applying an image.
            ///</summary>
            Error = WimMessage.WIM_MSG_ERROR,

            ///<summary>
            ///Enables the caller to align a file resource on a particular alignment boundary.
            ///</summary>
            Alignment = WimMessage.WIM_MSG_ALIGNMENT,

            ///<summary>
            ///Enables the caller to align a file resource on a particular alignment boundary.
            ///</summary>
            Split = WimMessage.WIM_MSG_SPLIT,

            ///<summary>
            ///Indicates that volume information is being gathered during an image capture.
            ///</summary>
            Scanning = WimMessage.WIM_MSG_SCANNING,

            ///<summary>
            ///Indicates the number of files that will be captured or applied.
            ///</summary>
            SetRange = WimMessage.WIM_MSG_SETRANGE,

            ///<summary>
            ///Indicates the number of files that have been captured or applied.
            /// </summary>
            SetPos = WimMessage.WIM_MSG_SETPOS,

            ///<summary>
            ///Indicates that a file has been either captured or applied.
            ///</summary>
            StepIt = WimMessage.WIM_MSG_STEPIT,

            ///<summary>
            ///Success.
            ///</summary>
            Success = WimMessage.WIM_MSG_SUCCESS,

            ///<summary>
            ///Abort.
            ///</summary>
            Abort = WimMessage.WIM_MSG_ABORT_IMAGE
        }
    }
}
