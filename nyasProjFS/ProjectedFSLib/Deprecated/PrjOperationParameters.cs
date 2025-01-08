using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib.Deprecated;

[StructLayout(LayoutKind.Explicit, Size = 24)]
internal struct PrjOperationParameters
{
    internal struct PostCreateType
    {
        internal uint DesiredAccess;
        internal uint ShareMode;
        internal uint CreateDisposition;
        internal uint CreateOptions;
        internal uint IoStatusInformation;
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