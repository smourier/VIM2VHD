using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VIM2VHD;

namespace ConvertIsoToVhdx
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = @"F:\sources\install.wim";
            using (var file = new WimFile(input))
            {
                Console.WriteLine(file.ImagesCount);

                var output = @"d:\temp\vhdx\test.vhdx";
                using (var disk = VirtualHardDisk.CreateFixedDisk(VIRTUAL_STORAGE_TYPE_DEVICE.VIRTUAL_STORAGE_TYPE_DEVICE_VHDX, output, 512 * 1024 * 1024, true))
                {
                    disk.Attach();
                    disk.Detach();
                }
            }
        }
    }
}
