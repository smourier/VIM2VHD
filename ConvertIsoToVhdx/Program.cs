using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management;
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
                var output = @"d:\temp\vhdx\test.vhdx";
                using (var vdisk = VirtualHardDisk.CreateFixedDisk(VIRTUAL_STORAGE_TYPE_DEVICE.VIRTUAL_STORAGE_TYPE_DEVICE_VHDX, output, 128 * 1024 * 1024, true))
                {
                    vdisk.Attach();

                    var disk = ManagementExtensions.GetDisk(vdisk.DiskIndex);
                    disk.Dump();

                    // https://docs.microsoft.com/en-us/previous-versions/windows/desktop/stormgmt/initialize-msft-disk
                    // initializes a RAW disk for first time use, enabling the disk to be formatted and used to store data

                    var result = disk.InvokeMethod("Initialize", new Dictionary<string, object>
                    {
                        { "PartitionStyle", 2} // GPT
                    });

                    // reread the disk
                    disk = ManagementExtensions.GetDisk(vdisk.DiskIndex);
                    disk.Dump();

                    var PARTITION_SYSTEM_GUID = "{c12a7328-f81f-11d2-ba4b-00a0c93ec93b}";

                    result = disk.InvokeMethod("CreatePartition", new Dictionary<string, object>
                    {
                        { "Size", 100 * 1024 * 1024 },
                        { "GptType", PARTITION_SYSTEM_GUID }
                    });

                    var systemPartition = (ManagementBaseObject)result["CreatedPartition"];
                    systemPartition.Dump();

                    var systemVolume = ManagementExtensions.GetVolumePath(systemPartition);

                    // reread the disk
                    disk = ManagementExtensions.GetDisk(vdisk.DiskIndex);
                    disk.Dump();

                    var PARTITION_BASIC_DATA_GUID = "{ebd0a0a2-b9e5-4433-87c0-68b6b72699c7}";

                    result = disk.InvokeMethod("CreatePartition", new Dictionary<string, object>
                    {
                        { "UseMaximumSize", true },
                        { "GptType", PARTITION_BASIC_DATA_GUID }
                    });

                    var partition = (ManagementBaseObject)result["CreatedPartition"];
                    partition.Dump();

                    vdisk.Detach();
                }
            }
        }
    }
}
