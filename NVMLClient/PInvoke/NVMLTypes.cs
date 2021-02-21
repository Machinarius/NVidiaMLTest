using System.Runtime.InteropServices;

namespace NvidiaMLClient.PInvoke
{
    public enum NVMLReturnCodes
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
}
