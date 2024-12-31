using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib;

public struct PrjNotificationMapping
{
    public NotificationType NotificationBitMask;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string NotificationRoot;
}