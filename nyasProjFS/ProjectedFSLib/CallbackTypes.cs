using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib;

using PrjDirEntryBufferHandle = nint;

public static class CallbackTypes
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate HResult PrjStartDirectoryEnumeration(
        ref PrjCallbackData callbackData,
        ref Guid enumerationId
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate HResult PrjGetDirectoryEnumeration(
        ref PrjCallbackData callbackData,
        ref Guid enumerationId,
        [MarshalAs(UnmanagedType.LPTStr)] string searchExpression,
        PrjDirEntryBufferHandle dirEntryBufferHandle
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate HResult PrjEndDirectoryEnumeration(
        ref PrjCallbackData callbackData,
        ref Guid enumerationId
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate HResult PrjGetPlaceholderInfo(
        ref PrjCallbackData callbackData
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate HResult PrjGetFileData(
        ref PrjCallbackData callbackData,
        ulong byteOffset,
        uint length
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate HResult PrjQueryFileName(
        ref PrjCallbackData callbackData
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate HResult PrjNotification(
        ref PrjCallbackData callbackData,
        [MarshalAs(UnmanagedType.I1)] bool isDirectory,
        PrjNotification notification,
        [MarshalAs(UnmanagedType.LPTStr)] string destinationFileName,
        ref PrjNotificationParameters operationParameters
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate HResult PrjCancelCommand(
        ref PrjCallbackData callbackData
    );
}