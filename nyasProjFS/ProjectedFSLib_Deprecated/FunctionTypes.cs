using System.Runtime.InteropServices;
using nyasProjFS.ProjectedFSLib;

namespace nyasProjFS.ProjectedFSLib_Deprecated;

using PrjVirtualizationInstanceHandle = nint;

public static class FunctionTypes
{
    #region Virtualization instance APIs

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [Obsolete("use PrjStartVirtualizing instead of PrjStartVirtualizationInstance")]
    public delegate HResult PrjStartVirtualizationInstance(
        [MarshalAs(UnmanagedType.LPTStr)] string virtualizationRootPath,
        ref PrjCommandCallbacks callbacks,
        uint flags,
        uint globalNotificationMask,
        uint poolThreadCount,
        uint concurrentThreadCount,
        nint instanceContext,
        out PrjVirtualizationInstanceHandle virtualizationInstanceHandle
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [Obsolete("use PrjStartVirtualizing instead of PrjStartVirtualizationInstanceEx")]
    public delegate HResult PrjStartVirtualizationInstanceEx(
        [MarshalAs(UnmanagedType.LPTStr)] string virtualizationRootPath,
        ref PrjCommandCallbacks callbacks,
        nint instanceContext,
        ref VirtualizationInstExtendedParameters extendedParameters,
        out PrjVirtualizationInstanceHandle virtualizationInstanceHandle
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [Obsolete("use PrjStopVirtualizing instead of PrjStopVirtualizationInstance")]
    public delegate HResult PrjStopVirtualizationInstance(
        PrjVirtualizationInstanceHandle virtualizationInstanceHandle
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [Obsolete("use PrjGetVirtualizationInstanceInfo instead of PrjGetVirtualizationInstanceIdFromHandle")]
    public delegate HResult PrjGetVirtualizationInstanceIdFromHandle(
        PrjVirtualizationInstanceHandle virtualizationInstanceHandle,
        out Guid virtualizationInstanceId
    );

    #endregion // Virtualization instance APIs
    #region Placeholder and File APIs

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [Obsolete("use PrjMarkDirectoryAsPlaceholder instead of PrjConvertDirectoryToPlaceholder")]
    public delegate HResult PrjConvertDirectoryToPlaceholder(
        [MarshalAs(UnmanagedType.LPTStr)] string rootPathName,
        [MarshalAs(UnmanagedType.LPTStr)] string targetPathName,
        ref PrjPlaceholderVersionInfo versionInfo,
        uint flags,
        ref Guid virtualizationInstanceId
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [Obsolete("use PrjWritePlaceholderInfo instead of PrjWritePlaceholderInformation")]
    public delegate HResult PrjWritePlaceholderInformation(
        PrjVirtualizationInstanceHandle virtualizationInstanceHandle,
        [MarshalAs(UnmanagedType.LPTStr)] string destinationFileName,
        ref PrjPlaceholderInformation placeholderInformation,
        uint length
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [Obsolete("use PrjUpdateFileIfNeeded instead of PrjUpdatePlaceholderIfNeeded")]
    public delegate HResult PrjUpdatePlaceholderIfNeeded(
        PrjVirtualizationInstanceHandle virtualizationInstanceHandle,
        [MarshalAs(UnmanagedType.LPTStr)] string destinationFileName,
        ref PrjPlaceholderInformation placeholderInformation,
        uint length,
        UpdateType updateFlags,
        out UpdateFailureCause failureReason
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [Obsolete("Use PrjWriteFileData instead of PrjWriteFile")]
    public delegate HResult PrjWriteFile(
        PrjVirtualizationInstanceHandle virtualizationInstanceHandle,
        ref Guid streamId,
        nint buffer,
        ulong byteOffset,
        uint length
    );

    #endregion // Placeholder and File APIs
    #region Callback support

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [Obsolete("PrjCommandCallbacksInit is deprecated and will not exist in future versions of Windows")]
    public delegate HResult PrjCommandCallbacksInit(
        uint callbackSize,
        out PrjCommandCallbacks callbacks
    );

    #endregion // Callback support
}