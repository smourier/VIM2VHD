using System;
using System.Runtime.InteropServices;

namespace VIM2VHD
{
    ///<summary>
    ///Describes the file that is being processed for the ProcessFileEvent.
    ///</summary>
    public class ProcessFileEventArgs : EventArgs
    {
        private string _filePath;
        private IntPtr _skipFileFlag;

        ///<summary>
        ///Default constructor.
        ///</summary>
        ///<param name="file">Fully qualified path and file name. For example: c:\file.sys.</param>
        ///<param name="skipFileFlag">Default is false - skip file and continue.
        ///Set to true to abort the entire image capture.</param>
        public ProcessFileEventArgs(string file, IntPtr skipFileFlag)
        {
            _filePath = file;
            _skipFileFlag = skipFileFlag;
        }

        ///<summary>
        ///Fully qualified path and file name.
        ///</summary>
        public string FilePath => _filePath ?? string.Empty;

        ///<summary>
        ///Flag to indicate if the entire image capture should be aborted.
        ///Default is false - skip file and continue. Setting to true will
        ///abort the entire image capture.
        ///</summary>
        public bool Abort { get; set; }

        ///<summary>
        ///Skip file from being imaged.
        ///</summary>
        public void SkipFile()
        {
            byte[] byteBuffer = { 0 };
            int byteBufferSize = byteBuffer.Length;
            Marshal.Copy(byteBuffer, 0, _skipFileFlag, byteBufferSize);
        }
    }
}
