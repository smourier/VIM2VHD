using System;
using System.IO;

namespace VIM2VHD
{
    internal static class Extensions
    {
        public static void FileCreateDirectory(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            if (!Path.IsPathRooted(filePath))
            {
                filePath = Path.GetFullPath(filePath);
            }

            string dir = Path.GetDirectoryName(filePath);
            if (dir == null || Directory.Exists(dir))
                return;

            Directory.CreateDirectory(dir);
        }
    }
}
