using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib;

public unsafe struct PrjPlaceholderInfo
{
    public static uint StructSize { get; } = (uint) Marshal.SizeOf<PrjPlaceholderInfo>();

    public struct EaInformationType
    {
        public uint EaBufferSize;
        public uint OffsetToFirstEa;
    }
    public struct SecurityInformationType
    {
        public uint SecurityBufferSize;
        public uint OffsetToSecurityDescriptor;
    }
    public struct StreamsInformationType
    {
        public uint StreamsInfoBufferSize;
        public uint OffsetToFirstStreamInfo;
    }

    public PrjFileBasicInfo FileBasicInfo;
    public EaInformationType EaInformation;
    public SecurityInformationType SecurityInformation;
    public StreamsInformationType StreamsInformation;
    public PrjPlaceholderVersionInfo VersionInfo;
    public fixed byte VariableData[1];
}