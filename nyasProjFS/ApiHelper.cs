using System.Runtime.InteropServices;

namespace nyasProjFS;

public static class ApiHelper
{
    /// <summary>the path to the ProjFS API library file in current windows</summary>
    public static string DLLPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ProjectedFSLib.dll");

    /// <summary>the ProjFS api is initialized or not</summary>
    public static bool IsInitialized { get; private set; } = false;
    /// <summary>the windows build number of the ProjFS Api, the value can be `Beta`, `Release`, `Additional` in `BuildNumbers`, or 0 when not initialized</summary>
    public static int ApiLevel { get; private set; } = 0;

    public static bool UseBetaApi => ApiLevel == BuildNumbers.Beta;
    public static bool HasAdditionalApi => ApiLevel >= BuildNumbers.Additional;

    /// <summary>initialize this ProjFS API</summary>
    public static void Initialize()
    {
        if (IsInitialized) { return; }
        nint dllPtr = NativeLibrary.Load(DLLPath);

        int apiLevel;
        if (NativeLibrary.GetExport(dllPtr, "PrjStartVirtualizing") != 0) {
            apiLevel = BuildNumbers.Release;

            if (NativeLibrary.GetExport(dllPtr, "PrjWritePlaceholderInfo2") != 0) { apiLevel = BuildNumbers.Additional; }
        }
        else if (NativeLibrary.GetExport(dllPtr, "PrjStartVirtualizationInstance") != 0) { apiLevel = BuildNumbers.Beta; }
        else {
            throw new EntryPointNotFoundException("could not set up the ProjFS API entry point");
        }

        NativeLibrary.Free(dllPtr);
        IsInitialized = true;
        ApiLevel = apiLevel;
    }
}