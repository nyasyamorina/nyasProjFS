namespace nyasProjFS;

[Flags]
public enum OnDiskFileState : uint {
    /// <summary>the item's content (primary data stream) is not present on the disk.  The item's metadata
    /// (name, size, timestamps, attributes, etc.) is cached on the disk</summary>
    Placeholder = 0x1,
    /// <summary>the item's content and metadata have been cached to the disk.  Also referred to as a
    /// "partial file/directory"</summary>
    HydratedPlaceholder = 0x2,
    /// <summary>the item's metadata has been locally modified and is no longer a cache of its state in
    /// the provider's store. Note that creating or deleting a file or directory under a placeholder
    /// directory causes that placeholder directory to become dirty</summary>
    DirtyPlaceholder = 0x4,
    /// <summary>The item's content (primary data stream) has been modified.  The file is no longer a cache
    /// of its state in the provider's store.  Files that have been created on the local file
    /// system (i.e.that do not exist in the provider's store at all) are also considered to be
    /// full files.</summary>
    Full = 0x8,
    /// <summary>A special hidden placeholder that represents an item that has been deleted from the local
    /// file system.</summary>
    Tombstone = 0x10,
}