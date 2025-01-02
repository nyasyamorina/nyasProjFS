namespace nyasProjFS;

/// <summary>Defines values describing when to allow a virtualized file to be deleted or updated.</summary>
/// <remarks>
/// <para>
/// These values are used in the <paramref name="updateFlags"/> input parameter of
/// <c>ProjFS.VirtualizationInstance.UpdateFileIfNeeded</c> and <c>ProjFS.VirtualizationInstance.DeleteFile</c>.
/// The flags control whether ProjFS should allow the update given the state of the file or directory on disk.
/// </para>
/// <para>
/// See the documentation for <c>ProjFS.OnDiskFileState</c> for a description of possible file
/// and directory states in ProjFS.
/// </para>
/// </remarks>
[Flags]
public enum UpdateType : uint {
    None               = 0x00000000,
    /// <summary>ProjFS will allow the update if the item is a placeholder or a dirty placeholder (whether hydrated or not)</summary>
    AllowDirtyMetadata = 0x00000001,
    /// <summary>ProjFS will allow the update if the item is a placeholder or is a full file</summary>
    AllowDirtyData     = 0x00000002,
    /// <summary>ProjFS will allow the update if the item is a placeholder or is a tombstone</summary>
    AllowTombstone     = 0x00000004,
    Reserved1          = 0x00000008,
    Reserved2          = 0x00000010,
    /// <summary>ProjFS will allow the update regardless of whether the DOS read-only bit is set on the item</summary>
    AllowReadOnly      = 0x00000020,
    MaxVal             = AllowReadOnly << 1,
}