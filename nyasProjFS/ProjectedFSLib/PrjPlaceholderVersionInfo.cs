using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib;

public struct PrjPlaceholderVersionInfo
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) PrjPlaceholderId.Length)]
    public byte[] ProviderId;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) PrjPlaceholderId.Length)]
    public byte[] ContentId;
}