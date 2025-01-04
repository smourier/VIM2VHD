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
        static int _percentLeft;
        static int _percentTop;
        static ManagementObject _iso;
        static string inputFilePath;
        static string outputFilePath;

        static void Main(string[] args)
        {
            Console.WriteLine("ConvertIsoToVhdx - Copyright (C) 2017-" + DateTime.Now.Year + " Simon Mourier. All rights reserved.");
            Console.WriteLine();

            if (CommandLine.HelpRequested || args.Length < 2)
            {
                Help();
                return;
            }

            inputFilePath = CommandLine.GetNullifiedArgument(0);
            outputFilePath = CommandLine.GetNullifiedArgument(1);
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
            int systemPartitionSizeInMB = Math.Max(CommandLine.GetArgument("systemPartitionSizeInMB", 0), 100);

            if (_logFilePath != null)
            {
                _logFilePath = Path.GetFullPath(_logFilePath);
                WimFile.RegisterLogfile(_logFilePath);
                Console.WriteLine("Logging Imaging information to log file: " + _logFilePath);
            }

            Console.WriteLine();
            Console.CancelKeyPress += OnConsoleCancelKeyPress;
            _iso = ManagementExtensions.MountDiskImage(inputFilePath, out var driveLetter);
            Console.WriteLine(inputFilePath + " has been mounted as drive '" + driveLetter + "'.");
            try
            {
                var input = driveLetter + @":\sources\install.wim";
                if (!File.Exists(input))
                {
                    Console.WriteLine("Error: windows image file at '" + input + "' was not found.");
                    return;
                }

                var options = new WimFileOpenOptions();
                options.RegisterForEvents = true;
                Console.WriteLine("Opening windows image file '" + input + "'.");

                using (var file = new WimFile(input, options))
                {
                    if (file.ImagesCount == 0)
                    {
                        Console.WriteLine("Error: windows image file at '" + input + "' does not contain any image.");
                        return;
                    }

                    file.Event += OnFileEvent;
                    var diskSize = 512 * (file.Images[0].Size / 512);
                    Console.WriteLine("Creating virtual disk '" + outputFilePath + "'. Maximum size: " + diskSize + " (" + Conversions.FormatByteSize(diskSize) + ")");
                    using (var vdisk = VirtualHardDisk.CreateDisk(outputFilePath, diskSize, IntPtr.Zero, true))
                    {
                        vdisk.Attach();

                        var disk = ManagementExtensions.GetDisk(vdisk.DiskIndex);
                        Console.WriteLine("Virtual disk path: " + disk["Path"]);
                        var size = (ulong)disk["Size"];
                        Console.WriteLine("Virtual disk size: " + size + " bytes (" + Conversions.FormatByteSize(size) + ")");
                        //disk.Dump();

                        var result = disk.InvokeMethod("Initialize", new Dictionary<string, object>
                            {
                                { "PartitionStyle", 2} // GPT
                            });

                        // reread the disk
                        disk = ManagementExtensions.GetDisk(vdisk.DiskIndex);
                        Console.WriteLine("Virtual disk partition style: " + disk["PartitionStyle"]);
                        //disk.Dump();

                        //var PARTITION_SYSTEM_GUID = "{c12a7328-f81f-11d2-ba4b-00a0c93ec93b}";

                        //// https://docs.microsoft.com/en-us/windows-hardware/manufacture/desktop/configure-uefigpt-based-hard-drive-partitions
                        //result = disk.InvokeMethod("CreatePartition", new Dictionary<string, object>
                        //    {
                        //        { "Size", systemPartitionSizeInMB * 1024 * 1024L },
                        //        { "GptType", PARTITION_SYSTEM_GUID }
                        //    });

                        //var systemPartition = (ManagementBaseObject)result["CreatedPartition"];
                        //Console.WriteLine("System partition GPT Type: " + systemPartition["GptType"]);
                        ////systemPartition.Dump();

                        //var systemVolume = ManagementExtensions.GetPartitionVolume(systemPartition);
                        //systemVolume.InvokeMethod("Format", new Dictionary<string, object>
                        //    {
                        //        { "FileSystem", "FAT32" },
                        //        //{ "Force", true },
                        //        //{ "Full", false },
                        //        { "Compress", true }
                        //    });

                        //// reread the disk
                        //disk = ManagementExtensions.GetDisk(vdisk.DiskIndex);
                        ////disk.Dump();

                        var PARTITION_BASIC_DATA_GUID = "{ebd0a0a2-b9e5-4433-87c0-68b6b72699c7}";

                        result = disk.InvokeMethod("CreatePartition", new Dictionary<string, object>
                            {
                                { "UseMaximumSize", true },
                                //{ "AssignDriveLetter", true },
                                { "GptType", PARTITION_BASIC_DATA_GUID }
                            });

                        var partition = (ManagementBaseObject)result["CreatedPartition"];
                        //partition.Dump();
                        Console.WriteLine("Data partition GPT Type: " + partition["GptType"]);

                        var volume = ManagementExtensions.GetPartitionVolume(partition);
                        volume.InvokeMethod("Format", new Dictionary<string, object>
                            {
                                { "FileSystem", "NTFS" },
                                { "Compress", true }
                                //{ "Force", true },
                                //{ "Full", false }
                            });

                        //volume.Dump();
                        Console.WriteLine("Data volume path: " + volume["Path"]);
                        Console.WriteLine("Applying...");
                        Console.WriteLine();
                        Console.WriteLine("Completed 00%");

                        int col = 10;
                        int fixedLines = 1;
                        _percentLeft = col;
                        _percentTop = Console.CursorTop - fixedLines;

                        Console.CursorVisible = false;
                        file.Images[0].Apply((string)volume["Path"]);
                    }
                }
            }
            finally
            {
                Console.CursorVisible = true;
                Console.WriteLine();
                ManagementExtensions.UnmountDiskImage(_iso);
                Console.WriteLine(inputFilePath + " has been unmounted.");
            }
        }

        private static void OnFileEvent(object sender, WimFileEventArgs e)
        {
            if (e is WimFileProgressEventArgs pp)
            {
                Console.SetCursorPosition(_percentLeft, _percentTop);
                Console.Write(pp.Percent.ToString("D2") + "%");
            }
            else if ((int)e.Message <= (int)WIM_MSG.WIM_MSG_QUERY_ABORT)
            {
                //Console.WriteLine("Event " + e.Message + " (0x" + ((int)e.Message).ToString("X4") + ")");
            }
        }

        private static void OnConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("Aborting...");

            if (_iso != null)
            {
                ManagementExtensions.UnmountDiskImage(_iso);
                _iso = null;
                Console.WriteLine(inputFilePath + " has been unmounted.");
            }

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
