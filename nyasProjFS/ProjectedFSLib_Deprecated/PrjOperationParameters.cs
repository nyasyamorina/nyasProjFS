using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib_Deprecated;

[StructLayout(LayoutKind.Explicit, Size = 24)]
public struct PrjOperationParameters
{
    public struct PostCreateType
    {
        public uint DesiredAccess;
        public uint ShareMode;
        public uint CreateDisposition;
        public uint CreateOptions;
        public uint IoStatusInformation;
        public uint NotificationMask;
    }
    public struct FileRenamedType
    {
        public uint NotificationMask;
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