using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib;

public struct PrjStartVirtualizingOptions
{
    public PrjStartVirtualizingFlags Flags;
    public uint PoolThreadCount;
    public uint ConcurrentThreadCount;
    [MarshalAs(UnmanagedType.LPArray)]
    public PrjNotificationMapping[] NotificationMappings;
    public uint NotificationMappingsCount;
}