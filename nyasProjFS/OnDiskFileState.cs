namespace nyasProjFS;

/// <summary>Defines values describing the on-disk state of a virtualized file.</summary>
/// <remarks>
/// <para>
/// The <see cref="Tombstone"/> state is used to managed deleted files.  When
/// a directory is enumerated ProjFS merges the set of local items (placeholders, full files,
/// etc.) with the set of virtual items projected by the provider's <c>IRequiredCallbacks::GetDirectoryEnumerationCallback</c>
/// method.  If an item appears in both the local and projected sets, the local item takes precedence.
/// If a file does not exist there is no local state, so it would appear in the enumeration.
/// However if that item had been deleted, having it appear in the enumeration would be unexpected.
/// ProjFS deals with this by replacing a deleted item with a special hidden placeholder called
/// a "tombstone".  This has the following effects:
/// <list type="bullet">
///     <item>
///     <description>Enumerations do not reveal the item.</description>
///     </item>
///     <item>
///     <description>File opens that expect the item to exist fail with e.g. "file not found".</description>
///     </item>
///     <item>
///     <description>File creates that expect to succeed only if the item does not exist succeed;
///         ProjFS removes the tombstone as part of the operation.</description>
///     </item>
/// </list>
/// </para>
/// <para>
/// To illustrate the on-disk states consider the following sequence, given a ProjFS provider
/// that has a single file "foo.txt" located in the virtualization root C:\root.
/// <list type="number">
///     <item>
///     <description>An app enumerates <c>C:\root</c>.  It sees the virtual file "foo.txt".  Since the
///         file has not yet been accessed, the file does not exist on disk.</description>
///     </item>
///     <item>
///     <description>The app opens a handle to <c>C:\root\foo.txt</c>.  ProjFS tells the provider to
///         create a placeholder for it.  The file's state is now <see cref="Placeholder"/></description>
///     </item>
///     <item>
///     <description>The app reads the content of the file.  The provider provides the file
///         content to ProjFS and it is cached to <c>C:\root\foo.txt</c>.  The file's state is
///         now <see cref="Placeholder"/> | <see cref="HydratedPlaceholder"/>.</description>
///     </item>
///     <item>
///     <description>The app updates the Last Modified timestamp.  The file's state is now
///         <see cref="Placeholder"/> | <see cref="HydratedPlaceholder"/> | <see cref="DirtyPlaceholder"/>.</description>
///     </item>
///     <item>
///     <description>The app writes some new data to the file.  <c>C:\root\foo.txt</c>'s state
///         is now <see cref="Full"/>.</description>
///     </item>
///     <item>
///     <description>The app deletes <c>C:\root\foo.txt</c>.  ProjFS replaces the file with a tombstone,
///         so its state is now <see cref="Tombstone"/>.
///         Now when the app enumerates <c>C:\root</c> it does not see foo.txt.  If it tries to open the
///         file, the open fails with <c>HResult.FileNotFound</c>.</description>
///     </item>
/// </list>
/// </para>
/// </remarks>
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