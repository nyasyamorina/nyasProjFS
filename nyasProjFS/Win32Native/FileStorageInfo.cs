using System.Runtime.InteropServices;

namespace nyasProjFS;

internal static partial class Win32Native
{
    internal struct FileStorageInfo
    {
        internal static uint StructSize { get; } = (uint) Marshal.SizeOf<FileStorageInfo>();
        internal static FileInfoByHandleClass InfoType { get; } = FileInfoByHandleClass.Storage;

        internal uint LogicalBytesPerSector;
        internal uint PhysicalBytesPerSectorForAtomicity;
        internal uint PhysicalBytesPerSectorForPerformance;
        internal uint FileSystemEffectivePhysicalBytesPerSectorForAtomicity;
        internal uint Flags;
        internal uint ByteOffsetForSectorAlignment;
        internal uint ByteOffsetForPartitionAlignment;
    }
}