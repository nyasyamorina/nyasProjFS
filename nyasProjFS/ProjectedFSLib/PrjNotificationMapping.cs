using System.Runtime.InteropServices.Marshalling;

namespace nyasProjFS.ProjectedFSLib;

[NativeMarshalling(typeof(PrjNotificationMappingMarshaller))]
internal struct PrjNotificationMapping
{
    internal NotificationType NotificationBitMask;
    internal string NotificationRoot;
}

// ?? `MarshalAs` is not working with `LibraryImport` ??

internal unsafe struct PrjNotificationMappingUnmanaged
{
    internal NotificationType NotificationBitMask;
    internal ushort* NotificationRoot;
}

[CustomMarshaller(typeof(PrjNotificationMapping), MarshalMode.ManagedToUnmanagedIn, typeof(PrjNotificationMappingMarshaller))]
internal static class PrjNotificationMappingMarshaller
{
    internal static unsafe PrjNotificationMappingUnmanaged ConvertToUnmanaged(in PrjNotificationMapping managed)
    {
        ushort* notificationRootPtr = Utf16StringMarshaller.ConvertToUnmanaged(managed.NotificationRoot);

        PrjNotificationMappingUnmanaged unmanaged = default;
        unmanaged.NotificationBitMask = managed.NotificationBitMask;
        unmanaged.NotificationRoot = notificationRootPtr;
        return unmanaged;
    }

    internal static unsafe void Free(ref PrjNotificationMappingUnmanaged unmanaged)
    {
        ushort* notificationRootPtr = unmanaged.NotificationRoot;
        Utf16StringMarshaller.Free(notificationRootPtr);
        unmanaged.NotificationRoot = null;
    }
}