using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace NvidiaMLClient.PInvoke
{
    public static class NVMLFunctions
    {
        static NVMLFunctions()
        {
            NVMLLocator.EnsureNVMLCanBeLocated();
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
        }

        // References:
        // https://github.com/dotnet/samples/blob/master/core/extensions/DllMapDemo/Map.cs
        // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.dllimportresolver?view=net-5.0
        private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName == "nvml.dll")
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return NativeLibrary.Load("libnvidia-ml.so.1");
                }

                return NativeLibrary.Load(libraryName);
            }

            throw new InvalidOperationException("This mapper may only map NVML libraries");
        }

        // https://github.com/NVIDIA/gpu-monitoring-tools/blob/master/bindings/go/nvml/nvml.h
        // Interesting link on calling conventions: https://stackoverflow.com/a/15664100/528131

        [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlInitWithFlags")]
        public static extern NVMLReturnCodes Initialize();

        [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlDeviceGetCount_v2")]
        public static extern NVMLReturnCodes GetDeviceCount(ref int deviceCount);

        [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlDeviceGetHandleByIndex_v2")]
        public static extern NVMLReturnCodes GetDeviceHandleByIndex(int index, out IntPtr handle);

        /// <summary>
        /// Buffer size guaranteed to be large enough for nvmlDeviceGetName
        /// </summary>
        public static readonly int DeviceNameBufferSize = 64;

        [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlDeviceGetName")]
        public static extern NVMLReturnCodes GetDeviceName(IntPtr handle, StringBuilder buffer, int length);

        [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlDeviceGetArchitecture")]
        public static extern NVMLReturnCodes GetArchitecture(IntPtr handle, out Architectures architecture);

        [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlDeviceGetMemoryInfo")]
        public static extern NVMLReturnCodes GetMemoryInfo(IntPtr handle, out MemoryInfo memInfo);

        [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlDeviceGetTemperature")]
        public static extern NVMLReturnCodes GetTemperature(IntPtr handle, TemperatureSensors sensor, out uint tempValue);

        /// <summary>
        /// Buffer size guaranteed to be large enough for nvmlDeviceGetVbiosVersion
        /// </summary>
        public static readonly int VBiosVersionBufferSize = 32;

        [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlDeviceGetVbiosVersion")]
        public static extern NVMLReturnCodes GetVBiosVersion(IntPtr handle, StringBuilder buffer, int length);

        // https://stackoverflow.com/a/370093/528131
        [DllImport("nvml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlErrorString")]
        public static extern IntPtr GetErrorString(NVMLReturnCodes returnCode);
    }
}
