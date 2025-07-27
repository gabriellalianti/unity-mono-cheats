#include <windows.h>
#include <string>
#include <thread>

#define DLL_NAME     "CheatAssembly.REPO.dll"
#define NAMESPACE    "CheatAssembly.REPO"
#define CLASS_NAME   "Entry"
#define METHOD_NAME  "Loader"

// Typedefs for Mono types
typedef void* MonoDomain;
typedef void* MonoAssembly;
typedef void* MonoImage;
typedef void* MonoClass;
typedef void* MonoMethod;
typedef void* MonoObject;
typedef void* MonoThread;

// Typedefs for Mono functions
typedef MonoThread* (__cdecl* t_mono_thread_attach)(MonoDomain*);
typedef MonoDomain* (__cdecl* t_mono_get_root_domain)();
typedef MonoDomain* (__cdecl* t_mono_domain_get)();
typedef void(__cdecl* t_mono_security_set_mode)(DWORD);
typedef MonoAssembly* (__cdecl* t_mono_domain_assembly_open)(MonoDomain*, const char*);
typedef MonoImage* (__cdecl* t_mono_assembly_get_image)(MonoAssembly*);
typedef MonoClass* (__cdecl* t_mono_class_from_name)(MonoImage*, const char*, const char*);
typedef MonoMethod* (__cdecl* t_mono_class_get_method_from_name)(MonoClass*, const char*, int);
typedef MonoObject* (__cdecl* t_mono_runtime_invoke)(MonoMethod*, void*, void**, MonoObject**);

// Mono function pointers
t_mono_thread_attach mono_thread_attach;
t_mono_get_root_domain mono_get_root_domain;
t_mono_domain_get mono_domain_get;
t_mono_security_set_mode mono_security_set_mode;
t_mono_domain_assembly_open mono_domain_assembly_open;
t_mono_assembly_get_image mono_assembly_get_image;
t_mono_class_from_name mono_class_from_name;
t_mono_class_get_method_from_name mono_class_get_method_from_name;
t_mono_runtime_invoke mono_runtime_invoke;


void RunInjection()
{
	// Get the Mono module handle
    HMODULE mono = GetModuleHandleW(L"mono.dll");
    if (!mono) mono = GetModuleHandleW(L"mono-2.0-bdwgc.dll");
    if (!mono) return;

	// Get addresses of Mono functions
    mono_thread_attach = (t_mono_thread_attach)GetProcAddress(mono, "mono_thread_attach");
    mono_get_root_domain = (t_mono_get_root_domain)GetProcAddress(mono, "mono_get_root_domain");
    mono_domain_get = (t_mono_domain_get)GetProcAddress(mono, "mono_domain_get");
    mono_security_set_mode = (t_mono_security_set_mode)GetProcAddress(mono, "mono_security_set_mode");
    mono_domain_assembly_open = (t_mono_domain_assembly_open)GetProcAddress(mono, "mono_domain_assembly_open");
    mono_assembly_get_image = (t_mono_assembly_get_image)GetProcAddress(mono, "mono_assembly_get_image");
    mono_class_from_name = (t_mono_class_from_name)GetProcAddress(mono, "mono_class_from_name");
    mono_class_get_method_from_name = (t_mono_class_get_method_from_name)GetProcAddress(mono, "mono_class_get_method_from_name");
    mono_runtime_invoke = (t_mono_runtime_invoke)GetProcAddress(mono, "mono_runtime_invoke");

	// Attach the current thread to the Mono runtime
    MonoDomain* rootDomain = mono_get_root_domain();
    mono_thread_attach(rootDomain);

	mono_security_set_mode(0); // Disable Mono code access security

    // Construct full path to the cheat assembly DLL
    char selfPath[MAX_PATH];
    GetModuleFileNameA(GetModuleHandleA("UnityMonoCheats.MonoLoader.dll"), selfPath, MAX_PATH);
    std::string path(selfPath);
    std::string dir = path.substr(0, path.find_last_of("\\/") + 1);
    std::string fullPath = dir + DLL_NAME;

	// Load and call the loader method of our cheat assembly
    MonoDomain* domain = mono_domain_get(); // Get current Mono domain
    MonoAssembly* assembly = mono_domain_assembly_open(domain, fullPath.c_str());
    if (!assembly) return;

    MonoImage* image = mono_assembly_get_image(assembly);
    if (!image) return;

    MonoClass* klass = mono_class_from_name(image, NAMESPACE, CLASS_NAME);
    if (!klass) return;

    MonoMethod* method = mono_class_get_method_from_name(klass, METHOD_NAME, 0);
    if (!method) return;

    mono_runtime_invoke(method, nullptr, nullptr, nullptr);
}

// DLL entry point
BOOL APIENTRY DllMain(HMODULE hModule, DWORD reason, LPVOID)
{
    if (reason == DLL_PROCESS_ATTACH)
    {
        DisableThreadLibraryCalls(hModule); // Disable notif when new threads are created/destroyed in the process
		std::thread(RunInjection).detach(); // Run injection in a separate thread
    }
    return TRUE;
}
