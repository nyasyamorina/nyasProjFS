using System.Runtime.InteropServices;
using nyasProjFS.ProjectedFSLib;

namespace nyasProjFS.ProjectedFSLib_Deprecated;

public static class CallbackTypes
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate HResult PrjGetPlaceholderInformation(
        ref PrjCallbackData callbackData,
        uint desiredAccess,
        uint shareMode,
        uint createDisposition,
        uint createOptions,
        [MarshalAs(UnmanagedType.LPTStr)] string destinationFileName
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate HResult PrjGetFileStream(
        ref PrjCallbackData callbackData,
        long byteOffset,
        uint length
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate HResult PrjNotifyOperation(
        ref PrjCallbackData callbackData,
        [MarshalAs(UnmanagedType.I1)] bool isDirectory,
        PrjNotification notificationType,
        [MarshalAs(UnmanagedType.LPTStr)] string destinationFileName,
        ref PrjOperationParameters operationParameters
    );
}