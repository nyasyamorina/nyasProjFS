using System.Runtime.InteropServices;
using nyasProjFS.ProjectedFSLib;

namespace nyasProjFS.ProjectedFSLib_Deprecated;

public struct PrjPlaceholderInformation
{
    public uint Size;
    public PrjFileBasicInfo FileBasicInfo;
    public PrjEaInformation EaInformation;
    public PrjSecurityInformation SecurityInformation;
    public PrjStreamsInformation StreamsInformation;
    public PrjPlaceholderVersionInfo VersionInfo;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
    public byte[] VariableData;
}