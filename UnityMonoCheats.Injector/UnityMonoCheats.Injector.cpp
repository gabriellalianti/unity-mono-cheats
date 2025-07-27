#include <iostream>
#include <windows.h>
#include <tlhelp32.h>
#include <string>

static DWORD FindPID(const std::wstring& exeName)
{
	// Setup struct to hold process information
    PROCESSENTRY32W pe{ sizeof(pe) };

    // Take a snapshot of all running processes
    HANDLE snap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (snap == INVALID_HANDLE_VALUE) return 0;

    // Look for our target process in the process list
    for (BOOL ok = Process32FirstW(snap, &pe); ok; ok = Process32NextW(snap, &pe))
        if (_wcsicmp(pe.szExeFile, exeName.c_str()) == 0) {
            CloseHandle(snap);
            return pe.th32ProcessID;
        }

    CloseHandle(snap);
    return 0;
}

static bool InjectDLL(DWORD pid, const std::wstring& dllPath)
{
	// Open a handle to the target process
    HANDLE hProc = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
    if (!hProc) return false;

	// Allocate memory in the target process for the DLL path
    SIZE_T bytes = (dllPath.size() + 1) * sizeof(wchar_t);
    LPVOID remoteBuf = VirtualAllocEx(hProc, nullptr, bytes, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
    if (!remoteBuf) { CloseHandle(hProc); return false; }

    // Write the path of the DLL to the allocated memory
    if (!WriteProcessMemory(hProc, remoteBuf, dllPath.c_str(), bytes, nullptr)) {
        VirtualFreeEx(hProc, remoteBuf, 0, MEM_RELEASE);
        CloseHandle(hProc);
        return false;
    }

    // Get the address of LoadLibraryW from kernel32.dll
    FARPROC loadLib = GetProcAddress(GetModuleHandleW(L"kernel32.dll"), "LoadLibraryW");

	// Create a remote thread in the target process to call LoadLibraryW
    HANDLE hThread = CreateRemoteThread(hProc, nullptr, 0,
        (LPTHREAD_START_ROUTINE)loadLib, remoteBuf, 0, nullptr);

    if (!hThread) {
        VirtualFreeEx(hProc, remoteBuf, 0, MEM_RELEASE);
        CloseHandle(hProc);
        return false;
    }

    // Wait for the remote thread to finish and get exit code
    WaitForSingleObject(hThread, INFINITE);
    DWORD code = 0;
    GetExitCodeThread(hThread, &code);

    CloseHandle(hThread);
    VirtualFreeEx(hProc, remoteBuf, 0, MEM_RELEASE);
    CloseHandle(hProc);

    return code != 0;
}

int wmain(int argc, wchar_t* argv[])
{
    if (argc < 2) {
        std::wcout << L"Usage:\n  UnityMonoCheats.Injector.exe <Full\\Path\\MonoLoader.dll>\n";
        return 0;
    }

    std::wstring exeName = L"REPO.exe";
    std::wstring dllPath = argv[1];

    DWORD pid = FindPID(exeName);
    if (!pid) {
        std::wcout << L"Process not found\n";
        return 1;
    }

    if (!InjectDLL(pid, dllPath)) {
        std::wcout << L"Injection failed\n";
        return 2;
    }

    std::wcout << L"Injection succeeded\n";
    return 0;
}
