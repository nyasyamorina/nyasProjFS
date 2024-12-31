using System.Runtime.InteropServices;

namespace nyasProjFS.ProjectedFSLib;

public struct PrjPlaceholderInfo
{
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

    public EaInformationType EaInformation;
    public SecurityInformationType SecurityInformation;
    public StreamsInformationType StreamsInformation;
    public PrjPlaceholderVersionInfo VersionInfo;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
    public byte[] VariableData;
}