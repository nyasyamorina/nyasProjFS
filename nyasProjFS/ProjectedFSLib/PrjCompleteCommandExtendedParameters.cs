using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib;

using PrjDirEntryBufferHandle = nint;

[StructLayout(LayoutKind.Explicit, Size = 12)]
public struct PrjCompleteCommandExtendedParameters
{
    public struct NotificationType
    {
        public nyasProjFS.NotificationType NotificationMask;
    }
    public struct EnumerationType
    {
        public PrjDirEntryBufferHandle DirEntryBufferHandle;
    }

    [FieldOffset(0)]
    public PrjCompleteCommandType CommandType;
    [FieldOffset(4)]
    public NotificationType Notification;
    [FieldOffset(4)]
    public EnumerationType Enumeration;
}