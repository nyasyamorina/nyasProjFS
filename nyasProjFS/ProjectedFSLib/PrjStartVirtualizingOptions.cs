namespace nyasProjFS.ProjectedFSLib;

public struct PrjStartVirtualizingOptions
{
    public PrjStartVirtualizingFlags Flags;
    public uint PoolThreadCount;
    public uint ConcurrentThreadCount;
    public /* PrjNotificationMapping* */ nint NotificationMappings;
    public uint NotificationMappingsCount;
}