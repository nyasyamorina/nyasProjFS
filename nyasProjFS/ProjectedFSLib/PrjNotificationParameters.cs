using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib;

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct PrjNotificationParameters
{
    public struct PostCreateType
    {
        public NotificationType NotificationMask;
    }
    public struct FileRenamedType
    {
        public NotificationType NotificationMask;
    }
    public struct FileDeletedOnHandleCloseType
    {
        [MarshalAs(UnmanagedType.I1)]
        public bool IsFileModified;
    }

    [FieldOffset(0)]
    public PostCreateType PostCreate;
    [FieldOffset(0)]
    public FileRenamedType FileRenamed;
    [FieldOffset(0)]
    public FileDeletedOnHandleCloseType FileDeletedOnHandleClose;
}