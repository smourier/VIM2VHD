using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace VIM2VHD
{
    public class WimFile
    {
        internal XDocument _xmlInfo;
        internal List<WimImage> _imageList;

        private static WimMessageCallback wimMessageCallback;

        ///<summary>
        ///Enable the caller to prevent a file resource from being compressed during a capture.
        ///</summary>
        public event ProcessFileEventHandler ProcessFileEvent;

        ///<summary>
        ///Indicate an update in the progress of an image application.
        ///</summary>
        public event DefaultImageEventHandler ProgressEvent;

        ///<summary>
        ///Alert the caller that an error has occurred while capturing or applying an image.
        ///</summary>
        public event DefaultImageEventHandler ErrorEvent;

        ///<summary>
        ///Indicate that a file has been either captured or applied.
        ///</summary>
        public event DefaultImageEventHandler StepItEvent;

        ///<summary>
        ///Indicate the number of files that will be captured or applied.
        ///</summary>
        public event DefaultImageEventHandler SetRangeEvent;

        ///<summary>
        ///Indicate the number of files that have been captured or applied.
        ///</summary>
        public event DefaultImageEventHandler SetPosEvent;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="wimPath">Path to the WIM container.</param>
        public WimFile(string wimPath)
        {
            if (wimPath == null)
                throw new ArgumentNullException(nameof(wimPath));

            if (!File.Exists(Path.GetFullPath(wimPath)))
                throw new FileNotFoundException((new FileNotFoundException()).Message, wimPath);

            Handle = new WimFileHandle(wimPath);

            // Hook up the events before we return.
            //wimMessageCallback = new NativeMethods.WimMessageCallback(ImageEventMessagePump);
            //NativeMethods.RegisterMessageCallback(this.Handle, wimMessageCallback);
        }

        public WimFileHandle Handle { get; }

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
        /// <param name="ImageName"></param>
        /// <returns></returns>
        public WimImage this[string ImageName]
        {
            get
            {
                return Images.Where(i => (
                        i.ImageName.ToUpper() == ImageName.ToUpper() ||
                        i.ImageFlags.ToUpper() == ImageName.ToUpper()))
                        .DefaultIfEmpty(null)
                        .FirstOrDefault();
            }
        }

        /// <summary>
        /// Returns the number of images in the WIM container.
        /// </summary>
        internal int ImageCount => NativeMethods.WimGetImageCount(Handle);

        /// <summary>
        /// Returns an XDocument representation of the XML metadata for the WIM container and associated images.
        /// </summary>
        internal XDocument XmlInfo
        {
            get
            {
                if (null == _xmlInfo)
                {
                    if (!NativeMethods.WimGetImageInformation(Handle, out StringBuilder builder, out uint bytes))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    // Ensure the length of the returned bytes to avoid garbage characters at the end.
                    int charCount = (int)bytes / sizeof(char);
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
        private int ImageEventMessagePump(uint MessageId, IntPtr wParam, IntPtr lParam, IntPtr UserData)
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

        /// <summary>
        /// Closes the WIM file.
        /// </summary>
        public void Close()
        {
            foreach (WimImage image in Images)
            {
                image.Close();
            }

            if (null != wimMessageCallback)
            {
                NativeMethods.UnregisterMessageCallback(this.Handle, wimMessageCallback);
                wimMessageCallback = null;
            }

            if (!Handle.IsClosed && !Handle.IsInvalid)
            {
                Handle.Close();
            }
        }

        /// <summary>
        /// Provides a list of WimImage objects, representing the images in the WIM container file.
        /// </summary>
        public IReadOnlyList<WimImage> Images
        {
            get
            {
                if (_imageList == null)
                {
                    int imageCount = (int)ImageCount;
                    _imageList = new List<WimImage>(imageCount);
                    for (int i = 0; i < imageCount; i++)
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
