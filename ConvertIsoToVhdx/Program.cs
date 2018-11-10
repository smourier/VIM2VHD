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
            var temp = @"F:\sources\install.wim";
            using (var file = new WimFile(temp))
            {
                Console.WriteLine(file.ImagesCount);
            }
        }
    }
}
