using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib.Deprecated;

internal static class CallbackTypes
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate HResult PrjGetPlaceholderInformation(
        in PrjCallbackData callbackData,
        uint desiredAccess,
        uint shareMode,
        uint createDisposition,
        uint createOptions,
        [MarshalAs(UnmanagedType.LPWStr)] string destinationFileName
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate HResult PrjGetFileStream(
        in PrjCallbackData callbackData,
        long byteOffset,
        uint length
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate HResult PrjNotifyOperation(
        in PrjCallbackData callbackData,
        [MarshalAs(UnmanagedType.I1)] bool isDirectory,
        PrjNotification notificationType,
        [MarshalAs(UnmanagedType.LPWStr)] string destinationFileName,
        ref PrjOperationParameters operationParameters
    );
}