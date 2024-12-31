using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib;

public struct PrjExtendedInfo
{
    public struct SymlinkType
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string TargetName;
    }

    public PrjExtInfoType InfoType;
    public uint NextInfoOffset;
    public SymlinkType Symlink;
}