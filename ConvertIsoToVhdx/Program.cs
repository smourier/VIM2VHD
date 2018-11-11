using System;
using System.Collections.Generic;
using System.Management;
using VIM2VHD;

namespace ConvertIsoToVhdx
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"win8.iso";
            var iso = ManagementExtensions.MountDiskImage(path, out var driveLetter);
            try
            {
                var input = driveLetter + @":\sources\install.wim";
                using (var file = new WimFile(input))
                {
                    var output = @"d:\temp\vhdx\test.vhdx";
                    using (var vdisk = VirtualHardDisk.CreateFixedDisk(VIRTUAL_STORAGE_TYPE_DEVICE.VIRTUAL_STORAGE_TYPE_DEVICE_VHDX, output, 3000 * 1024 * 1024L, true))
                    {
                        vdisk.Attach();

                        var disk = ManagementExtensions.GetDisk(vdisk.DiskIndex);
                        disk.Dump();

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

                        var systemVolume = ManagementExtensions.GetPartitionVolume(systemPartition);
                        systemVolume.InvokeMethod("Format", new Dictionary<string, object>
                            {
                                { "FileSystem", "FAT32" },
                                { "Force", true },
                                { "Full", false }
                            });

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
                        Console.WriteLine(" *** PART");
                        partition.Dump();

                        var volume = ManagementExtensions.GetPartitionVolume(partition);
                        volume.InvokeMethod("Format", new Dictionary<string, object>
                            {
                                { "FileSystem", "NTFS" },
                                { "Force", true },
                                { "Full", false }
                            });

                        Console.WriteLine(" *** VOLUME");
                        volume.Dump();

                        file.Images[0].Apply((string)volume["Path"]);
                    }
                }
            }
            finally
            {
                ManagementExtensions.UnmountDiskImage(iso);
            }
        }
    }
}
