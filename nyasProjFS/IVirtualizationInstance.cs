using static nyasProjFS.CallbackDelegates;

namespace nyasProjFS;

/// <summary>
/// Interface to allow for easier unit testing of a virtualization provider.
/// </summary>
/// <remarks>
/// This class defines the interface implemented by the <c>ProjFS.VirtualizationInstance</c> class.
/// This interface class is provided for use by unit tests to mock up the interface to ProjFS.
/// </remarks>
public interface IVirtualizationInstance
{
    /// <summary>
    /// When overridden in a derived class, stores the provider's implementation of <see cref="QueryFileName"/>.
    /// </summary>
    /// <seealso cref="QueryFileName"/>
    public QueryFileName? OnQueryFileName { get; set; }

    /// <summary>When overridden in a derived class, stores the provider's implementation of <see cref="CancelCommand"/>.</summary>
    /// <seealso cref="CancelCommand"/>
    public CancelCommand? OnCancelCommand { get; set; }

    /// <summary>When overridden in a derived class, stores the provider's implementation of <see cref="NotifyFileOpened"/>.</summary>
    /// <seealso cref="NotifyFileOpened"/>
    public NotifyFileOpened? OnNotifyFileOpened { get; set; }

    /// <summary>When overridden in a derived class, stores the provider's implementation of <see cref="NotifyNewFileCreated"/>.</summary>
    /// <seealso cref="NotifyNewFileCreated"/>
    public NotifyNewFileCreated? OnNotifyNewFileCreated { get; set; }

    /// <summary>When overridden in a derived class, stores the provider's implementation of <see cref="NotifyFileOverwritten"/>.</summary>
    /// <seealso cref="NotifyFileOverwritten"/>
    public NotifyFileOverwritten? OnNotifyFileOverwritten { get; set; }

    /// <summary>When overridden in a derived class, stores the provider's implementation of <see cref="NotifyPreDelete"/>.</summary>
    /// <seealso cref="NotifyPreDelete"/>
    public NotifyPreDelete? OnNotifyPreDelete { get; set; }

    /// <summary>When overridden in a derived class, stores the provider's implementation of <see cref="NotifyPreRename"/>.</summary>
    /// <seealso cref="NotifyPreRename"/>
    public NotifyPreRename? OnNotifyPreRename { get; set; }

    /// <summary>When overridden in a derived class, stores the provider's implementation of <see cref="NotifyPreCreateHardlink"/>.</summary>
    /// <seealso cref="NotifyPreCreateHardlink"/>
    public NotifyPreCreateHardlink? OnNotifyPreCreateHardlink { get; set; }

    /// <summary>When overridden in a derived class, stores the provider's implementation of <see cref="NotifyFileRenamed"/>.</summary>
    /// <seealso cref="NotifyFileRenamed"/>
    public NotifyFileRenamed? OnNotifyFileRenamed { get; set; }

    /// <summary>When overridden in a derived class, stores the provider's implementation of <see cref="NotifyHardlinkCreated"/>.</summary>
    /// <seealso cref="NotifyHardlinkCreated"/>
    public NotifyHardlinkCreated? OnNotifyHardlinkCreated { get; set; }

    /// <summary>When overridden in a derived class, stores the provider's implementation of <see cref="NotifyFileHandleClosedNoModification"/>.</summary>
    /// <seealso cref="NotifyFileHandleClosedNoModification"/>
    public NotifyFileHandleClosedNoModification? OnNotifyFileHandleClosedNoModification { get; set; }

    /// <summary>When overridden in a derived class, stores the provider's implementation of <see cref="NotifyFileHandleClosedFileModifiedOrDeleted"/>.</summary>
    /// <seealso cref="NotifyFileHandleClosedFileModifiedOrDeleted"/>
    public NotifyFileHandleClosedFileModifiedOrDeleted? OnNotifyFileHandleClosedFileModifiedOrDeleted { get; set; }

    /// <summary>When overridden in a derived class, stores the provider's implementation of <see cref="NotifyFilePreConvertToFull"/>.</summary>
    /// <seealso cref="NotifyFilePreConvertToFull"/>
    public NotifyFilePreConvertToFull? OnNotifyFilePreConvertToFull { get; set; }

    /// <summary>
    /// When overridden in a derived class, starts a ProjFS virtualization instance, making it
    /// available to service I/O and invoke callbacks on the provider.
    /// </summary>
    /// <param name="requiredCallbacks">
    ///     <para>
    ///     The provider's implementation of the <see cref="IRequiredCallbacks"/> interface.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para><see cref="HResult.Ok"/> if the virtualization instance started successfully.</para>
    ///     <para><see cref="HResult.OutOfMemory"/> if a buffer could not be allocated to communicate with ProjFS.</para>
    ///     <para><see cref="HResult.VirtualizationInvalidOp"/> if the virtualization root is an ancestor or descendant of an existing virtualization root.</para>
    ///     <para><see cref="HResult.AlreadyInitialized"/> if the virtualization instance is already running.</para>
    /// </returns>
    /// <exception cref="IOException">
    /// The sector alignment requirements of the volume could not be determined.  See the Remarks section.
    /// </exception>
    public HResult StartVirtualizing(IRequiredCallbacks requiredCallbacks);

    /// <summary>
    /// When overridden in a derived class, stops the virtualization instance, making it unavailable
    /// to service I/O or invoke callbacks on the provider.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The virtualization instance is in an invalid state (it may already be stopped).
    /// </exception>
    public void StopVirtualizing();

    /// <summary>
    /// When overridden in a derived class, purges the virtualization instance's negative path
    /// cache, if it is active.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     If the negative path cache is active, then if the provider indicates that a file path does
    ///     not exist by returning <see cref="HResult.FileNotFound"/> from its implementation of
    ///     <see cref="IRequiredCallbacks.GetPlaceholderInfoCallback"/>
    ///     then ProjFS will fail subsequent opens of that path without calling
    ///     <see cref="IRequiredCallbacks.GetPlaceholderInfoCallback"/> again.
    ///     </para>
    ///     <para>
    ///     To resume receiving <see cref="IRequiredCallbacks.GetPlaceholderInfoCallback"/> for
    ///     paths the provider has indicated do not exist, the provider must call this method.
    ///     </para>
    /// </remarks>
    /// <param name="totalEntryNumber">
    /// Returns the number of paths that were in the cache before it was purged.
    /// </param>
    /// <returns>
    ///     <para><see cref="HResult.Ok"/> if the the cache was successfully purged.</para>
    ///     <para><see cref="HResult.OutOfMemory"/> if a buffer could not be allocated to communicate with ProjFS.</para>
    /// </returns>
    public HResult ClearNegativePathCache(out uint totalEntryNumber);

    /// <summary>
    /// When overridden in a derived class, sends file contents to ProjFS.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The provider uses this method to provide the data requested when ProjFS calls the provider's
    ///     implementation of <see cref="IRequiredCallbacks.GetFileDataCallback"/>.
    ///     </para>
    ///     <para>
    ///     The provider calls <see cref="CreateWriteBuffer(uint)"/> to create an instance of <see cref="WriteBuffer"/>
    ///     to contain the data to be written.  The <see cref="WriteBuffer"/> ensures that any alignment
    ///     requirements of the underlying storage device are met.
    ///     </para>
    /// </remarks>
    /// <param name="dataStreamId">
    /// Identifier for the data stream to write to.  The provider must use the value of
    /// <paramref name="dataStreamId"/> passed in its <see cref="IRequiredCallbacks.GetFileDataCallback"/>
    /// callback.
    /// </param>
    /// <param name="buffer">
    /// A <see cref="WriteBuffer"/> created using <see cref="CreateWriteBuffer(uint)"/> that contains
    /// the data to write.
    /// </param>
    /// <param name="byteOffset">
    /// Byte offset from the beginning of the file at which to write the data.
    /// </param>
    /// <param name="length">
    /// The number of bytes to write to the file.
    /// </param>
    /// <returns>
    ///     <para><see cref="HResult.Ok"/> if the data was successfully written.</para>
    ///     <para><see cref="HResult.OutOfMemory"/> if a buffer could not be allocated to communicate with ProjFS.</para>
    ///     <para><see cref="HResult.InvalidArg"/> if <paramref name="buffer"/> is not specified,
    ///     <paramref name="length"/> is 0, or <paramref name="byteOffset"/> is greater than the
    ///     length of the file.</para>
    ///     <para><see cref="HResult.Handle"/> if <paramref name="dataStreamId"/> does not
    ///     correspond to a placeholder that is expecting data to be provided.</para>
    /// </returns>
    /// <seealso cref="WriteBuffer"/>
    public HResult WriteFileData(Guid dataStreamId, IWriteBuffer? buffer, ulong byteOffset, uint length);

    /// <summary>
    /// When overridden in a derived class, enables a provider to delete a file or directory that
    /// has been cached on the local file system.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     If the item is still in the provider's store, deleting it from the local file system changes
    ///     it to a virtual item.
    ///     </para>
    ///     <para>
    ///     This routine will fail if called on a file or directory that is already virtual.
    ///     </para>
    ///     <para>
    ///     If the file or directory to be deleted is in any state other than "placeholder", the
    ///     provider must specify an appropriate combination of <see cref="UpdateType"/> values
    ///     in the <paramref name="updateFlags"/> parameter.  This helps guard against accidental
    ///     loss of data. If the provider did not specify a combination of <see cref="UpdateType"/>
    ///     values in the <paramref name="updateFlags"/> parameter that would allow the delete
    ///     to happen, the method fails with <see cref="HResult.VirtualizationInvalidOp"/>.
    ///     </para>
    ///     <para>
    ///     If a directory contains only tombstones, it may be deleted using this method and
    ///     specifying <see cref="UpdateType.AllowTombstone"/> in <paramref name="updateFlags"/>.
    ///     If the directory contains non-tombstone files, then this method will return <see cref="HResult.DirNotEmpty"/>.
    ///     </para>
    /// </remarks>
    /// <param name="relativePath">
    /// The path, relative to the virtualization root, to the file or directory to delete.
    /// </param>
    /// <param name="updateFlags">
    ///     <para>
    ///     A combination of 0 or more <see cref="UpdateType"/> values to control whether ProjFS
    ///     should allow the delete given the state of the file or directory on disk.  See the documentation
    ///     of <see cref="UpdateType"/> for a description of each flag and what it will allow.
    ///     </para>
    ///     <para>
    ///     If the item is a dirty placeholder, full file, or tombstone, and the provider does not
    ///     specify the appropriate flag(s), this routine will fail to delete the placeholder.
    ///     </para>
    /// </param>
    /// <param name="failureReason">
    /// If this method fails with <see cref="HResult.VirtualizationInvalidOp"/>, this receives a
    /// <see cref="UpdateFailureCause"/> value that describes the reason the delete failed.
    /// </param>
    /// <returns>
    ///     <para><see cref="HResult.Ok"/> if the delete succeeded.</para>
    ///     <para><see cref="HResult.OutOfMemory"/> if a buffer could not be allocated to communicate with ProjFS.</para>
    ///     <para><see cref="HResult.InvalidArg"/> if <paramref name="relativePath"/> is an empty string.</para>
    ///     <para><see cref="HResult.FileNotFound"/> if <paramref name="relativePath"/> cannot
    ///     be found.  It may be for a virtual file or directory.</para>
    ///     <para><see cref="HResult.PathNotFound"/> if <paramref name="relativePath"/> contains
    ///     an intermediate component that cannot be found.  The path may terminate beneath a
    ///     directory that has been replaced with a tombstone.</para>
    ///     <para><see cref="HResult.DirNotEmpty"/> if <paramref name="relativePath"/> is a
    ///     directory and is not empty.</para>
    ///     <para><see cref="HResult.VirtualizationInvalidOp"/> if the input value of <paramref name="updateFlags"/>
    ///     does not allow the delete given the state of the file or directory on disk.  The value
    ///     of <paramref name="failureReason"/> indicates the cause of the failure.</para>
    /// </returns>
    public HResult DeleteFile(string relativePath, UpdateType updateFlags, out UpdateFailureCause failureReason);

    /// <summary>
    /// When overridden in a derived class, sends file or directory metadata to ProjFS.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The provider uses this method to create a placeholder on disk.  It does this when ProjFS
    ///     calls the provider's implementation of <see cref="IRequiredCallbacks.GetPlaceholderInfoCallback"/>,
    ///     or the provider may use this method to proactively lay down a placeholder.
    ///     </para>
    ///     <para>
    ///     Note that the timestamps the provider specifies in the <paramref name="creationTime"/>,
    ///     <paramref name="lastAccessTime"/>, <paramref name="lastWriteTime"/>, and <paramref name="changeTime"/>
    ///     parameters may be any values the provider wishes.  This allows the provider to preserve
    ///     the illusion of files and directories that already exist on the user's system even before they
    ///     are physically created on the user's disk.
    ///     </para>
    /// </remarks>
    /// <param name="relativePath">
    ///     <para>
    ///     The path, relative to the virtualization root, of the file or directory.
    ///     </para>
    ///     <para>
    ///     If the provider is processing a call to <see cref="IRequiredCallbacks.GetPlaceholderInfoCallback"/>,
    ///     then this must be a match to the <c>relativePath</c> value passed in that call.  The
    ///     provider should use the <see cref="Utils.FileNameCompare"/> method to determine whether
    ///     the two names match.
    ///     </para>
    ///     <para>
    ///     For example, if <see cref="IRequiredCallbacks.GetPlaceholderInfoCallback"/> specifies
    ///     <c>dir1\dir1\FILE.TXT</c> in <c>relativePath</c>, and the provider抯 backing store contains
    ///     a file called <c>File.txt</c> in the <c>dir1\dir2</c> directory, and <see cref="Utils.FileNameCompare"/>
    ///     returns 0 when comparing the names <c>FILE.TXT</c> and <c>File.txt</c>, then the provider
    ///     specifies <c>dir1\dir2\File.txt</c> as the value of this parameter.
    ///     </para>
    /// </param>
    /// <param name="creationTime">
    /// The time the file was created.
    /// </param>
    /// <param name="lastAccessTime">
    /// The time the file was last accessed.
    /// </param>
    /// <param name="lastWriteTime">
    /// The time the file was last written to.
    /// </param>
    /// <param name="changeTime">
    /// The time the file was last changed.
    /// </param>
    /// <param name="fileAttributes">
    /// The file attributes.
    /// </param>
    /// <param name="endOfFile">
    /// The size of the file in bytes.
    /// </param>
    /// <param name="isDirectory">
    /// <c>true</c> if <paramref name="relativePath"/> is for a directory, <c>false</c> otherwise.
    /// </param>
    /// <param name="contentId">
    ///     <para>
    ///     A content identifier, generated by the provider.  ProjFS will pass this value back to the
    ///     provider when requesting file contents in the <see cref="IRequiredCallbacks.GetFileDataCallback"/> callback.
    ///     This allows the provider to distinguish between different versions of the same file, i.e.
    ///     different file contents and/or metadata for the same file path.
    ///     </para>
    /// </param>
    /// <param name="providerId">
    ///     <para>
    ///     Optional provider-specific data.  ProjFS will pass this value back to the provider
    ///     when requesting file contents in the <see cref="IRequiredCallbacks.GetFileDataCallback"/> callback.  The
    ///     provider may use this value as its own unique identifier, for example as a version number
    ///     for the format of the <paramref name="contentId"/> value.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para><see cref="HResult.Ok"/> if the placeholder information was successfully written.</para>
    ///     <para><see cref="HResult.OutOfMemory"/> if a buffer could not be allocated to communicate with ProjFS.</para>
    ///     <para><see cref="HResult.InvalidArg"/> if <paramref name="relativePath"/> is an empty string.</para>
    /// </returns>
    public HResult WritePlaceholderInfo(
        string? relativePath,
        DateTime creationTime,
        DateTime lastAccessTime,
        DateTime lastWriteTime,
        DateTime changeTime,
        FileAttributes fileAttributes,
        long endOfFile,
        bool isDirectory,
        byte[] contentId,
        byte[] providerId
    );

    // Additional
    public HResult WritePlaceholderInfo2(
        string? relativePath,
        DateTime creationTime,
        DateTime lastAccessTime,
        DateTime lastWriteTime,
        DateTime changeTime,
        FileAttributes fileAttributes,
        long endOfFile,
        bool isDirectory,
        string? symlinkTargetOrNull,
        byte[] contentId,
        byte[] providerId
    );

    /// <summary>
    /// When overridden in a derived class, updates an item that has been cached on the local
    /// file system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This routine cannot be called on a virtual file or directory.
    /// </para>
    /// <para>
    /// If the file or directory to be updated is in any state other than "placeholder", the provider
    /// must specify an appropriate combination of <see cref="UpdateType"/> values in the
    /// <paramref name="updateFlags"/> parameter.  This helps guard against accidental loss of
    /// data, since upon successful return from this routine the item becomes a placeholder with
    /// the updated metadata.  Any metadata that had been changed since the placeholder was created,
    /// or any file data it contained is discarded.
    /// </para>
    /// <para>
    /// Note that the timestamps the provider specifies in the <paramref name="creationTime"/>,
    /// <paramref name="lastAccessTime"/>, <paramref name="lastWriteTime"/>, and <paramref name="changeTime"/>
    /// parameters may be any values the provider wishes.  This allows the provider to preserve
    /// the illusion of files and directories that already exist on the user's system even before they
    /// are physically created on the user's disk.
    /// </para>
    /// </remarks>
    /// <param name="relativePath">
    /// The path, relative to the virtualization root, to the file or directory to updated.
    /// </param>
    /// <param name="creationTime">
    /// The time the file was created.
    /// </param>
    /// <param name="lastAccessTime">
    /// The time the file was last accessed.
    /// </param>
    /// <param name="lastWriteTime">
    /// The time the file was last written to.
    /// </param>
    /// <param name="changeTime">
    /// The time the file was last changed.
    /// </param>
    /// <param name="fileAttributes">
    /// The file attributes.
    /// </param>
    /// <param name="endOfFile">
    /// The size of the file in bytes.
    /// </param>
    /// <param name="contentId">
    /// <para>
    /// A content identifier, generated by the provider.  ProjFS will pass this value back to the
    /// provider when requesting file contents in the <see cref="IRequiredCallbacks.GetFileDataCallback"/> callback.
    /// This allows the provider to distinguish between different versions of the same file, i.e.
    /// different file contents and/or metadata for the same file path.
    /// </para>
    /// <para>
    /// If this parameter specifies a content identifier that is the same as the content identifier
    /// already on the file or directory, the call succeeds and no update takes place.  Otherwise,
    /// if the call succeeds then the value of this parameter replaces the existing content identifier
    /// on the file or directory.
    /// </para>
    /// </param>
    /// <param name="providerId">
    /// <para>
    /// Optional provider-specific data.  ProjFS will pass this value back to the provider
    /// when requesting file contents in the <see cref="IRequiredCallbacks.GetFileDataCallback"/> callback.  The
    /// provider may use this value as its own unique identifier, for example as a version number
    /// for the format of the <paramref name="contentId"/> value.
    /// </para>
    /// </param>
    /// <param name="updateFlags">
    /// <para>
    /// A combination of 0 or more <see cref="UpdateType"/> values to control whether ProjFS
    /// should allow the update given the state of the file or directory on disk.  See the documentation
    /// of <see cref="UpdateType"/> for a description of each flag and what it will allow.
    /// </para>
    /// <para>
    /// If the item is a dirty placeholder, full file, or tombstone, and the provider does not
    /// specify the appropriate flag(s), this routine will fail to update the item.
    /// </para>
    /// </param>
    /// <param name="failureReason">
    /// If this method fails with <see cref="HResult.VirtualizationInvalidOp"/>, this receives a
    /// <see cref="UpdateFailureCause"/> value that describes the reason the update failed.
    /// </param>
    /// <returns>
    ///     <para><see cref="HResult.Ok"/> if the update succeeded.</para>
    ///     <para><see cref="HResult.OutOfMemory"/> if a buffer could not be allocated to communicate with ProjFS.</para>
    ///     <para><see cref="HResult.InvalidArg"/> if <paramref name="relativePath"/> is an empty string.</para>
    ///     <para><see cref="HResult.FileNotFound"/> if <paramref name="relativePath"/> cannot
    ///     be found.  It may be for a virtual file or directory.</para>
    ///     <para><see cref="HResult.PathNotFound"/> if <paramref name="relativePath"/> contains
    ///     an intermediate component that cannot be found.  The path may terminate beneath a
    ///     directory that has been replaced with a tombstone.</para>
    ///     <para><see cref="HResult.DirNotEmpty"/> if <paramref name="relativePath"/> is a
    ///     directory and is not empty.</para>
    ///     <para><see cref="HResult.VirtualizationInvalidOp"/> if the input value of <paramref name="updateFlags"/>
    ///     does not allow the update given the state of the file or directory on disk.  The value
    ///     of <paramref name="failureReason"/> indicates the cause of the failure.</para>
    /// </returns>
    public HResult UpdateFileIfNeeded(
        string relativePath,
        DateTime creationTime,
        DateTime lastAccessTime,
        DateTime lastWriteTime,
        DateTime changeTime,
        FileAttributes fileAttributes,
        long endOfFile,
        byte[] contentId,
        byte[] providerId,
        UpdateType updateFlags,
        out UpdateFailureCause failureReason
    );

    /// <summary>
    /// When overridden in a derived class, signals to ProjFS that the provider has completed processing a callback from which it
    /// previously returned <see cref="HResult.Pending"/>.
    /// </summary>
    /// <remarks>
    /// If the provider calls this method for the <paramref name="commandId"/> passed by the
    /// <see cref="CancelCommand"/> callback it is not an error, however it is a no-op
    /// because the I/O that caused the callback invocation identified by <paramref name="commandId"/>
    /// has already ended.
    /// </remarks>
    /// <param name="commandId">
    /// A value that uniquely identifies the callback invocation to complete.  This value must be
    /// equal to the value of the <paramref name="commandId"/> parameter of the callback from
    /// which the provider previously returned <see cref="HResult.Pending"/>.
    /// </param>
    /// <returns>
    ///     <para><see cref="HResult.Ok"/> if completion succeeded.</para>
    ///     <para><see cref="HResult.InvalidArg"/> if <paramref name="commandId"/> does not specify a pended callback.</para>
    /// </returns>
    public HResult CompleteCommand(int commandId);

    /// <summary>
    /// When overridden in a derived class, signals to ProjFS that the provider has completed processing a callback from which it
    /// previously returned <see cref="HResult.Pending"/>.
    /// </summary>
    /// <remarks>
    /// If the provider calls this method for the <paramref name="commandId"/> passed by the
    /// <see cref="CancelCommand"/> callback it is not an error, however it is a no-op
    /// because the I/O that caused the callback invocation identified by <paramref name="commandId"/>
    /// has already ended.
    /// </remarks>
    /// <param name="commandId">
    /// A value that uniquely identifies the callback invocation to complete.  This value must be
    /// equal to the value of the <paramref name="commandId"/> parameter of the callback from
    /// which the provider previously returned <see cref="HResult.Pending"/>.
    /// </param>
    /// <param name="completionResult">
    /// The final status of the operation.  See the descriptions for the callback delegates for
    /// appropriate return codes.
    /// </param>
    /// <returns>
    ///     <para><see cref="HResult.Ok"/> if completion succeeded.</para>
    ///     <para><see cref="HResult.InvalidArg"/> if <paramref name="commandId"/> does not specify a pended callback.</para>
    /// </returns>
    public HResult CompleteCommand(int commandId, HResult completionResult);

    /// <summary>
    /// When overridden in a derived class, signals to ProjFS that the provider has completed processing a callback from which it
    /// previously returned <see cref="HResult.Pending"/>.
    /// </summary>
    /// <remarks>
    /// If the provider calls this method for the <paramref name="commandId"/> passed by the
    /// <see cref="CancelCommand"/> callback it is not an error, however it is a no-op
    /// because the I/O that caused the callback invocation identified by <paramref name="commandId"/>
    /// has already ended.
    /// </remarks>
    /// <param name="commandId">
    /// A value that uniquely identifies the callback invocation to complete.  This value must be
    /// equal to the value of the <paramref name="commandId"/> parameter of the callback from
    /// which the provider previously returned <see cref="HResult.Pending"/>.
    /// </param>
    /// <param name="results">
    /// Used when completing <see cref="IRequiredCallbacks.GetDirectoryEnumerationCallback"/>.  Receives
    /// the results of the enumeration.
    /// </param>
    /// <returns>
    ///     <para><see cref="HResult.Ok"/> if completion succeeded.</para>
    ///     <para><see cref="HResult.InvalidArg"/> if <paramref name="commandId"/> does not specify a pended callback or
    ///     if <paramref name="results"/> is not a valid <see cref="IDirectoryEnumerationResults"/>.</para>
    /// </returns>
    public HResult CompleteCommand(int commandId, IDirectoryEnumerationResults results);

    /// <summary>
    /// When overridden in a derived class, signals to ProjFS that the provider has completed processing a callback from which it
    /// previously returned <see cref="HResult.Pending"/>.
    /// </summary>
    /// <remarks>
    /// If the provider calls this method for the <paramref name="commandId"/> passed by the
    /// <see cref="CancelCommand"/> callback it is not an error, however it is a no-op
    /// because the I/O that caused the callback invocation identified by <paramref name="commandId"/>
    /// has already ended.
    /// </remarks>
    /// <param name="commandId">
    /// A value that uniquely identifies the callback invocation to complete.  This value must be
    /// equal to the value of the <paramref name="commandId"/> parameter of the callback from
    /// which the provider previously returned <see cref="HResult.Pending"/>.
    /// </param>
    /// <param name="newNotificationMask">
    /// <para>
    /// Used when completing <c>Microsoft.Windows.ProjFS.Notify*</c> callbacks that have a
    /// <c>notificationMask</c> parameter.  Specifies a bitwise-OR of <see cref="NotificationType"/>
    /// values indicating the set of notifications the provider wishes to receive for this file.
    /// </para>
    /// <para>
    /// If the provider sets this value to 0, it is equivalent to specifying
    /// <see cref="NotificationType.UseExistingMask"/>.
    /// </para>
    /// </param>
    /// <returns>
    ///     <para><see cref="HResult.Ok"/> if completion succeeded.</para>
    ///     <para><see cref="HResult.InvalidArg"/> if <paramref name="commandId"/> does not specify a pended callback
    ///     or if <paramref name="newNotificationMask"/> is not a valid combination of <see cref="NotificationType"/>
    ///     values.</para>
    /// </returns>
    public HResult CompleteCommand(int commandId, NotificationType newNotificationMask);

    /// <summary>
    /// When overridden in a derived class, creates a <see cref="WriteBuffer"/> for use with
    /// <see cref="WriteFileData"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The <see cref="WriteBuffer"/> object ensures that any alignment requirements of the
    ///     underlying storage device are met when writing data with the <see cref="WriteFileData"/>
    ///     method.
    ///     </para>
    /// </remarks>
    /// <param name="desiredBufferSize">
    /// The size in bytes of the buffer required to write the data.
    /// </param>
    /// <returns>
    /// A <see cref="WriteBuffer"/> that provides at least <paramref name="desiredBufferSize"/>
    /// bytes of capacity.
    /// </returns>
    /// <exception cref="OutOfMemoryException">
    /// A buffer could not be allocated.
    /// </exception>
    public IWriteBuffer CreateWriteBuffer(uint desiredBufferSize);

    /// <summary>
    /// When overridden in a derived class, creates a <see cref="WriteBuffer"/> for use with <see cref="WriteFileData"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The <see cref="WriteBuffer"/> object ensures that any alignment requirements of the
    ///     underlying storage device are met when writing data with the <see cref="WriteFileData"/>
    ///     method.
    ///     </para>
    ///     <para>
    ///     This overload allows a provider to get sector-aligned values for the start offset and
    ///     length of the write.  The provider uses <paramref name="alignedByteOffset"/> and
    ///     <paramref name="alignedLength"/> to copy the correct data out of its backing store
    ///     into the <see cref="WriteBuffer"/> and transfer it when calling <see cref="WriteFileData"/>.
    ///     </para>
    /// </remarks>
    /// <param name="byteOffset">
    /// Byte offset from the beginning of the file at which the provider wants to write data.
    /// </param>
    /// <param name="length">
    /// The number of bytes to write to the file.
    /// </param>
    /// <param name="alignedByteOffset">
    /// <paramref name="byteOffset"/>, aligned to the sector size of the storage device.  The
    /// provider uses this value as the <c>byteOffset</c> parameter to <see cref="WriteFileData"/>.
    /// </param>
    /// <param name="alignedLength">
    /// <paramref name="length"/>, aligned to the sector size of the storage device.  The
    /// provider uses this value as the <c>length</c> parameter to <see cref="WriteFileData"/>.
    /// </param>
    /// <returns>
    /// A <see cref="WriteBuffer"/> that provides the needed capacity.
    /// </returns>
    /// <exception cref="System.ComponentModel.Win32Exception">
    /// An error occurred retrieving the sector size from ProjFS.
    /// </exception>
    /// <exception cref="OutOfMemoryException">
    /// A buffer could not be allocated.
    /// </exception>
    public IWriteBuffer CreateWriteBuffer(ulong byteOffset, uint length, out ulong alignedByteOffset, out uint alignedLength);

    /// <summary>
    /// When overridden in a derived class, converts an existing directory to a hydrated directory
    /// placeholder.
    /// </summary>
    /// <remarks>
    /// Children of the directory are not affected.
    /// </remarks>
    /// <param name="targetDirectoryPath">
    /// The full path (i.e. not relative to the virtualization root) to the directory to convert
    /// to a placeholder.
    /// </param>
    /// <param name="contentId">
    /// <para>
    /// A content identifier, generated by the provider. This value is used to distinguish between
    /// different versions of the same file, for example different file contents and/or metadata
    /// (e.g. timestamps) for the same file path.
    /// </para>
    /// </param>
    /// <param name="providerId">
    /// <para>
    /// Optional provider-specific data.  The provider may use this value as its own unique identifier,
    /// for example as a version number for the format of the <paramref name="contentId"/> value.
    /// </para>
    /// </param>
    /// <returns>
    /// <para><see cref="HResult.Ok"/> if the conversion succeeded.</para>
    /// <para><see cref="HResult.OutOfMemory"/> if a buffer could not be allocated to communicate with ProjFS.</para>
    /// <para><see cref="HResult.InvalidArg"/> if <paramref name="targetDirectoryPath"/> is an empty string
    /// or if it is not a directory path.</para>
    /// <para><see cref="HResult.ReparsePointEncountered"/> if <paramref name="targetDirectoryPath"/>
    /// is already a placeholder or some other kind of reparse point.</para>
    /// </returns>
    public HResult MarkDirectoryAsPlaceholder(string targetDirectoryPath, byte[] contentId, byte[] providerId);
}