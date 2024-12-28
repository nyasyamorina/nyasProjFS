using System.Runtime.InteropServices;

namespace nyasProjFS;

public static partial class ApiHelper
{
    private static partial class Win32Native
    {
        [LibraryImport("kernel32.dll", EntryPoint = "LoadLibraryW", StringMarshalling = StringMarshalling.Utf16)]
        internal static partial nint LoadLibrary(string dllToLoad);

        [LibraryImport("kernel32.dll", EntryPoint = "GetProcAddress", StringMarshalling = StringMarshalling.Utf16)]
        internal static partial nint GetProcAddress(nint hModule, string procedureName);

        [LibraryImport("kernel32.dll", EntryPoint = "FreeLibrary")] [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool FreeLibrary(nint hModule);
    }


    public static bool IsInitialized { get; private set; } = false;

    /// <summary>initialize this ProjFS API</summary>
    public static void Initialize()
    {
        if (IsInitialized) { return; }
    }
}