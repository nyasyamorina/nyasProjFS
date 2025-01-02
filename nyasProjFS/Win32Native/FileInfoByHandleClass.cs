namespace nyasProjFS;

internal static partial class Win32Native
{
    internal enum FileInfoByHandleClass : int {
        Basic,
        Standard,
        Name,
        Rename,
        Disposition,
        Allocation,
        EndOfFile,
        Stream,
        Compression,
        AttributeTag,
        IdBothDirectory,
        IdBothDirectoryRestart,
        IoPriorityHint,
        RemoteProtocol,
        FullDirectory,
        FullDirectoryRestart,
        // if (NTDDI_VERSION >= NTDDI_WIN8)
        Storage,
        Alignment,
        Id,
        IdExtdDirectory,
        IdExtdDirectoryRestart,
        // if (NTDDI_VERSION >= NTDDI_WIN10_RS1)
        DispositionEx,
        RenameEx,
        // if (NTDDI_VERSION >= NTDDI_WIN10_19H1)
        CaseSensitive,
        NormalizedName,
        /// <summary>!! this value is determined by os version !!</summary>
        Maximum
    }
}