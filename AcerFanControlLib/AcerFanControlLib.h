#pragma once

#include <Windows.h>
#include <winioctl.h>
#include <assert.h>

enum FanControlError : int
{
	None = 0,
	InvalidDevice = 1 << 0,
	ReadError = 1 << 1,
	WriteError = 1 << 2,
	Timeout = 1 << 3,
	CloseError = 1 << 4
};

extern "C" __declspec(dllexport) FanControlError WINAPI on();
extern "C" __declspec(dllexport) FanControlError WINAPI off();