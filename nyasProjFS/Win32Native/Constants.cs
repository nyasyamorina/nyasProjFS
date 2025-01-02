namespace nyasProjFS;

internal static partial class Win32Native
{
    internal const uint FileReadAttributes = 0x0080;
    internal const uint OpenExisting = 0x0003;
    internal const uint FileFlagBackupSemantics  = 0x02000000;
    internal const uint FileFlagOpenReparsePoint = 0x00200000;
    internal const uint FsctlGetReparsePoint = 0x000900A8;
    internal const uint IoReparseRagProjFS = 0x9000001C;

    internal const uint MaximumReparseDataBufferSize = 16 * 1024;
    internal const uint MaxPath = 260;

    internal const int ErrorInternalError = 1359;
    internal const int ErrorNotAReparsePoint = 4390;
    internal const int ErrorReparseTagMismatch = 4394;
}