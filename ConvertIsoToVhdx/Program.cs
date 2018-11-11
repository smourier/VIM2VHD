using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Reflection;
using VIM2VHD;

namespace ConvertIsoToVhdx
{
    class Program
    {
        static string _logFilePath;

        static void Main(string[] args)
        {
            Console.WriteLine("ConvertIsoToVhdx - Copyright (C) 2017-" + DateTime.Now.Year + " Simon Mourier. All rights reserved.");
            Console.WriteLine();

            if (CommandLine.HelpRequested || args.Length < 2)
            {
                Help();
                return;
            }

            string inputFilePath = CommandLine.GetNullifiedArgument(0);
            string outputFilePath = CommandLine.GetNullifiedArgument(1);
            if (inputFilePath == null || outputFilePath == null)
            {
                Help();
                return;
            }

            inputFilePath = Path.GetFullPath(inputFilePath);
            outputFilePath = Path.GetFullPath(outputFilePath);
            Console.WriteLine("Input file: " + inputFilePath);
            Console.WriteLine("Output file: " + outputFilePath);

            _logFilePath = CommandLine.GetNullifiedArgument("log");

            if (_logFilePath != null)
            {
                _logFilePath = Path.GetFullPath(_logFilePath);
                WimFile.RegisterLogfile(_logFilePath);
                Console.WriteLine("Logging Imaging information to log file: " + _logFilePath);
            }

            Console.CancelKeyPress += OnConsoleCancelKeyPress;
            var iso = ManagementExtensions.MountDiskImage(inputFilePath, out var driveLetter);
            try
            {
                var input = driveLetter + @":\sources\install.wim";
                var options = new WimFileOpenOptions();
                options.RegisterForEvents = true;
                using (var file = new WimFile(input, options))
                {
                    file.Event += OnFileEvent;
                    var output = @"d:\temp\vhdx\test.vhdx";
                    using (var vdisk = VirtualHardDisk.CreateFixedDisk(VIRTUAL_STORAGE_TYPE_DEVICE.VIRTUAL_STORAGE_TYPE_DEVICE_VHDX, output, 130 * 1024 * 1024L, true))
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

        private static void OnFileEvent(object sender, WimFileEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (e is WimFileProcessEventArgs pe)
            {
                Console.WriteLine("Processing path: " + pe.RelativePath);
            }
            else if (e is WimFileErrorEventArgs ee)
            {
                Console.WriteLine("Error processing path: " + ee.RelativePath);
                Console.WriteLine(" Error: " + ee.ErrorCode);
            }
            else
            {
                Console.WriteLine("Event 0x" + ((int)e.Message).ToString("X4"));
            }
            Console.ResetColor();
        }

        private static void OnConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (_logFilePath != null)
            {
                WimFile.UnregisterLogfile(_logFilePath);
            }
        }

        static void Help()
        {
            Console.WriteLine(Assembly.GetEntryAssembly().GetName().Name.ToUpperInvariant() + " <input file path> <output file path> [Options]");
            Console.WriteLine();
            Console.WriteLine("Description:");
            Console.WriteLine("    This tool is used to convert Windows installation .ISO files to bootable VHDX files.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("    /log:<logfilepath>   Defines a log file path for Imaging operations.");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine();
            Console.WriteLine("    " + Assembly.GetEntryAssembly().GetName().Name.ToUpperInvariant() + " c:\\mypath\\myproject\\win8.iso c:\\mypath\\myproject\\win8.vhdx");
            Console.WriteLine();
        }
    }
}
