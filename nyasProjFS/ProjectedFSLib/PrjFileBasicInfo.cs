using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace nyasProjFS.ProjectedFSLib;

[NativeMarshalling(typeof(PrjFileBasicInfoMarshaller))]
internal struct PrjFileBasicInfo
{
    internal bool IsDirectory;
    internal long FileSize;
    internal long CreationTime;
    internal long LastAccessTime;
    internal long LastWriteTime;
    internal long ChangeTime;
    internal uint FileAttributes;
}

// ?? `MarshalAs` is not working with `LibraryImport` ??

[StructLayout(LayoutKind.Explicit, Size = 8)]
internal struct LargeInteger
{
    [FieldOffset(0)]
    internal uint LowPart;
    [FieldOffset(4)]
    internal int HighPart;
    [FieldOffset(0)]
    internal long QuadPart;
}

internal struct PrjFileBasicInfoUnmanaged
{
    internal byte IsDirectory;
    internal long FileSize;
    internal LargeInteger CreationTime;
    internal LargeInteger LastAccessTime;
    internal LargeInteger LastWriteTime;
    internal LargeInteger ChangeTime;
    internal uint FileAttributes;
}

[CustomMarshaller(typeof(PrjFileBasicInfo), MarshalMode.ManagedToUnmanagedIn, typeof(PrjFileBasicInfoMarshaller))]
internal static class PrjFileBasicInfoMarshaller
{
    internal static PrjFileBasicInfoUnmanaged ConvertToUnmanaged(in PrjFileBasicInfo managed)
    {
        PrjFileBasicInfoUnmanaged unmanaged = default;
        unmanaged.IsDirectory = managed.IsDirectory ? byte.MaxValue : (byte) 0;
        unmanaged.FileSize = managed.FileSize;
        unmanaged.CreationTime.QuadPart = managed.CreationTime;
        unmanaged.LastAccessTime.QuadPart = managed.LastAccessTime;
        unmanaged.LastWriteTime.QuadPart = managed.LastWriteTime;
        unmanaged.ChangeTime.QuadPart = managed.ChangeTime;
        unmanaged.FileAttributes = managed.FileAttributes;
        return unmanaged;
    }
}