using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace nyasProjFS.ProjectedFSLib;

[NativeMarshalling(typeof(PrjPlaceholderVersionInfoMarshaller))]
internal struct PrjPlaceholderVersionInfo
{
    internal byte[] ProviderId;
    internal byte[] ContentId;
}

// ?? `MarshalAs` is not working with `LibraryImport` ??

internal unsafe struct PrjPlaceholderVersionInfoUnmanaged
{
    internal fixed byte ProviderId[PrjPlaceholderId.Length];
    internal fixed byte ContentId[PrjPlaceholderId.Length];
}

[CustomMarshaller(typeof(PrjPlaceholderVersionInfo), MarshalMode.Default, typeof(PrjPlaceholderVersionInfoMarshaller))]
internal static class PrjPlaceholderVersionInfoMarshaller
{
    internal static unsafe PrjPlaceholderVersionInfoUnmanaged ConvertToUnmanaged(in PrjPlaceholderVersionInfo managed)
    {
        PrjPlaceholderVersionInfoUnmanaged unmanaged = default;
        fixed (byte* src = managed.ProviderId) {
            if (src != null) {
                nuint copyLength = nuint.Min(PrjPlaceholderId.Length, (nuint) managed.ProviderId.Length);
                NativeMemory.Copy(src, unmanaged.ProviderId, copyLength);
            }
        }
        fixed (byte* src = managed.ContentId) {
            if (src != null) {
                nuint copyLength = nuint.Min(PrjPlaceholderId.Length, (nuint) managed.ContentId.Length);
                NativeMemory.Copy(src, unmanaged.ContentId, copyLength);
            }
        }
        return unmanaged;
    }

    internal static unsafe PrjPlaceholderVersionInfo ConvertToManaged(in PrjPlaceholderVersionInfoUnmanaged unmanaged)
    {
        PrjPlaceholderVersionInfo managed = default;
        managed.ProviderId = new byte[PrjPlaceholderId.Length];
        fixed (byte* src = unmanaged.ProviderId, dst = managed.ProviderId) {
            NativeMemory.Copy(src, dst, PrjPlaceholderId.Length);
        }
        managed.ContentId = new byte[PrjPlaceholderId.Length];
        fixed (byte* src = unmanaged.ContentId, dst = managed.ContentId) {
            NativeMemory.Copy(src, dst, PrjPlaceholderId.Length);
        }
        return managed;
    }
}