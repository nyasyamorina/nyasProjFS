using System.Runtime.InteropServices.Marshalling;

namespace nyasProjFS.ProjectedFSLib;

[NativeMarshalling(typeof(PrjExtendedInfoMarshaller))]
internal struct PrjExtendedInfo
{
    internal struct SymlinkType
    {
        internal string TargetName;
    }

    internal PrjExtInfoType InfoType;
    internal uint NextInfoOffset;
    internal SymlinkType Symlink;
}

// ?? `MarshalAs` is not working with `LibraryImport` ??

internal unsafe struct PrjExtendedInfoUnmanaged
{
    internal struct SymlinkType
    {
        internal ushort* TargetName;
    }

    internal PrjExtInfoType InfoType;
    internal uint NextInfoOffset;
    internal SymlinkType Symlink;
}

[CustomMarshaller(typeof(PrjExtendedInfo), MarshalMode.ManagedToUnmanagedIn, typeof(PrjExtendedInfoMarshaller))]
internal static class PrjExtendedInfoMarshaller
{
    internal static unsafe PrjExtendedInfoUnmanaged ConvertToUnmanaged(in PrjExtendedInfo managed)
    {
        ushort* targetNamePtr = Utf16StringMarshaller.ConvertToUnmanaged(managed.Symlink.TargetName);

        PrjExtendedInfoUnmanaged unmanaged = default;
        unmanaged.InfoType = managed.InfoType;
        unmanaged.NextInfoOffset = managed.NextInfoOffset;
        unmanaged.Symlink.TargetName = targetNamePtr;
        return unmanaged;
    }

    internal static unsafe void Free(ref PrjExtendedInfoUnmanaged unmanaged)
    {
        ushort* targetNamePtr = unmanaged.Symlink.TargetName;
        Utf16StringMarshaller.Free(targetNamePtr);
        unmanaged.Symlink.TargetName = null;
    }
}