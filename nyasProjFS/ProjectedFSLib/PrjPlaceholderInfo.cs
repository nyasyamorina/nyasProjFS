using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace nyasProjFS.ProjectedFSLib;

[NativeMarshalling(typeof(PrjPlaceholderInfoMarshaller))]
internal struct PrjPlaceholderInfo
{
    internal static uint StructSize { get; } = (uint) Marshal.SizeOf<PrjPlaceholderInfoUnmanaged>();

    internal struct EaInformationType
    {
        internal uint EaBufferSize;
        internal uint OffsetToFirstEa;
    }
    internal struct SecurityInformationType
    {
        internal uint SecurityBufferSize;
        internal uint OffsetToSecurityDescriptor;
    }
    internal struct StreamsInformationType
    {
        internal uint StreamsInfoBufferSize;
        internal uint OffsetToFirstStreamInfo;
    }

    internal PrjFileBasicInfo FileBasicInfo;
    internal EaInformationType EaInformation;
    internal SecurityInformationType SecurityInformation;
    internal StreamsInformationType StreamsInformation;
    internal PrjPlaceholderVersionInfo VersionInfo;
    internal byte[] VariableData;
}

// ?? `MarshalAs` is not working with `LibraryImport` ??

internal unsafe struct PrjPlaceholderInfoUnmanaged
{
    internal PrjFileBasicInfoUnmanaged FileBasicInfo;
    internal PrjPlaceholderInfo.EaInformationType EaInformation;
    internal PrjPlaceholderInfo.SecurityInformationType SecurityInformation;
    internal PrjPlaceholderInfo.StreamsInformationType StreamsInformation;
    internal PrjPlaceholderVersionInfoUnmanaged VersionInfo;
    internal fixed byte VariableData[1];
}

[CustomMarshaller(typeof(PrjPlaceholderInfo), MarshalMode.ManagedToUnmanagedIn, typeof(PrjPlaceholderInfoMarshaller))]
internal static class PrjPlaceholderInfoMarshaller
{
    internal static unsafe PrjPlaceholderInfoUnmanaged ConvertToUnmanaged(in PrjPlaceholderInfo managed)
    {
        PrjFileBasicInfoUnmanaged fileBasicInfo = PrjFileBasicInfoMarshaller.ConvertToUnmanaged(managed.FileBasicInfo);
        PrjPlaceholderVersionInfoUnmanaged versionInfo = PrjPlaceholderVersionInfoMarshaller.ConvertToUnmanaged(managed.VersionInfo);

        PrjPlaceholderInfoUnmanaged unmanaged = default;
        unmanaged.FileBasicInfo = fileBasicInfo;
        unmanaged.EaInformation = managed.EaInformation;
        unmanaged.SecurityInformation = managed.SecurityInformation;
        unmanaged.StreamsInformation = managed.StreamsInformation;
        unmanaged.VersionInfo = versionInfo;
        unmanaged.VariableData[0] = managed.VariableData.ElementAtOrDefault(0);
        return unmanaged;
    }
}