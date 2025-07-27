#include <windows.h>
#include <tlhelp32.h>
#include <string>
#include <iostream>

static DWORD FindPID(const std::wstring& exeName)
{
    PROCESSENTRY32W pe{ sizeof(pe) };
    HANDLE snap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (snap == INVALID_HANDLE_VALUE) return 0;

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
    HANDLE hProc = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
    if (!hProc) return false;

    SIZE_T bytes = (dllPath.size() + 1) * sizeof(wchar_t);
    LPVOID remoteBuf = VirtualAllocEx(hProc, nullptr, bytes, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
    if (!remoteBuf) { CloseHandle(hProc); return false; }

    if (!WriteProcessMemory(hProc, remoteBuf, dllPath.c_str(), bytes, nullptr)) {
        VirtualFreeEx(hProc, remoteBuf, 0, MEM_RELEASE);
        CloseHandle(hProc);
        return false;
    }

    FARPROC loadLib = GetProcAddress(GetModuleHandleW(L"kernel32.dll"), "LoadLibraryW");
    HANDLE hThread = CreateRemoteThread(hProc, nullptr, 0,
        (LPTHREAD_START_ROUTINE)loadLib, remoteBuf, 0, nullptr);

    if (!hThread) {
        VirtualFreeEx(hProc, remoteBuf, 0, MEM_RELEASE);
        CloseHandle(hProc);
        return false;
    }

    WaitForSingleObject(hThread, INFINITE);
    DWORD code = 0; GetExitCodeThread(hThread, &code);

    CloseHandle(hThread);
    VirtualFreeEx(hProc, remoteBuf, 0, MEM_RELEASE);
    CloseHandle(hProc);

    return code != 0;
}

int wmain(int argc, wchar_t* argv[])
{
    if (argc < 3) {
        std::wcout << L"Usage:\n  Injector.exe <Game.exe> <Full\\Path\\MonoLoader.dll>\n";
        return 0;
    }

    std::wstring exeName = argv[1];
    std::wstring dllPath = argv[2];

    DWORD pid = FindPID(exeName);
    if (!pid) { std::wcout << L"Process not found\n"; return 1; }

    if (!InjectDLL(pid, dllPath)) {
        std::wcout << L"Injection failed\n"; return 2;
    }

    std::wcout << L"Injected!\n";
    return 0;
}
