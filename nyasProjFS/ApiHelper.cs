using System.Runtime.InteropServices;
using nyasProjFS.ProjectedFSLib;
using static nyasProjFS.ProjectedFSLib.FunctionTypes;
using static nyasProjFS.ProjectedFSLib_Deprecated.FunctionTypes;

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

    // Release ProjFS APIs
    public static PrjStartVirtualizing?             PrjStartVirtualizing              { get; private set; }
    public static PrjStopVirtualizing?              PrjStopVirtualizing               { get; private set; }
    public static PrjWriteFileData?                 PrjWriteFileData                  { get; private set; }
    public static PrjWritePlaceholderInfo?          PrjWritePlaceholderInfo           { get; private set; }
    public static PrjAllocateAlignedBuffer?         PrjAllocateAlignedBuffer          { get; private set; }
    public static PrjFreeAlignedBuffer?             PrjFreeAlignedBuffer              { get; private set; }
    public static PrjGetVirtualizationInstanceInfo? PrjGetVirtualizationInstanceInfo  { get; private set; }
    public static PrjUpdateFileIfNeeded?            PrjUpdateFileIfNeeded             { get; private set; }
    public static PrjMarkDirectoryAsPlaceholder?    PrjMarkDirectoryAsPlaceholder     { get; private set; }
    private static bool TryLoadReleaseApi(nint dllPtr)
    {
        if (!NativeLibrary.TryGetExport(dllPtr, nameof(PrjStartVirtualizing), out nint funcPtr)) { return false; }
        PrjStartVirtualizing = Marshal.GetDelegateForFunctionPointer<PrjStartVirtualizing>(funcPtr);

        PrjStopVirtualizing              = GetProc<PrjStopVirtualizing             >(dllPtr, nameof(PrjStopVirtualizing             ));
        PrjWriteFileData                 = GetProc<PrjWriteFileData                >(dllPtr, nameof(PrjWriteFileData                ));
        PrjWritePlaceholderInfo          = GetProc<PrjWritePlaceholderInfo         >(dllPtr, nameof(PrjWritePlaceholderInfo         ));
        PrjAllocateAlignedBuffer         = GetProc<PrjAllocateAlignedBuffer        >(dllPtr, nameof(PrjAllocateAlignedBuffer        ));
        PrjFreeAlignedBuffer             = GetProc<PrjFreeAlignedBuffer            >(dllPtr, nameof(PrjFreeAlignedBuffer            ));
        PrjGetVirtualizationInstanceInfo = GetProc<PrjGetVirtualizationInstanceInfo>(dllPtr, nameof(PrjGetVirtualizationInstanceInfo));
        PrjUpdateFileIfNeeded            = GetProc<PrjUpdateFileIfNeeded           >(dllPtr, nameof(PrjUpdateFileIfNeeded           ));
        PrjMarkDirectoryAsPlaceholder    = GetProc<PrjMarkDirectoryAsPlaceholder   >(dllPtr, nameof(PrjMarkDirectoryAsPlaceholder   ));
        return true;
    }

    // Additional ProjFS APIs
    public static PrjWritePlaceholderInfo2? PrjWritePlaceholderInfo2 { get; private set; }
    public static PrjFillDirEntryBuffer2?   PrjFillDirEntryBuffer2   { get; private set; }
    private static bool TryLoadAdditionalApi(nint dllPtr)
    {
        if (!NativeLibrary.TryGetExport(dllPtr, nameof(PrjWritePlaceholderInfo2), out nint funcPtr)) { return false; }
        PrjWritePlaceholderInfo2 = Marshal.GetDelegateForFunctionPointer<PrjWritePlaceholderInfo2>(funcPtr);

        PrjFillDirEntryBuffer2 = GetProc<PrjFillDirEntryBuffer2>(dllPtr, nameof(PrjFillDirEntryBuffer2));
        return true;
    }

    // Beta ProjFS APIs
#pragma warning disable CS0618
    public static PrjStartVirtualizationInstance?           PrjStartVirtualizationInstance           { get; private set; }
    public static PrjStartVirtualizationInstanceEx?         PrjStartVirtualizationInstanceEx         { get; private set; }
    public static PrjStopVirtualizationInstance?            PrjStopVirtualizationInstance            { get; private set; }
    public static PrjGetVirtualizationInstanceIdFromHandle? PrjGetVirtualizationInstanceIdFromHandle { get; private set; }
    public static PrjConvertDirectoryToPlaceholder?         PrjConvertDirectoryToPlaceholder         { get; private set; }
    public static PrjWritePlaceholderInformation?           PrjWritePlaceholderInformation           { get; private set; }
    public static PrjUpdatePlaceholderIfNeeded?             PrjUpdatePlaceholderIfNeeded             { get; private set; }
    public static PrjWriteFile?                             PrjWriteFile                             { get; private set; }
    public static PrjCommandCallbacksInit?                  PrjCommandCallbacksInit                  { get; private set; }
    private static bool TryLoadBetaApi(nint dllPtr)
    {
        if (!NativeLibrary.TryGetExport(dllPtr, nameof(PrjStartVirtualizationInstance), out nint funcPtr)) { return false; }
        PrjStartVirtualizationInstance = Marshal.GetDelegateForFunctionPointer<PrjStartVirtualizationInstance>(funcPtr);

        PrjStartVirtualizationInstanceEx         = GetProc<PrjStartVirtualizationInstanceEx        >(dllPtr, nameof(PrjStartVirtualizationInstanceEx        ));
        PrjStopVirtualizationInstance            = GetProc<PrjStopVirtualizationInstance           >(dllPtr, nameof(PrjStopVirtualizationInstance           ));
        PrjGetVirtualizationInstanceIdFromHandle = GetProc<PrjGetVirtualizationInstanceIdFromHandle>(dllPtr, nameof(PrjGetVirtualizationInstanceIdFromHandle));
        PrjConvertDirectoryToPlaceholder         = GetProc<PrjConvertDirectoryToPlaceholder        >(dllPtr, nameof(PrjConvertDirectoryToPlaceholder        ));
        PrjWritePlaceholderInformation           = GetProc<PrjWritePlaceholderInformation          >(dllPtr, nameof(PrjWritePlaceholderInformation          ));
        PrjUpdatePlaceholderIfNeeded             = GetProc<PrjUpdatePlaceholderIfNeeded            >(dllPtr, nameof(PrjUpdatePlaceholderIfNeeded            ));
        PrjWriteFile                             = GetProc<PrjWriteFile                            >(dllPtr, nameof(PrjWriteFile                            ));
        PrjCommandCallbacksInit                  = GetProc<PrjCommandCallbacksInit                 >(dllPtr, nameof(PrjCommandCallbacksInit                 ));
        return true;
    }
#pragma warning restore CS0618

    // Common ProjFS APIs
    public static PrjClearNegativePathCache   PrjClearNegativePathCache   { get; private set; } = DummyPrjClearNegativePathCache;
    public static PrjDeleteFile               PrjDeleteFile               { get; private set; } = DummyPrjDeleteFile;
    public static PrjGetOnDiskFileState       PrjGetOnDiskFileState       { get; private set; } = DummyPrjGetOnDiskFileState;
    public static unsafe PrjCompleteCommand   PrjCompleteCommand          { get; private set; } = DummyPrjCompleteCommand;
    public static PrjFillDirEntryBuffer       PrjFillDirEntryBuffer       { get; private set; } = DummyPrjFillDirEntryBuffer;
    public static PrjFileNameMatch            PrjFileNameMatch            { get; private set; } = DummyPrjFileNameMatch;
    public static PrjFileNameCompare          PrjFileNameCompare          { get; private set; } = DummyPrjFileNameCompare;
    public static PrjDoesNameContainWildCards PrjDoesNameContainWildCards { get; private set; } = DummyPrjDoesNameContainWildCards;
    private static void LoadCommandApi(nint dllPtr)
    {
        PrjClearNegativePathCache   = GetProc<PrjClearNegativePathCache  >(dllPtr, nameof(PrjClearNegativePathCache  ));
        PrjDeleteFile               = GetProc<PrjDeleteFile              >(dllPtr, nameof(PrjDeleteFile              ));
        PrjGetOnDiskFileState       = GetProc<PrjGetOnDiskFileState      >(dllPtr, nameof(PrjGetOnDiskFileState      ));
        PrjCompleteCommand          = GetProc<PrjCompleteCommand         >(dllPtr, nameof(PrjCompleteCommand         ));
        PrjFillDirEntryBuffer       = GetProc<PrjFillDirEntryBuffer      >(dllPtr, nameof(PrjFillDirEntryBuffer      ));
        PrjFileNameMatch            = GetProc<PrjFileNameMatch           >(dllPtr, nameof(PrjFileNameMatch           ));
        PrjFileNameCompare          = GetProc<PrjFileNameCompare         >(dllPtr, nameof(PrjFileNameCompare         ));
        PrjDoesNameContainWildCards = GetProc<PrjDoesNameContainWildCards>(dllPtr, nameof(PrjDoesNameContainWildCards));
    }

    /// <summary>initialize this ProjFS API</summary>
    public static void Initialize()
    {
        if (IsInitialized) { return; }
        nint dllPtr = NativeLibrary.Load(DLLPath);

        int apiLevel;
        if (TryLoadReleaseApi(dllPtr)) {
            apiLevel = BuildNumbers.Release;

            if (TryLoadAdditionalApi(dllPtr)) { apiLevel = BuildNumbers.Additional; }
        }
        else if (TryLoadBetaApi(dllPtr)) { apiLevel = BuildNumbers.Beta; }
        else {
            throw new EntryPointNotFoundException("could not set up the ProjFS API entry point");
        }

        LoadCommandApi(dllPtr);

        NativeLibrary.Free(dllPtr);
        IsInitialized = true;
        ApiLevel = apiLevel;
    }

    private static T GetProc<T>(nint dllPtr, string funcName)
    {
        nint funcPtr = NativeLibrary.GetExport(dllPtr, funcName);
        return Marshal.GetDelegateForFunctionPointer<T>(funcPtr);
    }

    // These dummy functions are to avoid null checks when using Common ProjFS APIs to speed up things.
    private static HResult DummyPrjClearNegativePathCache(nint _, out uint @out) => (HResult) (@out = default);
    private static HResult DummyPrjDeleteFile(nint _, string __, UpdateType ___, out UpdateFailureCause @out) => (HResult) (@out = default);
    private static HResult DummyPrjGetOnDiskFileState(string _, out OnDiskFileState @out) => (HResult) (@out = default);
    private static unsafe HResult DummyPrjCompleteCommand(nint _, int __, HResult ___, PrjCompleteCommandExtendedParameters* ____) => default;
    private static HResult DummyPrjFillDirEntryBuffer(string _, ref PrjFileBasicInfo __, nint ___) => default;
    private static bool DummyPrjFileNameMatch(string _, string __) => default;
    private static int DummyPrjFileNameCompare(string _, string __) => default;
    private static bool DummyPrjDoesNameContainWildCards(string _) => default;
}