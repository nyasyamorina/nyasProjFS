using System.Runtime.InteropServices;
using nyasProjFS.ProjectedFSLib;

namespace nyasProjFS.ProjectedFSLib_Deprecated;

public struct VirtualizationInstExtendedParameters
{
    public static uint StructSize { get; } = (uint) Marshal.SizeOf<VirtualizationInstExtendedParameters>();

    public uint Size;
    public uint Flags;
    public uint PoolThreadCount;
    public uint ConcurrentThreadCount;
    [MarshalAs(UnmanagedType.LPArray)]
    public PrjNotificationMapping[] NotificationMappings;
    public uint NumNotificationMappingsCount;
}