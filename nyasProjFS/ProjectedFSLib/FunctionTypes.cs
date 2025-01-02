using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib;

using PrjNamespaceVirtualizationContext = nint;
using PrjDirEntryBufferHandle = nint;

public static class FunctionTypes
{
    #region Virtualization instance APIs

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
    public delegate HResult PrjStartVirtualizing(
        [MarshalAs(UnmanagedType.LPTStr)] string virtualizationRootPath,
        ref PrjCallbacks callbacks,
        nint instanceContext,
        ref PrjStartVirtualizingOptions options,
        out PrjNamespaceVirtualizationContext namespaceVirtualizationContext
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
    public delegate void PrjStopVirtualizing(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Common
    public delegate HResult PrjClearNegativePathCache(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        out uint totalEntryNumber
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
    public delegate HResult PrjGetVirtualizationInstanceInfo(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        out PrjVirtualizationInstanceInfo virtualizationInstanceInfo
    );

    #endregion // Virtualization instance APIs
    #region Placeholder and File APIs

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
    public delegate HResult PrjMarkDirectoryAsPlaceholder(
        [MarshalAs(UnmanagedType.LPTStr)] string rootPathName,
        [MarshalAs(UnmanagedType.LPTStr)] string targetPathName,
        ref PrjPlaceholderVersionInfo versionInfo,
        ref Guid virtualizationInstanceId
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
    public delegate HResult PrjWritePlaceholderInfo(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        [MarshalAs(UnmanagedType.LPTStr)] string destinationFileName,
        ref PrjPlaceholderInfo placeholderInfo,
        uint placeholderInfoSize
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Additional
    public delegate HResult PrjWritePlaceholderInfo2(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        [MarshalAs(UnmanagedType.LPTStr)] string destinationFileName,
        ref PrjPlaceholderInfo placeholderInfo,
        uint placeholderInfoSize,
        ref PrjExtendedInfo extendedInfo
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
    public delegate HResult PrjUpdateFileIfNeeded(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        [MarshalAs(UnmanagedType.LPTStr)] string destinationFileName,
        ref PrjPlaceholderInfo placeholderInfo,
        uint placeholderInfoSize,
        UpdateType updateFlags,
        out UpdateFailureCause failureReason
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Common
    public delegate HResult PrjDeleteFile(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        [MarshalAs(UnmanagedType.LPTStr)] string destinationFileName,
        UpdateType updateFlags,
        out UpdateFailureCause failureReason
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
    public delegate HResult PrjWriteFileData(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        ref Guid dataStreamId,
        nint buffer,
        ulong byteOffset,
        uint length
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Common
    public delegate HResult PrjGetOnDiskFileState(
        [MarshalAs(UnmanagedType.LPTStr)] string destinationFileName,
        out OnDiskFileState fileState
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
    public delegate nint PrjAllocateAlignedBuffer(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        ulong size
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Release
    public delegate void PrjFreeAlignedBuffer(
        nint buffer
    );

    #endregion // Placeholder and File APIs
    #region Callback support

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Common
    public unsafe delegate HResult PrjCompleteCommand(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        int commandId,
        HResult completionResult,
        PrjCompleteCommandExtendedParameters* extendedParameters // null or ref
    );

    #endregion // Callback support
    #region Enumeration APIs

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Common
    public delegate HResult PrjFillDirEntryBuffer(
        [MarshalAs(UnmanagedType.LPTStr)] string fileName,
        ref PrjFileBasicInfo fileBasicInfo,
        PrjDirEntryBufferHandle dirEntryBufferHandle
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Additional
    public delegate HResult PrjFillDirEntryBuffer2(
        PrjDirEntryBufferHandle dirEntryBufferHandle,
        [MarshalAs(UnmanagedType.LPTStr)] string fileName,
        ref PrjFileBasicInfo fileBasicInfo,
        ref PrjExtendedInfo extendedInfo
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] // Common
    public delegate bool PrjFileNameMatch(
        [MarshalAs(UnmanagedType.LPTStr)] string fileNameToCheck,
        [MarshalAs(UnmanagedType.LPTStr)] string pattern
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // Common
    public delegate int PrjFileNameCompare(
        [MarshalAs(UnmanagedType.LPTStr)] string fileName1,
        [MarshalAs(UnmanagedType.LPTStr)] string fileName2
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] // Common
    public delegate bool PrjDoesNameContainWildCards(
        [MarshalAs(UnmanagedType.LPTStr)] string fileName
    );

    #endregion // Enumeration APIs
}
