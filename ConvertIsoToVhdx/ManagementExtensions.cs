using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace ConvertIsoToVhdx
{
    public static class ManagementExtensions
    {
        public static ManagementBaseObject InvokeMethod(this ManagementObject obj, string name, IDictionary<string, object> arguments = null, bool throwOnError = true)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var parameters = obj.GetMethodParameters(name);
            if (arguments != null)
            {
                foreach (var kv in arguments)
                {
                    parameters[kv.Key] = kv.Value;
                }
            }

            var ret = obj.InvokeMethod(name, parameters, null);
            var returnValue = (uint)ret["ReturnValue"];
            if (returnValue != 0 && throwOnError)
                throw new ManagementException("Error calling method '" + name + "'. ReturnValue: " + returnValue);

            return ret;
        }

        public static ManagementObject MountDiskImage(string filePath) => MountDiskImage(filePath, out var driveLetter);
        public static ManagementObject MountDiskImage(string filePath, out char driveLetter)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            var cls = new ManagementClass(@"ROOT\Microsoft\Windows\Storage:MSFT_DiskImage");
            var mo = cls.CreateInstance();
            mo["ImagePath"] = filePath;
            mo["StorageType"] = (uint)1; // ISO
            mo.Get();
            var result = mo.InvokeMethod("Mount", new Dictionary<string, object>
                    {
                        { "Access", 3 }, // readonly
                        { "NoDriveLetter", false }
                    });

            var di = (ManagementBaseObject)result["DiskImage"];
            driveLetter = GetDiskImageVolumeDriveLetter(di);
            return mo;
        }

        public static void UnmountDiskImage(ManagementObject diskImage)
        {
            if (diskImage == null)
                throw new ArgumentNullException(nameof(diskImage));

            diskImage.InvokeMethod("Dismount");
        }

        public static char GetDiskImageVolumeDriveLetter(ManagementBaseObject diskImage)
        {
            if (diskImage == null)
                throw new ArgumentNullException(nameof(diskImage));

            var imagePath = (string)diskImage["ImagePath"];
            var storageType = (uint)diskImage["StorageType"];
            var volume = new ManagementObjectSearcher(@"ROOT\Microsoft\Windows\Storage", "ASSOCIATORS OF {MSFT_DiskImage.ImagePath='" + imagePath + "',StorageType=" + storageType + "} WHERE AssocClass = MSFT_DiskImageToVolume ResultClass = MSFT_Volume").Get().OfType<ManagementObject>().First();
            return (char)volume["DriveLetter"];
        }

        public static ManagementObject GetDisk(int number) => new ManagementObjectSearcher(@"ROOT\Microsoft\Windows\Storage", "SELECT * FROM MSFT_Disk WHERE Number=" + number).Get().OfType<ManagementObject>().FirstOrDefault();

        public static ManagementObject GetPartitionVolume(ManagementBaseObject partition)
        {
            if (partition == null)
                throw new ArgumentNullException(nameof(partition));

            var relPath = (string)partition["__RELPATH"];
            foreach (var obj in new ManagementObjectSearcher(@"ROOT\Microsoft\Windows\Storage", "SELECT * FROM MSFT_PartitionToVolume").Get().OfType<ManagementObject>())
            {
                var part = (string)obj["Partition"];
                var volume = (string)obj["Volume"];
                if (part.EndsWith(":" + relPath, StringComparison.OrdinalIgnoreCase))
                    return new ManagementObject(volume);
            }
            return null;
        }

        public static void Dump(this ManagementBaseObject obj)
        {
            if (obj == null)
                return;

            var list = obj.Properties.Cast<PropertyData>().OrderBy(p => p.Name).ToList();
            foreach (var prop in list)
            {
                var value = prop.Value;
                if (value is Array array)
                {
                    value = string.Join(", ", ((IEnumerable)array).Cast<object>());
                    Console.WriteLine(prop.Name + " = [] " + value);
                }
                else
                {
                    Console.WriteLine(prop.Name + " = " + value);
                }
            }
        }
    }
}
