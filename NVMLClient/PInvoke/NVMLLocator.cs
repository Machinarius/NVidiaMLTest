using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace NvidiaMLClient.PInvoke
{
    public static class NVMLLocator
    {
        private const string VideoAdaptersClass = "{4d36e968-e325-11ce-bfc1-08002be10318}";
        private const ulong DeviceFlagFilterClass = 0x00000200;
        private const ulong DeviceFlagPresent = 0x00000100;
        private const ulong DeviceFlags = DeviceFlagFilterClass | DeviceFlagPresent;
        private const ulong LocateDeviceFlagNormal = 0;
        private const ulong AccessMaskQueryValue = 1;
        private const ulong CurrentHardwareProfile = 0;
        private const ulong RegistryDispositionOpenExisting = 1;
        private const ulong RegistrySoftware = 1;

        public static void EnsureNVMLCanBeLocated()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Assume linux, nothing to do.
                // Linux is smart with LDConfig, Windows is monke with PATH look-ups
                return;
            }

            CheckReturn(Functions.GetDeviceIdListSize(out var deviceListSize, VideoAdaptersClass, DeviceFlags));
            
            var stringBuffer = new byte[deviceListSize];
            CheckReturn(Functions.GetDeviceIdList(VideoAdaptersClass, stringBuffer, deviceListSize, DeviceFlags));
            
            var nvmlLocation = "";
            foreach (var deviceId in EnumerateDeviceStrings(stringBuffer))
            {
                CheckReturn(Functions.LocateDeviceNode(out var deviceNode, deviceId, LocateDeviceFlagNormal));
                CheckReturn(Functions.OpenDeviceNodeRegistryKey(
                    deviceNode, AccessMaskQueryValue, CurrentHardwareProfile,
                    RegistryDispositionOpenExisting, out var registryKeyHandle, 
                    RegistrySoftware
                ));

                var registryKey = RegistryKey.FromHandle(new SafeRegistryHandle(registryKeyHandle, true));
                var registryValue = registryKey.GetValue("OpenGLDriverName") as string[];
                if (registryValue == null || registryValue.Length != 1)
                {
                    continue;
                }

                var nvidiaDriverFilePath = registryValue[0];
                var nvidiaDriverStore = Path.GetDirectoryName(nvidiaDriverFilePath);
                if (File.Exists(Path.Combine(nvidiaDriverStore, "nvml.dll"))) {
                    nvmlLocation = nvidiaDriverStore;
                    break;
                }
            }

            if (string.IsNullOrEmpty(nvmlLocation))
            {
                ThrowGenericError();
            }

            // TODO: Do I have to clean-up this handle?
            Functions.AddDLLDirectory(nvmlLocation);
        }

        private static IEnumerable<string> EnumerateDeviceStrings(byte[] buffer)
        {  
            var currentIndex = 0;
            while (currentIndex < buffer.Length)
            {
                var currentString = new StringBuilder();
                while (buffer[currentIndex] != 0)
                {
                    currentString.Append(char.ConvertFromUtf32(buffer[currentIndex++]));
                }

                if (currentString.Length == 0)
                {
                    break;
                }

                currentIndex++;
                yield return currentString.ToString();
            }
        }

        private static void ThrowGenericError()
        {
            throw new Exception("Could not locate NVML. Did you install the NVidia driver?");
        }

        private static void CheckReturn(int returnCode)
        {
            // CR_SUCCESS
            if (returnCode != 0)
            {
                ThrowGenericError();
            }
        }

        private static class Functions
        {
            [DllImport("CfgMgr32.dll", EntryPoint = "CM_Get_Device_ID_List_Size")]
            public static extern int GetDeviceIdListSize(out ulong size, string filter, ulong flags);
            [DllImport("CfgMgr32.dll", EntryPoint = "CM_Get_Device_ID_List")]
            public static extern int GetDeviceIdList(string filter, byte[] buffer, ulong devicesLength, ulong flags);
            [DllImport("CfgMgr32.dll", EntryPoint = "CM_Locate_DevNode")]
            public static extern int LocateDeviceNode(out ulong deviceNodeId, string deviceId, ulong flags);
            [DllImport("CfgMgr32.dll", EntryPoint = "CM_Open_DevNode_Key")]
            public static extern int OpenDeviceNodeRegistryKey(ulong deviceNodeId, ulong accessMask, ulong hwProfile, ulong disposition, out IntPtr registryKey, ulong flags);
            [DllImport("Kernel32.dll", EntryPoint = "AddDllDirectory")]
            public static extern IntPtr AddDLLDirectory(string directory);
        }
    }
}
