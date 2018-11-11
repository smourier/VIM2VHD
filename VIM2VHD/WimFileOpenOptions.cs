namespace VIM2VHD
{
    public class WimFileOpenOptions
    {
        public WIM_COMPRESSION_TYPE CompressionType { get; set; }
        public string TempDirectoryPath { get; set; }
        public bool RegisterForEvents { get; set; }
        public WIM_FLAG Flags { get; set; }
    }
}
