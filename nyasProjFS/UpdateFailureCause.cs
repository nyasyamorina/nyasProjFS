namespace nyasProjFS;

/// <summary>
/// Defines values that describe why an attempt to update or delete a file in a virtualization
/// root has failed.
/// </summary>
/// <remarks>
/// These values are used in the <paramref name="failureReason"/> output parameter of
/// <c>ProjFS.VirtualizationInstance.UpdateFileIfNeeded</c> and <c>ProjFS.VirtualizationInstance.DeleteFile</c>.
/// These are set if the API returns <c>HResult.VirtualizationInvalidOp</c> because the file state
/// does not allow the operation with the <paramref name="updateFlags"/> value(s) passed to the API.
/// </remarks>
[Flags]
public enum UpdateFailureCause : uint {
    /// <summary>the update did not fail</summary>
    NoFailure = 0,
    /// <summary>the item was a dirty placeholder (hydrated or not), and the provider did not specify <c>UpdateType.AllowDirtyMetadata</c></summary>
    DirtyMetadata = 1,
    /// <summary>yhe item was a full file and the provider did not specify <c>UpdateType.AllowDirtyData</c></summary>
    DirtyData = 2,
    /// <summary>the item was a tombstone and the provider did not specify <c>UpdateType.AllowTombstone</c></summary>
    Tombstone = 4,
    /// <summary>the item had the DOS read-only bit set and the provider did not specify <c>UpdateType.AllowReadOnly</c></summary>
    ReadOnly = 8
}