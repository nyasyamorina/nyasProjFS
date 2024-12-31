using System.Runtime.InteropServices;
using nyasProjFS.ProjectedFSLib;

namespace nyasProjFS;

using BetaFunctionTypes = ProjectedFSLib_Deprecated.FunctionTypes;

public static class ApiHelper
{
    /// <summary>the path to the ProjFS API library file in current windows</summary>
    public static string DLLPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ProjectedFSLib.dll");

    /// <summary>the ProjFS api is initialized or not</summary>
    public static bool IsInitialized { get; private set; } = false;
    /// <summary>the windows build number of the ProjFS Api, the value can be `Beta`, `Release`, `Additional` in `BuildNumbers`, or 0 when not initialized</summary>
    public static int ApiLevel { get; private set; } = 0;

    // Release ProjFS APIs
    public static FunctionTypes.PrjStartVirtualizing?             PrjStartVirtualizing              { get; private set; }
    public static FunctionTypes.PrjStopVirtualizing?              PrjStopVirtualizing               { get; private set; }
    public static FunctionTypes.PrjWriteFileData?                 PrjWriteFileData                  { get; private set; }
    public static FunctionTypes.PrjWritePlaceholderInfo?          PrjWritePlaceholderInfo           { get; private set; }
    public static FunctionTypes.PrjAllocateAlignedBuffer?         PrjAllocateAlignedBuffer          { get; private set; }
    public static FunctionTypes.PrjFreeAlignedBuffer?             PrjFreeAlignedBuffer              { get; private set; }
    public static FunctionTypes.PrjGetVirtualizationInstanceInfo? PrjGetVirtualizationInstanceInfo  { get; private set; }
    public static FunctionTypes.PrjUpdateFileIfNeeded?            PrjUpdateFileIfNeeded             { get; private set; }
    public static FunctionTypes.PrjMarkDirectoryAsPlaceholder?    PrjMarkDirectoryAsPlaceholder     { get; private set; }
    private static bool TryLoadReleaseApi(nint dllPtr)
    {
        if (!NativeLibrary.TryGetExport(dllPtr, nameof(PrjStartVirtualizing), out nint funcPtr)) { return false; }
        PrjStartVirtualizing = Marshal.GetDelegateForFunctionPointer<ProjectedFSLib.FunctionTypes.PrjStartVirtualizing>(funcPtr);

        PrjStopVirtualizing              = GetProc<ProjectedFSLib.FunctionTypes.PrjStopVirtualizing             >(dllPtr, nameof(PrjStopVirtualizing             ));
        PrjWriteFileData                 = GetProc<ProjectedFSLib.FunctionTypes.PrjWriteFileData                >(dllPtr, nameof(PrjWriteFileData                ));
        PrjWritePlaceholderInfo          = GetProc<ProjectedFSLib.FunctionTypes.PrjWritePlaceholderInfo         >(dllPtr, nameof(PrjWritePlaceholderInfo         ));
        PrjAllocateAlignedBuffer         = GetProc<ProjectedFSLib.FunctionTypes.PrjAllocateAlignedBuffer        >(dllPtr, nameof(PrjAllocateAlignedBuffer        ));
        PrjFreeAlignedBuffer             = GetProc<ProjectedFSLib.FunctionTypes.PrjFreeAlignedBuffer            >(dllPtr, nameof(PrjFreeAlignedBuffer            ));
        PrjGetVirtualizationInstanceInfo = GetProc<ProjectedFSLib.FunctionTypes.PrjGetVirtualizationInstanceInfo>(dllPtr, nameof(PrjGetVirtualizationInstanceInfo));
        PrjUpdateFileIfNeeded            = GetProc<ProjectedFSLib.FunctionTypes.PrjUpdateFileIfNeeded           >(dllPtr, nameof(PrjUpdateFileIfNeeded           ));
        PrjMarkDirectoryAsPlaceholder    = GetProc<ProjectedFSLib.FunctionTypes.PrjMarkDirectoryAsPlaceholder   >(dllPtr, nameof(PrjMarkDirectoryAsPlaceholder   ));
        return true;
    }

    // Additional ProjFS APIs
    public static FunctionTypes.PrjWritePlaceholderInfo2? PrjWritePlaceholderInfo2 { get; private set; }
    public static FunctionTypes.PrjFillDirEntryBuffer2?   PrjFillDirEntryBuffer2   { get; private set; }
    private static bool TryLoadAdditionalApi(nint dllPtr)
    {
        if (!NativeLibrary.TryGetExport(dllPtr, nameof(PrjWritePlaceholderInfo2), out nint funcPtr)) { return false; }
        PrjWritePlaceholderInfo2 = Marshal.GetDelegateForFunctionPointer<FunctionTypes.PrjWritePlaceholderInfo2>(funcPtr);

        PrjFillDirEntryBuffer2 = GetProc<FunctionTypes.PrjFillDirEntryBuffer2>(dllPtr, nameof(PrjFillDirEntryBuffer2));
        return true;
    }

    // Beta ProjFS APIs
#pragma warning disable CS0618
    public static BetaFunctionTypes.PrjStartVirtualizationInstance?           PrjStartVirtualizationInstance           { get; private set; }
    public static BetaFunctionTypes.PrjStartVirtualizationInstanceEx?         PrjStartVirtualizationInstanceEx         { get; private set; }
    public static BetaFunctionTypes.PrjStopVirtualizationInstance?            PrjStopVirtualizationInstance            { get; private set; }
    public static BetaFunctionTypes.PrjGetVirtualizationInstanceIdFromHandle? PrjGetVirtualizationInstanceIdFromHandle { get; private set; }
    public static BetaFunctionTypes.PrjConvertDirectoryToPlaceholder?         PrjConvertDirectoryToPlaceholder         { get; private set; }
    public static BetaFunctionTypes.PrjWritePlaceholderInformation?           PrjWritePlaceholderInformation           { get; private set; }
    public static BetaFunctionTypes.PrjUpdatePlaceholderIfNeeded?             PrjUpdatePlaceholderIfNeeded             { get; private set; }
    public static BetaFunctionTypes.PrjWriteFile?                             PrjWriteFile                             { get; private set; }
    public static BetaFunctionTypes.PrjCommandCallbacksInit?                  PrjCommandCallbacksInit                  { get; private set; }
    private static bool TryLoadBetaApi(nint dllPtr)
    {
        if (!NativeLibrary.TryGetExport(dllPtr, nameof(PrjStartVirtualizationInstance), out nint funcPtr)) { return false; }
        PrjStartVirtualizationInstance = Marshal.GetDelegateForFunctionPointer<BetaFunctionTypes.PrjStartVirtualizationInstance>(funcPtr);

        PrjStartVirtualizationInstanceEx         = GetProc<BetaFunctionTypes.PrjStartVirtualizationInstanceEx        >(dllPtr, nameof(PrjStartVirtualizationInstanceEx        ));
        PrjStopVirtualizationInstance            = GetProc<BetaFunctionTypes.PrjStopVirtualizationInstance           >(dllPtr, nameof(PrjStopVirtualizationInstance           ));
        PrjGetVirtualizationInstanceIdFromHandle = GetProc<BetaFunctionTypes.PrjGetVirtualizationInstanceIdFromHandle>(dllPtr, nameof(PrjGetVirtualizationInstanceIdFromHandle));
        PrjConvertDirectoryToPlaceholder         = GetProc<BetaFunctionTypes.PrjConvertDirectoryToPlaceholder        >(dllPtr, nameof(PrjConvertDirectoryToPlaceholder        ));
        PrjWritePlaceholderInformation           = GetProc<BetaFunctionTypes.PrjWritePlaceholderInformation          >(dllPtr, nameof(PrjWritePlaceholderInformation          ));
        PrjUpdatePlaceholderIfNeeded             = GetProc<BetaFunctionTypes.PrjUpdatePlaceholderIfNeeded            >(dllPtr, nameof(PrjUpdatePlaceholderIfNeeded            ));
        PrjWriteFile                             = GetProc<BetaFunctionTypes.PrjWriteFile                            >(dllPtr, nameof(PrjWriteFile                            ));
        PrjCommandCallbacksInit                  = GetProc<BetaFunctionTypes.PrjCommandCallbacksInit                 >(dllPtr, nameof(PrjCommandCallbacksInit                 ));
        return true;
    }
#pragma warning restore CS0618

    // Common ProjFS APIs
    public static FunctionTypes.PrjClearNegativePathCache   PrjClearNegativePathCache   { get; private set; } = DummyPrjClearNegativePathCache;
    public static FunctionTypes.PrjDeleteFile               PrjDeleteFile               { get; private set; } = DummyPrjDeleteFile;
    public static FunctionTypes.PrjGetOnDiskFileState       PrjGetOnDiskFileState       { get; private set; } = DummyPrjGetOnDiskFileState;
    public static FunctionTypes.PrjCompleteCommand          PrjCompleteCommand          { get; private set; } = DummyPrjCompleteCommand;
    public static FunctionTypes.PrjFillDirEntryBuffer       PrjFillDirEntryBuffer       { get; private set; } = DummyPrjFillDirEntryBuffer;
    public static FunctionTypes.PrjFileNameMatch            PrjFileNameMatch            { get; private set; } = DummyPrjFileNameMatch;
    public static FunctionTypes.PrjFileNameCompare          PrjFileNameCompare          { get; private set; } = DummyPrjFileNameCompare;
    public static FunctionTypes.PrjDoesNameContainWildCards PrjDoesNameContainWildCards { get; private set; } = DummyPrjDoesNameContainWildCards;
    private static void LoadCommandApi(nint dllPtr)
    {
        PrjClearNegativePathCache   = GetProc<FunctionTypes.PrjClearNegativePathCache  >(dllPtr, nameof(PrjClearNegativePathCache  ));
        PrjDeleteFile               = GetProc<FunctionTypes.PrjDeleteFile              >(dllPtr, nameof(PrjDeleteFile              ));
        PrjGetOnDiskFileState       = GetProc<FunctionTypes.PrjGetOnDiskFileState      >(dllPtr, nameof(PrjGetOnDiskFileState      ));
        PrjCompleteCommand          = GetProc<FunctionTypes.PrjCompleteCommand         >(dllPtr, nameof(PrjCompleteCommand         ));
        PrjFillDirEntryBuffer       = GetProc<FunctionTypes.PrjFillDirEntryBuffer      >(dllPtr, nameof(PrjFillDirEntryBuffer      ));
        PrjFileNameMatch            = GetProc<FunctionTypes.PrjFileNameMatch           >(dllPtr, nameof(PrjFileNameMatch           ));
        PrjFileNameCompare          = GetProc<FunctionTypes.PrjFileNameCompare         >(dllPtr, nameof(PrjFileNameCompare         ));
        PrjDoesNameContainWildCards = GetProc<FunctionTypes.PrjDoesNameContainWildCards>(dllPtr, nameof(PrjDoesNameContainWildCards));
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
    private static HResult DummyPrjCompleteCommand(nint _, int __, HResult ___, ref PrjCompleteCommandExtendedParameters ____) => default;
    private static HResult DummyPrjFillDirEntryBuffer(string _, ref PrjFileBasicInfo __, nint ___) => default;
    private static bool DummyPrjFileNameMatch(string _, string __) => default;
    private static int DummyPrjFileNameCompare(string _, string __) => default;
    private static bool DummyPrjDoesNameContainWildCards(string _) => default;
}