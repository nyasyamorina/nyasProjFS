using System.Runtime.InteropServices;

namespace nyasProjFS;

public static partial class ApiHelper
{
    private static partial class Win32Native
    {
        [LibraryImport("kernel32.dll", EntryPoint = "LoadLibraryW", StringMarshalling = StringMarshalling.Utf16)]
        internal static partial nint LoadLibrary(string dllToLoad);

        [LibraryImport("kernel32.dll", EntryPoint = "GetProcAddress", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial nint GetProcAddress(nint hModule, string procedureName);

        [LibraryImport("kernel32.dll", EntryPoint = "FreeLibrary")] [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool FreeLibrary(nint hModule);
    }

    /// <summary>the path to the ProjFS API library file in current windows</summary>
    public static string DLLPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ProjectedFSLib.dll");

    /// <summary>the ProjFS api is initialized or not</summary>
    public static bool IsInitialized { get; private set; } = false;
    /// <summary>the windows build number of the ProjFS Api, the value can be `Beta`, `Release`, `Additional` in `BuildNumbers`, or 0 when not initialized</summary>
    public static int ApiLevel { get; private set; } = 0;

    /// <summary>initialize this ProjFS API</summary>
    public static void Initialize()
    {
        if (IsInitialized) { return; }

        if (!File.Exists(DLLPath)) { throw new FileNotFoundException("could not find the ProjFS API library file", DLLPath); }

        nint dllPtr = Win32Native.LoadLibrary(DLLPath);
        if (dllPtr == nint.Zero) { throw new FileLoadException("could not load the ProjFS API library", DLLPath); }

        int apiLevel;
        if (HasReleaseApi(dllPtr)) {
            apiLevel = BuildNumbers.Release;

            PrjStartVirtualizing             = GetProc<ApiFunctions.PrjStartVirtualizing            >(dllPtr, nameof(PrjStartVirtualizing            ));
            PrjStopVirtualizing              = GetProc<ApiFunctions.PrjStopVirtualizing             >(dllPtr, nameof(PrjStopVirtualizing             ));
            PrjWriteFileData                 = GetProc<ApiFunctions.PrjWriteFileData                >(dllPtr, nameof(PrjWriteFileData                ));
            PrjWritePlaceholderInfo          = GetProc<ApiFunctions.PrjWritePlaceholderInfo         >(dllPtr, nameof(PrjWritePlaceholderInfo         ));
            PrjAllocateAlignedBuffer         = GetProc<ApiFunctions.PrjAllocateAlignedBuffer        >(dllPtr, nameof(PrjAllocateAlignedBuffer        ));
            PrjFreeAlignedBuffer             = GetProc<ApiFunctions.PrjFreeAlignedBuffer            >(dllPtr, nameof(PrjFreeAlignedBuffer            ));
            PrjGetVirtualizationInstanceInfo = GetProc<ApiFunctions.PrjGetVirtualizationInstanceInfo>(dllPtr, nameof(PrjGetVirtualizationInstanceInfo));
            PrjUpdateFileIfNeeded            = GetProc<ApiFunctions.PrjUpdateFileIfNeeded           >(dllPtr, nameof(PrjUpdateFileIfNeeded           ));
            PrjMarkDirectoryAsPlaceholder    = GetProc<ApiFunctions.PrjMarkDirectoryAsPlaceholder   >(dllPtr, nameof(PrjMarkDirectoryAsPlaceholder   ));

            if (HasAdditionalApi(dllPtr)) {
                apiLevel = BuildNumbers.Additional;

                PrjWritePlaceholderInfo2 = GetProc<ApiFunctions.PrjWritePlaceholderInfo2>(dllPtr, nameof(PrjWritePlaceholderInfo2));
                PrjFillDirEntryBuffer2   = GetProc<ApiFunctions.PrjFillDirEntryBuffer2  >(dllPtr, nameof(PrjFillDirEntryBuffer2  ));
            }
        }
        else if (HasBetaApi(dllPtr)) {
            apiLevel = BuildNumbers.Beta;

            PrjStartVirtualizationInstance           = GetProc<ApiFunctions.PrjStartVirtualizationInstance          >(dllPtr, nameof(PrjStartVirtualizationInstance          ));
            PrjStartVirtualizationInstanceEx         = GetProc<ApiFunctions.PrjStartVirtualizationInstanceEx        >(dllPtr, nameof(PrjStartVirtualizationInstanceEx        ));
            PrjStopVirtualizationInstance            = GetProc<ApiFunctions.PrjStopVirtualizationInstance           >(dllPtr, nameof(PrjStopVirtualizationInstance           ));
            PrjGetVirtualizationInstanceIdFromHandle = GetProc<ApiFunctions.PrjGetVirtualizationInstanceIdFromHandle>(dllPtr, nameof(PrjGetVirtualizationInstanceIdFromHandle));
            PrjConvertDirectoryToPlaceholder         = GetProc<ApiFunctions.PrjConvertDirectoryToPlaceholder        >(dllPtr, nameof(PrjConvertDirectoryToPlaceholder        ));
            PrjWritePlaceholderInformation           = GetProc<ApiFunctions.PrjWritePlaceholderInformation          >(dllPtr, nameof(PrjWritePlaceholderInformation          ));
            PrjUpdatePlaceholderIfNeeded             = GetProc<ApiFunctions.PrjUpdatePlaceholderIfNeeded            >(dllPtr, nameof(PrjUpdatePlaceholderIfNeeded            ));
            PrjWriteFile                             = GetProc<ApiFunctions.PrjWriteFile                            >(dllPtr, nameof(PrjWriteFile                            ));
            PrjCommandCallbacksInit                  = GetProc<ApiFunctions.PrjCommandCallbacksInit                 >(dllPtr, nameof(PrjCommandCallbacksInit                 ));
        }
        else {
            throw new MissingMethodException("could not set up the ProjFS API entry point");
        }

        Win32Native.FreeLibrary(dllPtr);
        IsInitialized = true;
        ApiLevel = apiLevel;
    }

    private static bool HasBetaApi(nint dllPtr) => Win32Native.GetProcAddress(dllPtr, nameof(PrjStartVirtualizationInstance)) != nint.Zero;
    private static bool HasReleaseApi(nint dllPtr) => Win32Native.GetProcAddress(dllPtr, nameof(PrjStartVirtualizing)) != nint.Zero;
    private static bool HasAdditionalApi(nint dllPtr) => Win32Native.GetProcAddress(dllPtr, nameof(PrjWritePlaceholderInfo2)) != nint.Zero;

    private static T GetProc<T>(nint dllPtr, string funcName)
    {
        nint funcPtr = Win32Native.GetProcAddress(dllPtr, funcName);
        if (funcPtr == nint.Zero) { throw new MissingMethodException("could not find function in the ProjFS API", funcName); }
        return Marshal.GetDelegateForFunctionPointer<T>(funcPtr);
    }

    public static class ApiFunctions
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
        public delegate int PrjStartVirtualizing(nint virtualizationRootPath, nint callbacks, nint instanceContext, nint options, nint namespaceVirtualizationContext);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
        public delegate void PrjStopVirtualizing(nint namespaceVirtualizationContext);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
        public delegate int PrjWriteFileData(nint namespaceVirtualizationContext, nint dataStreamId, nint buffer, ulong byteOffset, uint length);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
        public delegate int PrjWritePlaceholderInfo(nint namespaceVirtualizationContext, nint destinationFileName, nint placeholderInfo, uint placeholderInfoSize);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
        public delegate nint PrjAllocateAlignedBuffer(nint namespaceVirtualizationContext, ulong size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
        public delegate void PrjFreeAlignedBuffer(nint buffer);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
        public delegate int PrjGetVirtualizationInstanceInfo(nint namespaceVirtualizationContext, nint virtualizationInstanceInfo);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
        public delegate int PrjUpdateFileIfNeeded(nint namespaceVirtualizationContext, nint destinationFileName, nint placeholderInfo, uint placeholderInfoSize, int updateFlags, nint failureReason);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
        public delegate int PrjMarkDirectoryAsPlaceholder(nint rootPathName, nint targetPathName, nint versionInfo, nint virtualizationInstanceID);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Additional
        public delegate int PrjWritePlaceholderInfo2(nint namespaceVirtualizationContext, nint destinationFileName, nint placeholderInfo, uint placeholderInfoSize, nint extendedInfo);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Additional
        public delegate int PrjFillDirEntryBuffer2(nint dirEntryBufferHandle, nint fileName, nint fileBasicInfo, nint extendedInfo);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Beta
        public delegate int PrjStartVirtualizationInstance(nint virtualizationRootPath, nint callbacks, uint flags, uint globalNotificationMask, uint poolThreadCount, uint concurrentThreadCount, nint instanceContext, nint virtualizationInstanceHandle);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Beta
        public delegate int PrjStartVirtualizationInstanceEx(nint virtualizationRootPath, nint callbacks, nint instanceContext, nint extendedParameters, nint virtualizationInstanceHandle);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Beta
        public delegate int PrjStopVirtualizationInstance(nint virtualizationInstanceHandle);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Beta
        public delegate int PrjGetVirtualizationInstanceIdFromHandle(nint virtualizationInstanceHandle, nint virtualizationInstanceID);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Beta
        public delegate int PrjConvertDirectoryToPlaceholder(nint rootPathName, nint targetPathName, nint versionInfo, uint flags, nint virtualizationInstanceID);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Beta
        public delegate int PrjWritePlaceholderInformation(nint virtualizationInstanceHandle, nint destinationFileName, nint placeholderInformation, uint length);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Beta
        public delegate int PrjUpdatePlaceholderIfNeeded(nint virtualizationInstanceHandle, nint destinationFileName, nint placeholderInformation, uint length, uint updateFlags, nint failureReason);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Beta
        public delegate int PrjWriteFile(nint virtualizationInstanceHandle, nint streamId, nint buffer, ulong byteOffset, uint length);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Beta
        public delegate int PrjCommandCallbacksInit(uint callbackSize, nint callbacks);
    }

    // Release
    public static ApiFunctions.PrjStartVirtualizing?                     PrjStartVirtualizing                     { get; private set;}
    public static ApiFunctions.PrjStopVirtualizing?                      PrjStopVirtualizing                      { get; private set;}
    public static ApiFunctions.PrjWriteFileData?                         PrjWriteFileData                         { get; private set;}
    public static ApiFunctions.PrjWritePlaceholderInfo?                  PrjWritePlaceholderInfo                  { get; private set;}
    public static ApiFunctions.PrjAllocateAlignedBuffer?                 PrjAllocateAlignedBuffer                 { get; private set;}
    public static ApiFunctions.PrjFreeAlignedBuffer?                     PrjFreeAlignedBuffer                     { get; private set;}
    public static ApiFunctions.PrjGetVirtualizationInstanceInfo?         PrjGetVirtualizationInstanceInfo         { get; private set;}
    public static ApiFunctions.PrjUpdateFileIfNeeded?                    PrjUpdateFileIfNeeded                    { get; private set;}
    public static ApiFunctions.PrjMarkDirectoryAsPlaceholder?            PrjMarkDirectoryAsPlaceholder            { get; private set;}
    // Additional
    public static ApiFunctions.PrjWritePlaceholderInfo2?                 PrjWritePlaceholderInfo2                 { get; private set;}
    public static ApiFunctions.PrjFillDirEntryBuffer2?                   PrjFillDirEntryBuffer2                   { get; private set;}
    // Beta
    public static ApiFunctions.PrjStartVirtualizationInstance?           PrjStartVirtualizationInstance           { get; private set;}
    public static ApiFunctions.PrjStartVirtualizationInstanceEx?         PrjStartVirtualizationInstanceEx         { get; private set;}
    public static ApiFunctions.PrjStopVirtualizationInstance?            PrjStopVirtualizationInstance            { get; private set;}
    public static ApiFunctions.PrjGetVirtualizationInstanceIdFromHandle? PrjGetVirtualizationInstanceIdFromHandle { get; private set;}
    public static ApiFunctions.PrjConvertDirectoryToPlaceholder?         PrjConvertDirectoryToPlaceholder         { get; private set;}
    public static ApiFunctions.PrjWritePlaceholderInformation?           PrjWritePlaceholderInformation           { get; private set;}
    public static ApiFunctions.PrjUpdatePlaceholderIfNeeded?             PrjUpdatePlaceholderIfNeeded             { get; private set;}
    public static ApiFunctions.PrjWriteFile?                             PrjWriteFile                             { get; private set;}
    public static ApiFunctions.PrjCommandCallbacksInit?                  PrjCommandCallbacksInit                  { get; private set;}
}