using System.Runtime.InteropServices;
using nyasProjFS.ProjectedFSLib;

namespace nyasProjFS.ProjectedFSLib_Deprecated;

public unsafe struct PrjPlaceholderInformation
{
    public static uint StructSize { get; } = (uint) Marshal.SizeOf<PrjPlaceholderInformation>();

    public uint Size;
    public PrjFileBasicInfo FileBasicInfo;
    public PrjEaInformation EaInformation;
    public PrjSecurityInformation SecurityInformation;
    public PrjStreamsInformation StreamsInformation;
    public PrjPlaceholderVersionInfo VersionInfo;
    public fixed byte VariableData[1];
}