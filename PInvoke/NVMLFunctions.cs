using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NvidiaMLTest.PInvoke
{
    public static class NVMLFunctions
    {
        public enum ReturnCodes
        {
            Success = 0,
            Uninitialized = 1,
            InvalidArgument = 2,
            NotSupported = 3,
            NoPermission = 4,
            AlreadyInitialized = 5,
            NotFound = 6,
            InsufficientSize = 7,
            InsufficientPower = 8,
            DriverNotLoaded = 9,
            Timeout = 10,
            IRQIssue = 11,
            LibraryNotFound = 12,
            FunctionNotFound = 13,
            CorruptedInfoROM = 14,
            GPUIsLost = 15,
            ResetIsRequired = 16,
            OperatingSystem = 17,
            RMVersionMismatch = 18,
            InUse = 19,
            Memory = 20,
            NoData = 21,
            VGPUECCNotSupported = 22,
            InsufficientResources = 23,
            Unknown = 999
        }

        public enum Architectures
        {
            Unknown,
            Kepler = 2,
            Maxwell = 3,
            Pascal = 4,
            Volta = 5,
            Turing = 6,
            Ampere = 7
        }

        public enum TemperatureSensors
        {
            GPUSensor = 0
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MemoryInfo
        {
            public readonly ulong Free;
            public readonly ulong Total;
            public readonly ulong Used;
        }

        // https://github.com/NVIDIA/gpu-monitoring-tools/blob/master/bindings/go/nvml/nvml.h
        // Interesting link on calling conventions: https://stackoverflow.com/a/15664100/528131

        [DllImport("nvidia-ml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlInitWithFlags")]
        public static extern ReturnCodes Initialize();

        [DllImport("nvidia-ml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlDeviceGetCount_v2")]
        public static extern ReturnCodes GetDeviceCount(ref int deviceCount);

        [DllImport("nvidia-ml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlDeviceGetHandleByIndex_v2")]
        public static extern ReturnCodes GetDeviceHandleByIndex(int index, out IntPtr handle);

        /// <summary>
        /// Buffer size guaranteed to be large enough for nvmlDeviceGetName
        /// </summary>
        public static readonly int DeviceNameBufferSize = 64;

        [DllImport("nvidia-ml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlDeviceGetName")]
        public static extern ReturnCodes GetDeviceName(IntPtr handle, StringBuilder buffer, int length);

        [DllImport("nvidia-ml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlDeviceGetArchitecture")]
        public static extern ReturnCodes GetArchitecture(IntPtr handle, out Architectures architecture);

        [DllImport("nvidia-ml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlDeviceGetMemoryInfo")]
        public static extern ReturnCodes GetMemoryInfo(IntPtr handle, out MemoryInfo memInfo);

        [DllImport("nvidia-ml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlDeviceGetTemperature")]
        public static extern ReturnCodes GetTemperature(IntPtr handle, TemperatureSensors sensor, out uint tempValue);

        /// <summary>
        /// Buffer size guaranteed to be large enough for nvmlDeviceGetVbiosVersion
        /// </summary>
        public static readonly int VBiosVersionBufferSize = 32;

        [DllImport("nvidia-ml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlDeviceGetVbiosVersion")]
        public static extern ReturnCodes GetVBiosVersion(IntPtr handle, StringBuilder buffer, int length);

        // https://stackoverflow.com/a/370093/528131
        [DllImport("nvidia-ml.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nvmlErrorString")]
        public static extern IntPtr GetErrorString(ReturnCodes returnCode);
    }
}
