using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

using UnityTcp.Editor.Helpers;
namespace UnityTcp.Editor.Native
{
    [InitializeOnLoad]
    internal static class NativeDllLoader
    {
        
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX

        // ---- Platform P/Invoke ----------------------------------------- //

#if UNITY_EDITOR_WIN
        private const uint GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT = 0x00000002;

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibraryW(string lpLibFileName);

        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetModuleHandleExW(uint dwFlags, string lpModuleName, out IntPtr phModule);

        private static string Win32Error() =>
            new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message;

        private static IntPtr PlatformLoad(string path)
        {
            IntPtr h = LoadLibraryW(path);
            if (h == IntPtr.Zero)
                CodelyLogger.LogWarning($"NativeDllLoader: LoadLibraryW failed: {Win32Error()}");
            return h;
        }
        private static void   PlatformFree(IntPtr handle)                   { if (handle != IntPtr.Zero) FreeLibrary(handle); }
        private static IntPtr PlatformGetProc(IntPtr handle, string symbol)
        {
            IntPtr ptr = GetProcAddress(handle, symbol);
            if (ptr == IntPtr.Zero)
                CodelyLogger.LogWarning($"NativeDllLoader: GetProcAddress('{symbol}') failed: {Win32Error()}");
            return ptr;
        }
        // Path-based probe: returns true iff the module at `path` is still loaded with the
        // given `expected` handle. UNCHANGED_REFCOUNT avoids bumping the refcount, so this
        // is purely a query — never call FreeLibrary on the result.
        private static bool PlatformIsHandleStillLoaded(string path, IntPtr expected)
        {
            if (expected == IntPtr.Zero) return false;
            return GetModuleHandleExW(
                       GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
                       path,
                       out IntPtr current)
                   && current == expected;
        }

#elif UNITY_EDITOR_OSX
        private const int RTLD_NOW    = 0x2;
        private const int RTLD_NOLOAD = 0x10; // dyld value (differs from glibc)

        [DllImport("libdl")]
        private static extern IntPtr dlopen(string filename, int flags);

        [DllImport("libdl")]
        private static extern int dlclose(IntPtr handle);

        [DllImport("libdl")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("libdl")]
        private static extern IntPtr dlerror();

        private static string DlError() { IntPtr p = dlerror(); return p != IntPtr.Zero ? Marshal.PtrToStringAnsi(p) : "unknown error"; }

        private static IntPtr PlatformLoad(string path)
        {
            dlerror();
            IntPtr h = dlopen(path, RTLD_NOW);
            if (h == IntPtr.Zero)
                CodelyLogger.LogWarning($"NativeDllLoader: dlopen failed: {DlError()}");
            return h;
        }
        private static void   PlatformFree(IntPtr handle)                   { if (handle != IntPtr.Zero) dlclose(handle); }
        private static IntPtr PlatformGetProc(IntPtr handle, string symbol)
        {
            dlerror();
            IntPtr ptr = dlsym(handle, symbol);
            if (ptr == IntPtr.Zero)
                CodelyLogger.LogWarning($"NativeDllLoader: dlsym('{symbol}') failed: {DlError()}");
            return ptr;
        }
        // Path-based probe via dlopen(RTLD_NOLOAD): returns the handle iff the image is
        // already mapped at `path`, NULL otherwise. NOLOAD still bumps the refcount on
        // success, so we balance with dlclose immediately. Never call PlatformFree on
        // anything other than handles we explicitly loaded.
        private static bool PlatformIsHandleStillLoaded(string path, IntPtr expected)
        {
            if (expected == IntPtr.Zero) return false;
            dlerror();
            IntPtr probe = dlopen(path, RTLD_NOW | RTLD_NOLOAD);
            if (probe == IntPtr.Zero) return false;
            bool matches = probe == expected;
            dlclose(probe);
            return matches;
        }

#else // UNITY_EDITOR_LINUX
        private const int RTLD_NOW    = 0x2;
        private const int RTLD_NOLOAD = 0x4; // glibc value (differs from dyld)

        [DllImport("libdl.so.2")]
        private static extern IntPtr dlopen(string filename, int flags);

        [DllImport("libdl.so.2")]
        private static extern int dlclose(IntPtr handle);

        [DllImport("libdl.so.2")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("libdl.so.2")]
        private static extern IntPtr dlerror();

        private static string DlError() { IntPtr p = dlerror(); return p != IntPtr.Zero ? Marshal.PtrToStringAnsi(p) : "unknown error"; }

        private static IntPtr PlatformLoad(string path)
        {
            dlerror();
            IntPtr h = dlopen(path, RTLD_NOW);
            if (h == IntPtr.Zero)
                CodelyLogger.LogWarning($"NativeDllLoader: dlopen failed: {DlError()}");
            return h;
        }
        private static void   PlatformFree(IntPtr handle)                   { if (handle != IntPtr.Zero) dlclose(handle); }
        private static IntPtr PlatformGetProc(IntPtr handle, string symbol)
        {
            dlerror();
            IntPtr ptr = dlsym(handle, symbol);
            if (ptr == IntPtr.Zero)
                CodelyLogger.LogWarning($"NativeDllLoader: dlsym('{symbol}') failed: {DlError()}");
            return ptr;
        }
        // Path-based probe via dlopen(RTLD_NOLOAD): returns the handle iff the image is
        // already mapped at `path`, NULL otherwise. NOLOAD still bumps the refcount on
        // success, so we balance with dlclose immediately. Never call PlatformFree on
        // anything other than handles we explicitly loaded.
        private static bool PlatformIsHandleStillLoaded(string path, IntPtr expected)
        {
            if (expected == IntPtr.Zero) return false;
            dlerror();
            IntPtr probe = dlopen(path, RTLD_NOW | RTLD_NOLOAD);
            if (probe == IntPtr.Zero) return false;
            bool matches = probe == expected;
            dlclose(probe);
            return matches;
        }
#endif

        // ---- Event handlers ---------------------------------------------- //

        static NativeDllLoader()
        {
#if UNITY_2020_1_OR_NEWER
            UnityEditor.PackageManager.Events.registeringPackages -= OnPackagesRegistering;
            UnityEditor.PackageManager.Events.registeringPackages += OnPackagesRegistering;
#endif
            EditorApplication.quitting -= OnEditorQuitting;
            EditorApplication.quitting += OnEditorQuitting;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }
        private static void OnEditorQuitting() => Unload("unity_quit");

        private static void OnBeforeAssemblyReload()
        {
            UnregisterLogCallback();
        }

#if UNITY_2020_1_OR_NEWER
        private static void OnPackagesRegistering(UnityEditor.PackageManager.PackageRegistrationEventArgs args)
        {
            if (!IsLoaded) return;

            string ourName;
            try { ourName = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(NativeDllLoader).Assembly)?.name; }
            catch { return; }

            if (string.IsNullOrEmpty(ourName)) return;

            // Unload when our package is updated (changedTo) OR removed (removed).
            // Removal only ever appears in args.removed, never in args.changedTo, so
            // without this the native socket stays open after an uninstall and clients
            // never see a disconnect.
            foreach (var package in args.changedTo)
            {
                if (package.name != ourName) continue;
                Unload("package_updating");
                return;
            }

            foreach (var package in args.removed)
            {
                if (package.name != ourName) continue;
                Unload("package_removed");
                return;
            }
        }
#endif

        // ---- State ----------------------------------------------------- //

        private const string HandleKey = "NativeDllLoader._handle";

        private static IntPtr _handle = IntPtr.Zero;

        internal static bool IsLoaded => _handle != IntPtr.Zero;

        /// <summary>
        /// Fired just before the native library is unloaded.
        /// Subscribers (e.g. UnityTcpBridge) should call their own Stop() here
        /// so higher-level teardown happens before the handle is freed.
        /// The argument is why the unload is happening (e.g. "unity_quit",
        /// "package_updating"), destined for the heartbeat file's "reason"
        /// field; null means no specific reason (generic "stopped").
        /// </summary>
        internal static event Action<string> OnUnload;

        // ---- Log callback ---------------------------------------------- //

        // Native setter — resolved from "Log_SetCallback" and held here so callers can reference NativeDllLoader.SetLogCallback directly.
        internal delegate void SetLogCallbackDelegate(IntPtr callback);
        internal static SetLogCallbackDelegate SetLogCallback;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NativeLogCallbackDelegate(int level, [MarshalAs(UnmanagedType.LPStr)] string message);

        // Kept as a field to prevent GC collection while native holds the pointer.
        private static NativeLogCallbackDelegate _logCallback;

        private static void RegisterLogCallback()
        {
            if (!IsLoaded) return;
            try
            {
                _logCallback = (level, message) =>
                {
                    switch (level)
                    {
                        case 0:  CodelyLogger.Log($"[NativeTcpBridge] {message}"); break;
                        case 1:  CodelyLogger.LogWarning($"[NativeTcpBridge] {message}"); break;
                        default: CodelyLogger.LogError($"[NativeTcpBridge] {message}"); break;
                    }
                };
                IntPtr ptr = Marshal.GetFunctionPointerForDelegate(_logCallback);
                SetLogCallback?.Invoke(ptr);
            }
            catch (Exception ex)
            {
                CodelyLogger.LogWarning($"NativeDllLoader: RegisterLogCallback failed: {ex.Message}");
            }
        }

        private static void UnregisterLogCallback()
        {
            if (!IsLoaded) return;
            try { SetLogCallback?.Invoke(IntPtr.Zero); } catch { }
            _logCallback = null;
        }

        // ---- Public API ------------------------------------------------ //

        internal static bool Load()
        {
            if (_handle != IntPtr.Zero) return true;

            string dllPath = FindDllPath();
            if (string.IsNullOrEmpty(dllPath)) return false;

            // On domain reload the DLL stays loaded in the process. Recover the handle
            // from SessionState so we can re-resolve delegates without reloading the binary.
            // Validate via a path-based probe first — a package update or other unload may
            // have invalidated the handle, and calling FreeLibrary / dlclose on a freed
            // handle is undefined behavior.
            string stored = SessionState.GetString(HandleKey, "0");
            if (long.TryParse(stored, out long handleValue) && handleValue != 0)
            {
                var storedHandle = new IntPtr(handleValue);
                if (PlatformIsHandleStillLoaded(dllPath, storedHandle))
                {
                    _handle = storedHandle;
                    if (ResolveAll())
                    {
                        CodelyLogger.Log("NativeDllLoader: resolved all symbols with stored handle");
                        RegisterLogCallback();
                        return true;
                    }

                    // Handle is valid but symbols are missing (rare ABI mismatch). The handle
                    // really refers to our module, so freeing it here is safe.
                    PlatformFree(_handle);
                    _handle = IntPtr.Zero;
                }
                else
                {
                    CodelyLogger.Log("NativeDllLoader: stored native handle is no longer loaded; reloading from disk");
                }
                SessionState.EraseString(HandleKey);
            }

            bool isLoaded = LoadFrom(dllPath);
            if(isLoaded) {
                RegisterLogCallback();
                return true;
            }
            return false;
        }

        internal static bool Reload()
        {
            Unload();
            return Load();
        }

        internal static void Unload(string reason = null)
        {
            if (_handle == IntPtr.Zero) return;
            OnUnload?.Invoke(reason);
            UnregisterLogCallback();
            PlatformFree(_handle);
            _handle = IntPtr.Zero;
            NativeUnityTcpBridgeAPI.NTB_GetAbiVersion                = null;
            NativeUnityTcpBridgeAPI.NTB_StartBridge                  = null;
            NativeUnityTcpBridgeAPI.NTB_Stop                         = null;
            NativeUnityTcpBridgeAPI.NTB_StopWithReason               = null;
            NativeUnityTcpBridgeAPI.NTB_IsRunning                    = null;
            NativeUnityTcpBridgeAPI.NTB_GetBoundPort                 = null;
            NativeUnityTcpBridgeAPI.NTB_GetConnectionCount           = null;
            NativeUnityTcpBridgeAPI.NTB_TryDequeueCommand            = null;
            NativeUnityTcpBridgeAPI.NTB_EnqueueResponse              = null;
            NativeUnityTcpBridgeAPI.NTB_GetClientsJson               = null;
            NativeUnityTcpBridgeAPI.NTB_SetIsCSharpAssemblyReloading = null;
            NativeUnityTcpBridgeAPI.NTB_NotifyAll                    = null;
            NativeUnityTcpBridgeAPI.NTB_SetStreamPort                = null;
            SetLogCallback                                          = null;
            SessionState.EraseString(HandleKey);
        }

        // ---- Private helpers ------------------------------------------- //

        private static bool LoadFrom(string dllPath)
        {
            _handle = PlatformLoad(dllPath);
            if (_handle == IntPtr.Zero)
            {
                CodelyLogger.LogWarning($"NativeDllLoader: failed to load '{dllPath}'.");
                return false;
            }
            if (!ResolveAll())
            {
                PlatformFree(_handle);
                _handle = IntPtr.Zero;
                return false;
            }
            SessionState.SetString(HandleKey, _handle.ToInt64().ToString());
            CodelyLogger.Log("NativeDllLoader: loaded DLL and resolved all symbols");
            return true;
        }

        // The ABI handshake must come first: it is the only symbol whose
        // signature is guaranteed to match across versions, so it has to be
        // checked before any other symbol is *called*. (Resolving the rest is
        // harmless on its own — GetDelegateForFunctionPointer does not call
        // into native — but a mismatched call would read arguments that were
        // never passed.) Failing here is the designed outcome on a version
        // skew, not an error path worth recovering from.
        private static bool ResolveAll()
        {
            if (!CheckAbiVersion()) return false;

            return
                Resolve("NTB_StartBridge",                  ref NativeUnityTcpBridgeAPI.NTB_StartBridge)                  &&
                Resolve("NTB_StopWithReason",               ref NativeUnityTcpBridgeAPI.NTB_StopWithReason)               &&
                Resolve("NTB_Stop",                         ref NativeUnityTcpBridgeAPI.NTB_Stop)                         &&
                Resolve("NTB_IsRunning",                    ref NativeUnityTcpBridgeAPI.NTB_IsRunning)                    &&
                Resolve("NTB_GetBoundPort",                 ref NativeUnityTcpBridgeAPI.NTB_GetBoundPort)                 &&
                Resolve("NTB_GetConnectionCount",           ref NativeUnityTcpBridgeAPI.NTB_GetConnectionCount)           &&
                Resolve("NTB_TryDequeueCommand",            ref NativeUnityTcpBridgeAPI.NTB_TryDequeueCommand)            &&
                Resolve("NTB_EnqueueResponse",              ref NativeUnityTcpBridgeAPI.NTB_EnqueueResponse)              &&
                Resolve("NTB_GetClientsJson",               ref NativeUnityTcpBridgeAPI.NTB_GetClientsJson)               &&
                Resolve("NTB_SetIsCSharpAssemblyReloading", ref NativeUnityTcpBridgeAPI.NTB_SetIsCSharpAssemblyReloading) &&
                Resolve("NTB_NotifyAll",                    ref NativeUnityTcpBridgeAPI.NTB_NotifyAll)                    &&
                Resolve("NTB_SetStreamPort",                 ref NativeUnityTcpBridgeAPI.NTB_SetStreamPort)                &&
                Resolve("Log_SetCallback",                  ref SetLogCallback);
        }

        private static bool CheckAbiVersion()
        {
            if (!Resolve("NTB_GetAbiVersion", ref NativeUnityTcpBridgeAPI.NTB_GetAbiVersion))
            {
                CodelyLogger.LogWarning(
                    "NativeDllLoader: native plugin predates ABI versioning (no NTB_GetAbiVersion); refusing to bind.");
                return false;
            }

            int nativeAbi;
            try
            {
                nativeAbi = NativeUnityTcpBridgeAPI.NTB_GetAbiVersion();
            }
            catch (Exception ex)
            {
                CodelyLogger.LogWarning($"NativeDllLoader: ABI version probe failed: {ex.Message}");
                return false;
            }

            if (nativeAbi != NativeUnityTcpBridgeAPI.ExpectedAbiVersion)
            {
                CodelyLogger.LogWarning(
                    $"NativeDllLoader: native ABI {nativeAbi} does not match the expected {NativeUnityTcpBridgeAPI.ExpectedAbiVersion}; " +
                    "refusing to bind. If this appears during startup right after a package update, it is expected and harmless: " +
                    "Unity is running a previously compiled assembly against the updated plugin, and the bridge will start normally " +
                    "once script compilation finishes and the domain reloads.");
                return false;
            }

            return true;
        }

        private static bool Resolve<T>(string symbol, ref T del) where T : Delegate
        {
            IntPtr ptr = PlatformGetProc(_handle, symbol);
            if (ptr == IntPtr.Zero) return false;
            del = Marshal.GetDelegateForFunctionPointer<T>(ptr);
            return true;
        }

        private static string FindDllPath()
        {
            try
            {
                var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(NativeDllLoader).Assembly);
                if (packageInfo == null)
                {
                    CodelyLogger.LogWarning("NativeDllLoader: could not locate package via PackageInfo.");
                    return null;
                }

#if UNITY_EDITOR_WIN
                string relative = Path.Combine("Plugins", "Win", "NativeTcpBridge.dll");
#elif UNITY_EDITOR_OSX
                string relative = Path.Combine("Plugins", "macOS", "libNativeTcpBridge.dylib");
#else
                string relative = Path.Combine("Plugins", "Linux", "x86_64", "libNativeTcpBridge.so");
#endif
                string fullPath = Path.GetFullPath(Path.Combine(packageInfo.resolvedPath, relative));
                if (File.Exists(fullPath)) return fullPath;

                CodelyLogger.LogWarning($"NativeDllLoader: DLL not found at '{fullPath}'.");
            }
            catch (Exception ex)
            {
                CodelyLogger.LogWarning($"NativeDllLoader: path lookup failed. {ex.Message}");
            }
            return null;
        }

#else
        // ---- Unsupported platform stubs -------------------------------- //
        internal static bool IsLoaded  => false;
        internal static bool Load()    => false;
        internal static bool Reload()  => false;
        internal static void Unload(string reason = null)  { }
#endif
    }
}
