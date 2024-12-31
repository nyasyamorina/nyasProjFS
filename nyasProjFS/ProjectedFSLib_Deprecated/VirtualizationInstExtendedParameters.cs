using nyasProjFS.ProjectedFSLib;

namespace nyasProjFS.ProjectedFSLib_Deprecated;

public struct VirtualizationInstExtendedParameters
{
    public uint Size;
    public uint Flags;
    public uint PoolThreadCount;
    public uint ConcurrentThreadCount;
    public /* PrjNotificationMapping* */ nint NotificationMappings;
    public uint NumNotificationMappingsCount;
}