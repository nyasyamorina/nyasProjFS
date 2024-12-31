using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib;

using PrjNamespaceVirtualizationContext = nint;

public struct PrjCallbackData
{
    public uint Size;
    public PrjCallbackDataFlags Flags;
    public PrjNamespaceVirtualizationContext NamespaceVirtualizationContext;
    public int CammandId;
    public Guid FileId;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string FilePathName;
    public /* PrjPlaceholderVersionInfo* */ nint VersionInfo;
    public uint TriggeringProccessId;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string TriggeringProccessImageFileName;
    public nint InstanceContext;
}