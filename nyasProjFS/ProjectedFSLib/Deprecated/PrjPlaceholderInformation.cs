using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace nyasProjFS.ProjectedFSLib.Deprecated;

[NativeMarshalling(typeof(PrjPlaceholderInformationMarshaller))]
internal struct PrjPlaceholderInformation
{
    internal static uint StructSize { get; } = (uint) Marshal.SizeOf<PrjPlaceholderInformationUnmanaged>();

    internal PrjFileBasicInfo FileBasicInfo;
    internal PrjEaInformation EaInformation;
    internal PrjSecurityInformation SecurityInformation;
    internal PrjStreamsInformation StreamsInformation;
    internal PrjPlaceholderVersionInfo VersionInfo;
    internal byte[] VariableData;
}

// ?? `MarshalAs` is not working with `LibraryImport` ??

internal unsafe struct PrjPlaceholderInformationUnmanaged
{
    internal uint Size;
    internal PrjFileBasicInfoUnmanaged FileBasicInfo;
    internal PrjEaInformation EaInformation;
    internal PrjSecurityInformation SecurityInformation;
    internal PrjStreamsInformation StreamsInformation;
    internal PrjPlaceholderVersionInfoUnmanaged VersionInfo;
    internal fixed byte VariableData[1];
}

[CustomMarshaller(typeof(PrjPlaceholderInformation), MarshalMode.ManagedToUnmanagedIn, typeof(PrjPlaceholderInformationMarshaller))]
internal static class PrjPlaceholderInformationMarshaller
{
    internal static unsafe PrjPlaceholderInformationUnmanaged ConvertToUnmanaged(in PrjPlaceholderInformation managed)
    {
        PrjFileBasicInfoUnmanaged fileBasicInfo = PrjFileBasicInfoMarshaller.ConvertToUnmanaged(managed.FileBasicInfo);
        PrjPlaceholderVersionInfoUnmanaged versionInfo = PrjPlaceholderVersionInfoMarshaller.ConvertToUnmanaged(managed.VersionInfo);

        PrjPlaceholderInformationUnmanaged unmanaged = default;
        unmanaged.Size = PrjPlaceholderInformation.StructSize;
        unmanaged.FileBasicInfo = fileBasicInfo;
        unmanaged.EaInformation = managed.EaInformation;
        unmanaged.SecurityInformation = managed.SecurityInformation;
        unmanaged.StreamsInformation = managed.StreamsInformation;
        unmanaged.VersionInfo = versionInfo;
        unmanaged.VariableData[0] = managed.VariableData.ElementAtOrDefault(0);
        return unmanaged;
    }
}