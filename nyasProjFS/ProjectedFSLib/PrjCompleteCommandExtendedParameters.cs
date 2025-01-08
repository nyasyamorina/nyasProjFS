using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib;

using PrjDirEntryBufferHandle = nint;

[StructLayout(LayoutKind.Explicit, Size = 12)]
internal struct PrjCompleteCommandExtendedParameters
{
    internal struct NotificationType
    {
        internal nyasProjFS.NotificationType NotificationMask;
    }
    internal struct EnumerationType
    {
        internal PrjDirEntryBufferHandle DirEntryBufferHandle;
    }

    [FieldOffset(0)]
    internal PrjCompleteCommandType CommandType;
    [FieldOffset(4)]
    internal NotificationType Notification;
    [FieldOffset(4)]
    internal EnumerationType Enumeration;
}