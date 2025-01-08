using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace nyasProjFS.ProjectedFSLib;

[NativeMarshalling(typeof(PrjStartVirtualizingOptionsMarsheller))]
internal struct PrjStartVirtualizingOptions
{
    internal PrjStartVirtualizingFlags Flags;
    internal uint PoolThreadCount;
    internal uint ConcurrentThreadCount;
    internal PrjNotificationMapping[] NotificationMappings;
}

// ?? `MarshalAs` is not working with `LibraryImport` ??

internal unsafe struct PrjStartVirtualizingOptionsUnmanaged
{
    internal PrjStartVirtualizingFlags Flags;
    internal uint PoolThreadCount;
    internal uint ConcurrentThreadCount;
    internal PrjNotificationMappingUnmanaged* NotificationMappings;
    internal uint NotificationMappingsCount;
}

[CustomMarshaller(typeof(PrjStartVirtualizingOptions), MarshalMode.ManagedToUnmanagedIn, typeof(PrjStartVirtualizingOptionsMarsheller))]
internal static class PrjStartVirtualizingOptionsMarsheller
{
    internal static unsafe PrjStartVirtualizingOptionsUnmanaged ConvertToUnmanaged(in PrjStartVirtualizingOptions managed)
    {
        PrjNotificationMappingUnmanaged* mappings = null;
        if (managed.NotificationMappings.Length > 0) {
            nuint totalBytes = (nuint) managed.NotificationMappings.Length * (nuint) Marshal.SizeOf<PrjStartVirtualizingOptionsUnmanaged>();
            mappings = (PrjNotificationMappingUnmanaged*) NativeMemory.Alloc(totalBytes);

            uint index = 0;
            foreach (PrjNotificationMapping mapping in managed.NotificationMappings) {
                mappings[index] = PrjNotificationMappingMarshaller.ConvertToUnmanaged(mapping);
            }
        }

        PrjStartVirtualizingOptionsUnmanaged unmanaged = default;
        unmanaged.Flags = managed.Flags;
        unmanaged.PoolThreadCount = managed.PoolThreadCount;
        unmanaged.ConcurrentThreadCount = managed.ConcurrentThreadCount;
        unmanaged.NotificationMappings = mappings;
        unmanaged.NotificationMappingsCount = (uint) managed.NotificationMappings.Length;
        return unmanaged;
    }

    internal static unsafe void Free(ref PrjStartVirtualizingOptionsUnmanaged unmanaged)
    {
        PrjNotificationMappingUnmanaged* mappingPtr = unmanaged.NotificationMappings;
        PrjNotificationMappingUnmanaged* mappingsEnd = mappingPtr + unmanaged.NotificationMappingsCount;
        while (mappingPtr < mappingsEnd) {
            PrjNotificationMappingMarshaller.Free(ref *mappingPtr);
            mappingPtr++;
        }

        NativeMemory.Free(unmanaged.NotificationMappings);
        unmanaged.NotificationMappings = null;
    }
}