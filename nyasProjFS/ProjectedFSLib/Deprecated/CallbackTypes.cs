using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib.Deprecated;

internal static class CallbackTypes
{
    internal static unsafe class Underlying
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate HResult PrjGetPlaceholderInformation(
            [In] PrjCallbackDataUnmanaged* callbackData,
            [In] uint desiredAccess,
            [In] uint shareMode,
            [In] uint createDisposition,
            [In] uint createOptions,
            [In] ushort* destinationFileName
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate HResult PrjGetFileStream(
            [In] PrjCallbackDataUnmanaged* callbackData,
            [In] long byteOffset,
            [In] uint length
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate HResult PrjNotifyOperation(
            [In] PrjCallbackDataUnmanaged* callbackData,
            [In] byte isDirectory,
            [In] PrjNotification notificationType,
            [In] ushort* destinationFileName,
            [In] PrjOperationParameters* operationParameters
        );
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal delegate HResult PrjGetPlaceholderInformation(
        in PrjCallbackData callbackData,
        uint desiredAccess,
        uint shareMode,
        uint createDisposition,
        uint createOptions,
        string destinationFileName
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate HResult PrjGetFileStream(
        in PrjCallbackData callbackData,
        long byteOffset,
        uint length
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal delegate HResult PrjNotifyOperation(
        in PrjCallbackData callbackData,
        [MarshalAs(UnmanagedType.I1)] bool isDirectory,
        PrjNotification notificationType,
        string? destinationFileName,
        ref PrjOperationParameters operationParameters
    );
}