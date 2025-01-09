using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib;

using PrjDirEntryBufferHandle = nint;

internal static class CallbackTypes
{
    internal static unsafe class Underlying
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate HResult PrjStartDirectoryEnumeration(
            PrjCallbackDataUnmanaged* callbackData,
            Guid* enumerationId
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate HResult PrjGetDirectoryEnumeration(
            PrjCallbackDataUnmanaged* callbackData,
            Guid* enumerationId,
            ushort* searchExpression,
            PrjDirEntryBufferHandle dirEntryBufferHandle
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate HResult PrjEndDirectoryEnumeration(
            PrjCallbackDataUnmanaged* callbackData,
            Guid* enumerationId
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate HResult PrjGetPlaceholderInfo(
            PrjCallbackDataUnmanaged* callbackData
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate HResult PrjGetFileData(
            PrjCallbackDataUnmanaged* callbackData,
            ulong byteOffset,
            uint length
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate HResult PrjQueryFileName(
            PrjCallbackDataUnmanaged* callbackData
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate HResult PrjNotification(
            PrjCallbackDataUnmanaged* callbackData,
            byte isDirectory,
            ProjectedFSLib.PrjNotification notification,
            ushort* destinationFileName,
            PrjNotificationParameters* notificationParameters
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void PrjCancelCommand(
            PrjCallbackDataUnmanaged* callbackData
        );
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate HResult PrjStartDirectoryEnumeration(
        in PrjCallbackData callbackData,
        in Guid enumerationId
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal delegate HResult PrjGetDirectoryEnumeration(
        in PrjCallbackData callbackData,
        in Guid enumerationId,
        string? searchExpression,
        PrjDirEntryBufferHandle dirEntryBufferHandle
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate HResult PrjEndDirectoryEnumeration(
        in PrjCallbackData callbackData,
        in Guid enumerationId
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate HResult PrjGetPlaceholderInfo(
        in PrjCallbackData callbackData
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate HResult PrjGetFileData(
        in PrjCallbackData callbackData,
        ulong byteOffset,
        uint length
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate HResult PrjQueryFileName(
        in PrjCallbackData callbackData
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    internal delegate HResult PrjNotification(
        in PrjCallbackData callbackData,
        [MarshalAs(UnmanagedType.I1)] bool isDirectory,
        ProjectedFSLib.PrjNotification notification,
        string? destinationFileName,
        ref PrjNotificationParameters notificationParameters
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void PrjCancelCommand(
        in PrjCallbackData callbackData
    );
}