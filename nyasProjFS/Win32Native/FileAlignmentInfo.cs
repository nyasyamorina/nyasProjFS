using System.Runtime.InteropServices;

namespace nyasProjFS;

internal static partial class Win32Native
{
    internal struct FileAlignmentInfo
    {
        internal static uint StructSize { get; } = (uint) Marshal.SizeOf<FileAlignmentInfo>();
        internal static FileInfoByHandleClass InfoType { get; } = FileInfoByHandleClass.Alignment;

        internal uint AlignmentRequirement;
    }
}