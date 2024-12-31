using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib;

public struct PrjFileBasicInfo
{
    [MarshalAs(UnmanagedType.U1)]
    public bool IsDirectory;
    public long FileSize;
    public long CreationTime;
    public long LastAccessTime;
    public long LastWriteTime;
    public long ChangeTime;
    public uint FileAttributes;
}