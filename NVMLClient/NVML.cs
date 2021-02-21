using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using NvidiaMLTest.PInvoke;

namespace NvidiaMLTest
{
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
    public static class NVML
    {
        static NVML()
        {
            ThrowIfReturnIsError(NVMLFunctions.Initialize());
        }

        [DebuggerStepThrough]
        [DebuggerNonUserCode]
        private static void ThrowIfReturnIsError(NVMLFunctions.ReturnCodes returnCode)
        {
            if (returnCode == NVMLFunctions.ReturnCodes.Success)
            {
                return;
            }

            // https://stackoverflow.com/a/370093/528131
            var errorMessagePtr = NVMLFunctions.GetErrorString(returnCode);
            var errorMessage = Marshal.PtrToStringAuto(errorMessagePtr);
            throw new Exception(string.Format("NVML Failure: {0} - {1}", returnCode, errorMessage));
        }

        public static int GetDeviceCount()
        {
            var deviceCount = 0;
            ThrowIfReturnIsError(NVMLFunctions.GetDeviceCount(ref deviceCount));
            return deviceCount;
        }

        public static IntPtr GetDeviceHandleByIndex(int deviceIndex)
        {
            ThrowIfReturnIsError(NVMLFunctions.GetDeviceHandleByIndex(deviceIndex, out var deviceHandle));
            return deviceHandle;
        }

        public static string GetDeviceName(IntPtr deviceHandle)
        {
            var stringBuffer = new StringBuilder(NVMLFunctions.DeviceNameBufferSize);
            ThrowIfReturnIsError(NVMLFunctions.GetDeviceName(deviceHandle, stringBuffer, stringBuffer.Capacity));
            return stringBuffer.ToString();
        }

        public static NVMLFunctions.Architectures GetArchitecture(IntPtr deviceHandle)
        {
            ThrowIfReturnIsError(NVMLFunctions.GetArchitecture(deviceHandle, out var architecture));
            return architecture;
        }

        public static (ulong Free, ulong Total, ulong Used) GetMemoryInfo(IntPtr deviceHandle)
        {
            ThrowIfReturnIsError(NVMLFunctions.GetMemoryInfo(deviceHandle, out var memoryInfo));
            return (memoryInfo.Free, memoryInfo.Total, memoryInfo.Used);
        }

        public static uint GetTemperature(IntPtr deviceHandle)
        {
            ThrowIfReturnIsError(NVMLFunctions.GetTemperature(deviceHandle, NVMLFunctions.TemperatureSensors.GPUSensor, out var temperature));
            return temperature;
        }

        public static string GetVBiosVersion(IntPtr deviceHandle)
        {
            var stringBuffer = new StringBuilder(NVMLFunctions.VBiosVersionBufferSize);
            ThrowIfReturnIsError(NVMLFunctions.GetVBiosVersion(deviceHandle, stringBuffer, stringBuffer.Capacity));
            return stringBuffer.ToString();
        }
    }
}
