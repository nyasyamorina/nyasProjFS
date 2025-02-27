using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib;

[StructLayout(LayoutKind.Explicit, Size = 4)]
internal struct PrjNotificationParameters
{
    internal struct PostCreateType
    {
        internal NotificationType NotificationMask;
    }
    internal struct FileRenamedType
    {
        internal NotificationType NotificationMask;
    }
    internal struct FileDeletedOnHandleCloseType
    {
        [MarshalAs(UnmanagedType.I1)]
        internal bool IsFileModified;
    }

    [FieldOffset(0)]
    internal PostCreateType PostCreate;
    [FieldOffset(0)]
    internal FileRenamedType FileRenamed;
    [FieldOffset(0)]
    internal FileDeletedOnHandleCloseType FileDeletedOnHandleClose;
}