#include <Windows.h>
#include <cfgmgr32.h>
#include <string>
#include <iostream>

// Mostly re-written from https://github.com/nvpro-samples/nvml_enterprise_gpu_check/blob/main/loadNVML.cpp
// NVidia please don't sue me.
static wchar_t* VideoAdaptersClass = L"{4d36e968-e325-11ce-bfc1-08002be10318}";
static unsigned long FilterFlags = CM_GETIDLIST_FILTER_CLASS | CM_GETIDLIST_FILTER_PRESENT;

boolean LoadNVMLIntoThePath() {
	unsigned long deviceListSize;
	if (CM_Get_Device_ID_List_Size(&deviceListSize, VideoAdaptersClass, FilterFlags) != CR_SUCCESS) {
		return false;
	}

	wchar_t* deviceNames = new wchar_t[deviceListSize];
	if (CM_Get_Device_ID_List(VideoAdaptersClass, deviceNames, deviceListSize, FilterFlags) != CR_SUCCESS) {
		delete[] deviceNames;
		return false;
	}

	for (wchar_t* deviceName = deviceNames; *deviceName; deviceName += wcslen(deviceName) + 1) {
		unsigned long deviceId = 0;
		if (CM_Locate_DevNode(&deviceId, deviceName, CM_LOCATE_DEVNODE_NORMAL) != CR_SUCCESS) {
			continue;
		}

		HKEY deviceRegKey = 0;
		if (CM_Open_DevInst_Key(deviceId, KEY_QUERY_VALUE, 0, RegDisposition_OpenExisting, &deviceRegKey, CM_REGISTRY_SOFTWARE) != CR_SUCCESS) {
			continue;
		}

		unsigned long deviceKeyValueSize;
		if (RegQueryValueEx(deviceRegKey, L"OpenGLDriverName", NULL, NULL, NULL, &deviceKeyValueSize) != ERROR_SUCCESS) {
			RegCloseKey(deviceRegKey);
			continue;
		}

		wchar_t* deviceKeyValue = new wchar_t[deviceKeyValueSize];
		if (RegQueryValueEx(deviceRegKey, L"OpenGLDriverName", NULL, NULL, reinterpret_cast<LPBYTE>(deviceKeyValue), &deviceKeyValueSize) != ERROR_SUCCESS) {
			delete[] deviceKeyValue;
			RegCloseKey(deviceRegKey);
			continue;
		}

		wchar_t* lastBackslashPosition = wcsrchr(deviceKeyValue, '\\');
		bool deviceKeyValueIsPath = lastBackslashPosition != nullptr;

		if (deviceKeyValueIsPath) {
			std::wstring nvidiaDriverLocation = std::wstring(deviceKeyValue, lastBackslashPosition);
			int returnCode = SetDllDirectory(nvidiaDriverLocation.c_str());
			if (returnCode == 0) {
				delete[] deviceKeyValue;
				RegCloseKey(deviceRegKey);
				continue;
			}

			delete[] deviceKeyValue;
			RegCloseKey(deviceRegKey);
			return true;
		}

		delete[] deviceKeyValue;
		RegCloseKey(deviceRegKey);
	}

	SetDllDirectory(NULL);
	return false;
}

int main() {
	if (LoadNVMLIntoThePath()) {
		std::cout << "NVML was found";
	}
	else {
		std::cout << "NVML was not found";
	}

	return 0;
}