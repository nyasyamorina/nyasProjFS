using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace nyasProjFS.ProjectedFSLib.Deprecated;

[NativeMarshalling(typeof(VirtualizationInstExtendedParametersMarshaller))]
internal struct VirtualizationInstExtendedParameters
{
    internal uint Flags { get; set; }
    internal uint PoolThreadCount { get; set; }
    internal uint ConcurrentThreadCount { get; set; }
    internal PrjNotificationMapping[] NotificationMappings { get; set; }
}

// ?? `MarshalAs` is not working with `LibraryImport` ??

internal unsafe struct VirtualizationInstExtendedParametersUnmanaged
{
    internal uint Size;
    internal uint Flags;
    internal uint PoolThreadCount;
    internal uint ConcurrentThreadCount;
    internal PrjNotificationMappingUnmanaged* NotificationMappings;
    internal uint NumNotificationMappingsCount;
}

[CustomMarshaller(typeof(VirtualizationInstExtendedParameters), MarshalMode.ManagedToUnmanagedIn, typeof(VirtualizationInstExtendedParametersMarshaller))]
internal static class VirtualizationInstExtendedParametersMarshaller
{
    internal static unsafe VirtualizationInstExtendedParametersUnmanaged ConvertToUnmanaged(in VirtualizationInstExtendedParameters managed)
    {
        PrjNotificationMappingUnmanaged* mappings = null;
        if (managed.NotificationMappings.Length > 0) {
            nuint totalBytes = (nuint) managed.NotificationMappings.Length * (nuint) Marshal.SizeOf<PrjNotificationMappingUnmanaged>();
            mappings = (PrjNotificationMappingUnmanaged*) NativeMemory.Alloc(totalBytes);

            uint index = 0;
            foreach (PrjNotificationMapping mapping in managed.NotificationMappings) {
                mappings[index] = PrjNotificationMappingMarshaller.ConvertToUnmanaged(mapping);
                index++;
            }
        }

        VirtualizationInstExtendedParametersUnmanaged unmanaged = default;
        unmanaged.Size = (uint) Marshal.SizeOf<VirtualizationInstExtendedParameters>();
        unmanaged.Flags = managed.Flags;
        unmanaged.PoolThreadCount = managed.PoolThreadCount;
        unmanaged.ConcurrentThreadCount = managed.ConcurrentThreadCount;
        unmanaged.NotificationMappings = mappings;
        unmanaged.NumNotificationMappingsCount = (uint) managed.NotificationMappings.Length;
        return unmanaged;
    }

    internal static unsafe void Free(ref VirtualizationInstExtendedParametersUnmanaged unmanaged)
    {
        PrjNotificationMappingUnmanaged* mappingPtr = unmanaged.NotificationMappings;
        PrjNotificationMappingUnmanaged* mappingsEnd = mappingPtr + unmanaged.NumNotificationMappingsCount;
        while (mappingPtr < mappingsEnd) {
            PrjNotificationMappingMarshaller.Free(ref *mappingPtr);
            mappingPtr++;
        }

        NativeMemory.Free(unmanaged.NotificationMappings);
        unmanaged.NotificationMappings = null;
    }
}