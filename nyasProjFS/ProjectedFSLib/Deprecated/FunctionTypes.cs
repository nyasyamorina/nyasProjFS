using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib.Deprecated;

using PrjVirtualizationInstanceHandle = nint;

internal static class FunctionTypes
{
    #region Virtualization instance APIs

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [Obsolete("use PrjStartVirtualizing instead of PrjStartVirtualizationInstance")]
    internal delegate HResult PrjStartVirtualizationInstance(
        [MarshalAs(UnmanagedType.LPWStr)] string virtualizationRootPath,
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
    internal delegate HResult PrjStartVirtualizationInstanceEx(
        [MarshalAs(UnmanagedType.LPWStr)] string virtualizationRootPath,
        ref PrjCommandCallbacks callbacks,
        nint instanceContext,
        ref VirtualizationInstExtendedParameters extendedParameters,
        out PrjVirtualizationInstanceHandle virtualizationInstanceHandle
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [Obsolete("use PrjStopVirtualizing instead of PrjStopVirtualizationInstance")]
    internal delegate HResult PrjStopVirtualizationInstance(
        PrjVirtualizationInstanceHandle virtualizationInstanceHandle
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [Obsolete("use PrjGetVirtualizationInstanceInfo instead of PrjGetVirtualizationInstanceIdFromHandle")]
    internal delegate HResult PrjGetVirtualizationInstanceIdFromHandle(
        PrjVirtualizationInstanceHandle virtualizationInstanceHandle,
        out Guid virtualizationInstanceId
    );

    #endregion // Virtualization instance APIs
    #region Placeholder and File APIs

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [Obsolete("use PrjMarkDirectoryAsPlaceholder instead of PrjConvertDirectoryToPlaceholder")]
    internal delegate HResult PrjConvertDirectoryToPlaceholder(
        [MarshalAs(UnmanagedType.LPWStr)] string rootPathName,
        [MarshalAs(UnmanagedType.LPWStr)] string targetPathName,
        ref PrjPlaceholderVersionInfo versionInfo,
        uint flags,
        ref Guid virtualizationInstanceId
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [Obsolete("use PrjWritePlaceholderInfo instead of PrjWritePlaceholderInformation")]
    internal delegate HResult PrjWritePlaceholderInformation(
        PrjVirtualizationInstanceHandle virtualizationInstanceHandle,
        [MarshalAs(UnmanagedType.LPWStr)] string destinationFileName,
        ref PrjPlaceholderInformation placeholderInformation,
        uint length
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [Obsolete("use PrjUpdateFileIfNeeded instead of PrjUpdatePlaceholderIfNeeded")]
    internal delegate HResult PrjUpdatePlaceholderIfNeeded(
        PrjVirtualizationInstanceHandle virtualizationInstanceHandle,
        [MarshalAs(UnmanagedType.LPWStr)] string destinationFileName,
        ref PrjPlaceholderInformation placeholderInformation,
        uint length,
        UpdateType updateFlags,
        out UpdateFailureCause failureReason
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [Obsolete("Use PrjWriteFileData instead of PrjWriteFile")]
    internal delegate HResult PrjWriteFile(
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
    internal delegate HResult PrjCommandCallbacksInit(
        uint callbackSize,
        out PrjCommandCallbacks callbacks
    );

    #endregion // Callback support
}