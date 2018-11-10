using System;
using System.Runtime.InteropServices;

namespace VIM2VHD
{
    ///<summary>
    ///Describes the file that is being processed for the ProcessFileEvent.
    ///</summary>
    public class ProcessFileEventArgs : EventArgs
    {
        ///<summary>
        ///Default constructor.
        ///</summary>
        ///<param name="file">Fully qualified path and file name. For example: c:\file.sys.</param>
        ///<param name="skipFileFlag">Default is false - skip file and continue.
        ///Set to true to abort the entire image capture.</param>
        public
        ProcessFileEventArgs(
            string file,
            IntPtr skipFileFlag)
        {

            m_FilePath = file;
            m_SkipFileFlag = skipFileFlag;
        }

        ///<summary>
        ///Skip file from being imaged.
        ///</summary>
        public void
        SkipFile()
        {
            byte[] byteBuffer = {
                    0
            };
            int byteBufferSize = byteBuffer.Length;
            Marshal.Copy(byteBuffer, 0, m_SkipFileFlag, byteBufferSize);
        }

        ///<summary>
        ///Fully qualified path and file name.
        ///</summary>
        public string
        FilePath
        {
            get
            {
                string stringToReturn = "";
                if (m_FilePath != null)
                {
                    stringToReturn = m_FilePath;
                }
                return stringToReturn;
            }
        }

        ///<summary>
        ///Flag to indicate if the entire image capture should be aborted.
        ///Default is false - skip file and continue. Setting to true will
        ///abort the entire image capture.
        ///</summary>
        public bool Abort
        {
            set { m_Abort = value; }
            get { return m_Abort; }
        }

        private string m_FilePath;
        private bool m_Abort;
        private IntPtr m_SkipFileFlag;

    }
}
