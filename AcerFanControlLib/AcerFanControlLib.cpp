#include "AcerFanControlLib.h"

#define LPCFILTER_IOPORT_READ_FUNC 0x800
#define LPCFILTER_IOPORT_WRITE_FUNC 0x801

void read_uchar(const HANDLE hDevice, const unsigned char port, unsigned char* const value)
{
	const DWORD nInBufferSize = 8;
	const DWORD nOutBufferSize = 1;
	const DWORD dwIoControlCode = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x800, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
	assert(dwIoControlCode == 0x22E000);

	BYTE lpOutBuffer[nOutBufferSize];
	BYTE lpInBuffer[nInBufferSize] = {};
	DWORD bytesReturned;

	if (!hDevice)
		throw InvalidDevice;

	lpInBuffer[0] = port;
	BOOL status = DeviceIoControl(hDevice, dwIoControlCode, lpInBuffer, nInBufferSize, lpOutBuffer, nOutBufferSize, &bytesReturned, nullptr);
	if (!status || bytesReturned != 1)
		throw ReadError;

	*value = lpOutBuffer[0];
}

void write_uchar(const HANDLE hDevice, const unsigned char port, const unsigned char value)
{
	const DWORD dwIoControlCode = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x801, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
	const DWORD nInBufferSize = 8;

	BYTE lpInBuffer[nInBufferSize] = { port, 0x00, 0x00, 0x00, value, 0x00, 0x00, 0x00 };
	DWORD bytesReturned;

	if (!hDevice)
		throw InvalidDevice;

	BOOL status = DeviceIoControl(hDevice, dwIoControlCode, lpInBuffer, nInBufferSize, nullptr, 0, &bytesReturned, nullptr);
	if (!status)
		throw WriteError;
}

void wait_until_bitmask_is_value(const HANDLE hDevice, const unsigned int bitmask, const unsigned char value)
{
	BYTE currentValue;
	for (WORD i = 0; i < 20000; ++i)
	{
		read_uchar(hDevice, 0x6C, &currentValue);
		if ((currentValue & bitmask) == value)
			return;
	}
	throw Timeout;
}

void ec_intro_sequence(HANDLE hDevice)
{
	unsigned char value;
	read_uchar(hDevice, 0x68, &value);
	wait_until_bitmask_is_value(hDevice, 0x02, 0x00);
	write_uchar(hDevice, 0x6C, 0x59);
}

void ec_close_sequence(HANDLE hDevice)
{
	unsigned char value;
	read_uchar(hDevice, 0x68, &value);
	wait_until_bitmask_is_value(hDevice, 0x02, 0x00);
	write_uchar(hDevice, 0x6C, 0xFF);
}

void setFan(UCHAR value)
{
	HANDLE lpcDriverHandle = CreateFileA("\\\\.\\LPCFilter", GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_READ | FILE_SHARE_WRITE, nullptr, OPEN_EXISTING, 0, nullptr);

	if (lpcDriverHandle == INVALID_HANDLE_VALUE)
		throw InvalidDevice;

	try
	{
		wait_until_bitmask_is_value(lpcDriverHandle, 0x80, 0x0);
		ec_intro_sequence(lpcDriverHandle);
		wait_until_bitmask_is_value(lpcDriverHandle, 0x02, 0x00);
		write_uchar(lpcDriverHandle, 0x68, value);
	}
	catch (FanControlError err)
	{
		ec_close_sequence(lpcDriverHandle);
		if (!CloseHandle(lpcDriverHandle))
			throw CloseError | err;
		throw err;
	}

	ec_close_sequence(lpcDriverHandle);
	if (!CloseHandle(lpcDriverHandle))
		throw CloseError;
}

extern "C" __declspec(dllexport) FanControlError WINAPI on()
{
	try	{ setFan(0x77); }
	catch (FanControlError err)	{ return err; }
	return None;
}

extern "C" __declspec(dllexport) FanControlError WINAPI off()
{
	try	{ setFan(0x76); }
	catch (FanControlError err)	{ return err; }
	return None;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD reason, LPVOID lpReserved) { return TRUE; }