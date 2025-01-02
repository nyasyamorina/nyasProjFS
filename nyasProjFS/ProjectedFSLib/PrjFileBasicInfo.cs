using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib;

public struct PrjFileBasicInfo
{
    [MarshalAs(UnmanagedType.U1)]
    public bool IsDirectory;
    public long FileSize;
    public LargeInteger CreationTime;
    public LargeInteger LastAccessTime;
    public LargeInteger LastWriteTime;
    public LargeInteger ChangeTime;
    public uint FileAttributes;
}