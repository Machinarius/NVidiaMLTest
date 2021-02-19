using System;
using System.Linq;

namespace NvidiaMLTest
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var allDevices = NVidiaGPU.GetAllGPUs();
            Console.WriteLine("You've got {0} NVidia GPUs", allDevices.Count());
            allDevices.ToList().ForEach(PrintDeviceInfo);
        }

        public static void PrintDeviceInfo(NVidiaGPU gpu)
        {
            Console.WriteLine("GPU {0} - {1} ({2})", gpu.Index, gpu.Name, gpu.ArchitectureName);
            Console.WriteLine("VBios Version: {0}", gpu.VBiosVersion);
            Console.WriteLine("Current Temperature: {0}c", gpu.TemperatureInC);

            var memory = gpu.MemoryConsumption;
            Console.WriteLine("Total Memory: {0}Mb, Used Memory: {1}Mb, Free Memory: {2}Mb", memory.TotalInMegabytes, memory.UsedInMegabytes, memory.FreeInMegabytes);
        }
    }
}
