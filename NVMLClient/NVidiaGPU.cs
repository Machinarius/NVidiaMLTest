using System;
using System.Collections.Generic;

namespace NvidiaMLClient
{
    public class NVidiaGPU
    {
        private readonly IntPtr _deviceHandle;

        public int Index { get; private set; }

        private NVidiaGPU(IntPtr deviceHandle, int index)
        {
            _deviceHandle = deviceHandle;
            Index = index;
        }

        public static IEnumerable<NVidiaGPU> GetAllGPUs()
        {
            for (var index = 0; index < NVML.GetDeviceCount(); index++)
            {
                var deviceHandle = NVML.GetDeviceHandleByIndex(index);
                yield return new NVidiaGPU(deviceHandle, index + 1);
            }
        }

        public string Name => NVML.GetDeviceName(_deviceHandle);
        public string ArchitectureName => NVML.GetArchitecture(_deviceHandle).ToString();
        public uint TemperatureInC => NVML.GetTemperature(_deviceHandle);
        public string VBiosVersion => NVML.GetVBiosVersion(_deviceHandle);
        public MemoryInformation MemoryConsumption => new MemoryInformation(NVML.GetMemoryInfo(_deviceHandle));

        public class MemoryInformation
        {
            private readonly ulong _free;
            private readonly ulong _total;
            private readonly ulong _used;

            public MemoryInformation((ulong Free, ulong Total, ulong Used) tempData)
            {
                _free = tempData.Free;
                _total = tempData.Total;
                _used = tempData.Used;
            }

            public ulong FreeInMegabytes => _free / 1000000;
            public ulong TotalInMegabytes => _total / 1000000;
            public ulong UsedInMegabytes => _used / 1000000;
        }
    }
}
