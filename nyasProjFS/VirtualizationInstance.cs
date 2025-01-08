using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using nyasProjFS.ProjectedFSLib;
using nyasProjFS.ProjectedFSLib.Deprecated;
using static nyasProjFS.ProjectedFSLib.Api;
using static nyasProjFS.ProjectedFSLib.Deprecated.Api;
using static nyasProjFS.CallbackDelegates;
using static nyasProjFS.Win32Native;

namespace nyasProjFS;

using Handle = nint;
using PrjNamespaceVirtualizationContext = nint;
using PrjDirEntryBufferHandle = nint;

/// <summary>
/// Provides methods and callbacks that allow a provider to interact with a virtualization instance.
/// </summary>
/// <remarks>
/// <para>
/// The provider creates one instance of this class for each virtualization root that it manages.
/// The provider uses this class's properties and methods to receive and respond to callbacks from
/// ProjFS for its virtualization instance, and to send commands that control the virtualization
/// instance's state.
/// </para>
/// </remarks>
public class VirtualizationInstance : IVirtualizationInstance
{
    /// <summary>lengthof "\\?\Volume{00000000-0000-0000-0000-000000000000}\"</summary>
    private const uint VolumePathLength = 49;

    private static bool Win32FromHResult(HResult hr, out uint Win32Error)
    {
        if (((uint) hr & 0xFFFF0000) == 0x80070000) {
            Win32Error = (uint) hr & 0x0000FFFF;
            return true;
        }
        Win32Error = 0;
        return hr == HResult.Ok;
    }

    private Win32Exception FailedToMakeVirtualizationRoot(int error, string format)
    {
        return new Win32Exception(error, string.Format(CultureInfo.InvariantCulture, format, _virtualizationRootPath));
    }

    /// <summary>
    /// Initializes an object that manages communication between a provider and ProjFS.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     If <paramref name="virtualizationRootPath"/> doesn't already exist, this constructor
    ///     will create it and mark it as the virtualization root.  The constructor will generate
    ///     a GUID to serve as the virtualization instance ID.
    ///     </para>
    ///     <para>
    ///     If <paramref name="virtualizationRootPath"/> does exist, this constructor will check
    ///     for a ProjFS reparse point.  If the reparse point does not exist, <c>virtualizationRootPath</c>
    ///     will be marked as the virtualization root.  If it has a different reparse point then
    ///     the constructor will throw a <see cref="Win32Exception"/> for
    ///     ERROR_REPARSE_TAG_MISMATCH.
    ///     </para>
    ///     <para>
    ///     For providers that create their virtualization root separately from instantiating the
    ///     <c>VirtualizationInstance</c> class, the static method
    ///     <see cref="MarkDirectoryAsVirtualizationRoot"/> is provided.
    ///     </para>
    /// </remarks>
    /// <param name="virtualizationRootPath">
    ///     The full path to the virtualization root directory.  If this directory does not already
    ///     exist, it will be created.  See the Remarks section for further details.
    /// </param>
    /// <param name="poolThreadCount">
    ///     <para>
    ///     The number of threads the provider will have available to process callbacks from ProjFS.
    ///     </para>
    ///     <para>
    ///     If the provider specifies 0, ProjFS will use a default value of 2 * <paramref name="concurrentThreadCount"/>.
    ///     </para>
    /// </param>
    /// <param name="concurrentThreadCount">
    ///     <para>
    ///     The maximum number of threads the provider wants to run concurrently to process callbacks
    ///     from ProjFS.
    ///     </para>
    ///     <para>
    ///     If the provider specifies 0, ProjFS will use a default value equal to the number of
    ///     CPU cores in the system.
    ///     </para>
    /// </param>
    /// <param name="enableNegativePathCache">
    ///     <para>
    ///     If <c>true</c>, specifies that the virtualization instance should maintain a "negative
    ///     path cache".  If the negative path cache is active, then if the provider indicates
    ///     that a file path does not exist by returning <see cref="HResult.FileNotFound"/> from its
    ///     implementation of <see cref="IRequiredCallbacks.GetPlaceholderInfoCallback"/>, then ProjFS will
    ///     fail subsequent opens of that path without calling <see cref="IRequiredCallbacks.GetPlaceholderInfoCallback"/>
    ///     again.
    ///     </para>
    ///     <para>
    ///     To resume receiving <see cref="IRequiredCallbacks.GetPlaceholderInfoCallback"/> for paths the provider has
    ///     indicated do not exist, the provider must call <see cref="ClearNegativePathCache"/>.
    ///     </para>
    /// </param>
    /// <param name="notificationMappings">
    ///     <para>
    ///     A collection of zero or more <see cref="NotificationMapping"/> objects that describe the
    ///     notifications the provider wishes to receive for the virtualization root.
    ///     </para>
    ///     <para>
    ///     If the collection is empty, ProjFS will send the notifications <see cref="NotificationType.FileOpened"/>,
    ///     <see cref="NotificationType.NewFileCreated"/>, and <see cref="NotificationType.FileOverwritten"/>
    ///     for all files and directories under the virtualization root.
    ///     </para>
    /// </param>
    /// <exception cref="FileLoadException">
    /// The native ProjFS library (ProjectedFSLib.dll) is not available.
    /// </exception>
    /// <exception cref="EntryPointNotFoundException">
    /// An expected entry point cannot be found in ProjectedFSLib.dll.
    /// </exception>
    /// <exception cref="Win32Exception">
    /// An error occurred in setting up the virtualization root.
    /// </exception>
    public VirtualizationInstance(
        string virtualizationRootPath,
        uint poolThreadCount,
        uint concurrentThreadCount,
        bool enableNegativePathCache,
        IReadOnlyCollection<NotificationMapping> notificationMappings
    ) {
        _virtualizationRootPath = virtualizationRootPath;
        _poolThreadCount = poolThreadCount;
        _concurrentThreadCount = concurrentThreadCount;
        _enableNegativePathCache = enableNegativePathCache;
        _notificationMappings = notificationMappings;
        _bytesPerSector = 0;
        _writeBufferAlignmentRequirement = 0;
        _virtualizationContext = 0;

        bool markAsRoot = false;

        DirectoryInfo dirInfo = new(_virtualizationRootPath);
        Guid virtualizationInstanceId = default;
        if (!dirInfo.Exists) {
            virtualizationInstanceId = Guid.NewGuid();
            dirInfo.Create();
            markAsRoot = true;
        }
        else {
            using SmartHandle rootHandle = new(CreateFile(
                _virtualizationRootPath,
                FileReadAttributes, (uint) (FileShare.Write | FileShare.Read),
                0,
                OpenExisting, FileFlagBackupSemantics | FileFlagOpenReparsePoint,
                0
            ));
            if (rootHandle.Handle == -1) {
                throw FailedToMakeVirtualizationRoot(GetLastError(), "failed to open root directory {0}");
            }

            var buffer = new byte[MaximumReparseDataBufferSize];
            unsafe { fixed (byte* bufferPtr = buffer) {
                bool querySuccess = DeviceIoControl(
                    rootHandle.Handle, FsctlGetReparsePoint,
                    0, 0,
                    bufferPtr, MaximumReparseDataBufferSize,
                    null, 0
                );
                if (!querySuccess) {
                    int lastError = GetLastError();
                    if (lastError == ErrorNotAReparsePoint) {
                        virtualizationInstanceId = Guid.NewGuid();
                        markAsRoot = true;
                    }
                    else {
                        throw FailedToMakeVirtualizationRoot(lastError, "failed to query for ProjFS reparse point on {0}");
                    }
                }

                ref ReparseDataBuffer reparseBuffer = ref Unsafe.AsRef<ReparseDataBuffer>(bufferPtr);
                if (!markAsRoot && reparseBuffer.ReparseTag != IoReparseRagProjFS) {
                    throw FailedToMakeVirtualizationRoot(
                        ErrorReparseTagMismatch,
                        "root directory {0} already has a different reparse point."
                    );
                }
            }}
        }

        if (markAsRoot) {
            HResult markResult = MarkDirectoryAsVirtualizationRoot(_virtualizationRootPath, virtualizationInstanceId);
            if (markResult != HResult.Ok) {
                if (!Win32FromHResult(markResult, out uint error)) { error = ErrorInternalError; }
                throw FailedToMakeVirtualizationRoot((int) error, "failed to mark directory {0} as virtualization root");
            }
        }
    }

    #region Callback properties

    /// <summary>
    /// Stores the provider's implementation of <see cref="QueryFileName"/>.
    /// </summary>
    /// <seealso cref="QueryFileName"/>
    /// <remarks>The provider must set this property prior to calling <see cref="StartVirtualizing"/>.</remarks>
    public virtual QueryFileName? OnQueryFileName {
        get => _queryFileNameCallback;
        /// <exception cref="InvalidOperationException">
        /// The provider has already called <see cref="StartVirtualizing"/>.
        /// </exception>
        set { ConfirmNotStarted(); _queryFileNameCallback = value; }
    }

    /// <summary>Stores the provider's implementation of <see cref="CancelCommand"/>.</summary>
    /// <seealso cref="CancelCommand"/>
    /// <remarks>
    /// <para>
    /// If the provider wishes to support asynchronous processing of callbacks (that is, if it
    /// intends to return <see cref="HResult.Pending"/> from any of its callbacks), then the provider
    /// must set this property prior to calling <see cref="StartVirtualizing"/>.
    /// </para>
    /// <para>
    /// If the provider does not wish to support asynchronous processing of callbacks, then it
    /// is not required to provide an implementation of this callback.
    /// </para>
    /// <para>If the provider does implement this callback, then it must set this property prior to
    /// calling <see cref="StartVirtualizing"/>.</para>
    /// </remarks>
    public virtual CancelCommand? OnCancelCommand
    {
        get => _cancelCommandCallback;
        /// <exception cref="InvalidOperationException">
        /// The provider has already called <see cref="StartVirtualizing"/>.
        /// </exception>
        set { ConfirmNotStarted(); _cancelCommandCallback = value; }
    }

    /// <summary>Stores the provider's implementation of <see cref="NotifyFileOpened"/>.</summary>
    /// <seealso cref="NotifyFileOpened"/>
    /// <remarks>
    /// <para>The provider is not required to provide an implementation of this callback.
    /// If it does not provide this callback, the provider will not receive notifications when
    /// a file has been opened.</para>
    /// <para>If the provider does implement this callback, then it must set this property prior to
    /// calling <see cref="StartVirtualizing"/>.</para>
    /// </remarks>
    public virtual NotifyFileOpened? OnNotifyFileOpened
    {
        get => _notifyFileOpenedCallback;
        /// <exception cref="InvalidOperationException">
        /// The provider has already called <see cref="StartVirtualizing"/>.
        /// </exception>
        set { ConfirmNotStarted(); _notifyFileOpenedCallback = value; }
    }

    /// <summary>Stores the provider's implementation of <see cref="NotifyNewFileCreated"/>.</summary>
    /// <seealso cref="NotifyNewFileCreated"/>
    /// <remarks>
    /// <para>The provider is not required to provide an implementation of this callback.
    /// If it does not provide this callback, the provider will not receive notifications when
    /// a new file has been created.</para>
    /// <para>If the provider does implement this callback, then it must set this property prior to
    /// calling <see cref="StartVirtualizing"/>.</para>
    /// </remarks>
    public virtual NotifyNewFileCreated? OnNotifyNewFileCreated
    {
        get => _notifyNewFileCreatedCallback;
        /// <exception cref="InvalidOperationException">
        /// The provider has already called <see cref="StartVirtualizing"/>.
        /// </exception>
        set { ConfirmNotStarted(); _notifyNewFileCreatedCallback = value; }
    }

    /// <summary>Stores the provider's implementation of <see cref="NotifyFileOverwritten"/>.</summary>
    /// <seealso cref="NotifyFileOverwritten"/>
    /// <remarks>
    /// <para>The provider is not required to provide an implementation of this callback.
    /// If it does not provide this callback, the provider will not receive notifications when
    /// a file has been superseded or overwritten.</para>
    /// <para>If the provider does implement this callback, then it must set this property prior to
    /// calling <see cref="StartVirtualizing"/>.</para>
    /// </remarks>
    public virtual NotifyFileOverwritten? OnNotifyFileOverwritten
    {
        get => _notifyFileOverwrittenCallback;
        /// <exception cref="InvalidOperationException">
        /// The provider has already called <see cref="StartVirtualizing"/>.
        /// </exception>
        set { ConfirmNotStarted(); _notifyFileOverwrittenCallback = value; }
    }

    /// <summary>Stores the provider's implementation of <see cref="NotifyPreDelete"/>.</summary>
    /// <seealso cref="NotifyPreDelete"/>
    /// <remarks>
    /// <para>The provider is not required to provide an implementation of this callback.
    /// If it does not provide this callback, the provider will not receive notifications when
    /// a file is about to be deleted.</para>
    /// <para>If the provider does implement this callback, then it must set this property prior to
    /// calling <see cref="StartVirtualizing"/>.</para>
    /// </remarks>
    public virtual NotifyPreDelete? OnNotifyPreDelete
    {
        get => _notifyPreDeleteCallback;
        /// <exception cref="InvalidOperationException">
        /// The provider has already called <see cref="StartVirtualizing"/>.
        /// </exception>
        set { ConfirmNotStarted(); _notifyPreDeleteCallback = value; }
    }

    /// <summary>Stores the provider's implementation of <see cref="NotifyPreRename"/>.</summary>
    /// <seealso cref="NotifyPreRename"/>
    /// <remarks>
    /// <para>The provider is not required to provide an implementation of this callback.
    /// If it does not provide this callback, the provider will not receive notifications when
    /// a file is about to be renamed.</para>
    /// <para>If the provider does implement this callback, then it must set this property prior to
    /// calling <see cref="StartVirtualizing"/>.</para>
    /// </remarks>
    public virtual NotifyPreRename? OnNotifyPreRename
    {
        get => _notifyPreRenameCallback;
        /// <exception cref="InvalidOperationException">
        /// The provider has already called <see cref="StartVirtualizing"/>.
        /// </exception>
        set { ConfirmNotStarted(); _notifyPreRenameCallback = value; }
    }

    /// <summary>Stores the provider's implementation of <see cref="NotifyPreCreateHardlink"/>.</summary>
    /// <seealso cref="NotifyPreCreateHardlink"/>
    /// <remarks>
    /// <para>The provider is not required to provide an implementation of this callback.
    /// If it does not provide this callback, the provider will not receive notifications when
    /// a hard link is about to be created for a file.</para>
    /// <para>If the provider does implement this callback, then it must set this property prior to
    /// calling <see cref="StartVirtualizing"/>.</para>
    /// </remarks>
    public virtual NotifyPreCreateHardlink? OnNotifyPreCreateHardlink
    {
        get => _notifyPreCreateHardlinkCallback;
        /// <exception cref="InvalidOperationException">
        /// The provider has already called <see cref="StartVirtualizing"/>.
        /// </exception>
        set { ConfirmNotStarted(); _notifyPreCreateHardlinkCallback = value; }
    }

    /// <summary>Stores the provider's implementation of <see cref="NotifyFileRenamed"/>.</summary>
    /// <seealso cref="NotifyFileRenamed"/>
    /// <remarks>
    /// <para>The provider is not required to provide an implementation of this callback.
    /// If it does not provide this callback, the provider will not receive notifications when
    /// a file has been renamed.</para>
    /// <para>If the provider does implement this callback, then it must set this property prior to
    /// calling <see cref="StartVirtualizing"/>.</para>
    /// </remarks>
    public virtual NotifyFileRenamed? OnNotifyFileRenamed
    {
        get => _notifyFileRenamedCallback;
        /// <exception cref="InvalidOperationException">
        /// The provider has already called <see cref="StartVirtualizing"/>.
        /// </exception>
        set { ConfirmNotStarted(); _notifyFileRenamedCallback = value; }
    }

    /// <summary>Stores the provider's implementation of <see cref="NotifyHardlinkCreated"/>.</summary>
    /// <seealso cref="NotifyHardlinkCreated"/>
    /// <remarks>
    /// <para>The provider is not required to provide an implementation of this callback.
    /// If it does not provide this callback, the provider will not receive notifications when
    /// a hard link has been created for a file.</para>
    /// <para>If the provider does implement this callback, then it must set this property prior to
    /// calling <see cref="StartVirtualizing"/>.</para>
    /// </remarks>
    public virtual NotifyHardlinkCreated? OnNotifyHardlinkCreated
    {
        get => _notifyHardlinkCreatedCallback;
        /// <exception cref="InvalidOperationException">
        /// The provider has already called <see cref="StartVirtualizing"/>.
        /// </exception>
        set { ConfirmNotStarted(); _notifyHardlinkCreatedCallback = value; }
    }

    /// <summary>Stores the provider's implementation of <see cref="NotifyFileHandleClosedNoModification"/>.</summary>
    /// <seealso cref="NotifyFileHandleClosedNoModification"/>
    /// <remarks>
    /// <para>The provider is not required to provide an implementation of this callback.
    /// If it does not provide this callback, the provider will not receive notifications when
    /// a file handle has been closed and the file has not been modified.</para>
    /// <para>If the provider does implement this callback, then it must set this property prior to
    /// calling <see cref="StartVirtualizing"/>.</para>
    /// </remarks>
    public virtual NotifyFileHandleClosedNoModification? OnNotifyFileHandleClosedNoModification
    {
        get => _notifyFileHandleClosedNoModificationCallback;
        /// <exception cref="InvalidOperationException">
        /// The provider has already called <see cref="StartVirtualizing"/>.
        /// </exception>
        set { ConfirmNotStarted(); _notifyFileHandleClosedNoModificationCallback = value; }
    }

    /// <summary>Stores the provider's implementation of <see cref="NotifyFileHandleClosedFileModifiedOrDeleted"/>.</summary>
    /// <seealso cref="NotifyFileHandleClosedFileModifiedOrDeleted"/>
    /// <remarks>
    /// <para>The provider is not required to provide an implementation of this callback.
    /// If it does not provide this callback, the provider will not receive notifications when
    /// a file handle has been closed on a modified file, or the file was deleted as a result
    /// of closing the handle.</para>
    /// <para>If the provider does implement this callback, then it must set this property prior to
    /// calling <see cref="StartVirtualizing"/>.</para>
    /// </remarks>
    public virtual NotifyFileHandleClosedFileModifiedOrDeleted? OnNotifyFileHandleClosedFileModifiedOrDeleted
    {
        get => _notifyFileHandleClosedFileModifiedOrDeletedCallback;
        /// <exception cref="InvalidOperationException">
        /// The provider has already called <see cref="StartVirtualizing"/>.
        /// </exception>
        set { ConfirmNotStarted(); _notifyFileHandleClosedFileModifiedOrDeletedCallback = value; }
    }

    /// <summary>Stores the provider's implementation of <see cref="NotifyFilePreConvertToFull"/>.</summary>
    /// <seealso cref="NotifyFilePreConvertToFull"/>
    /// <remarks>
    /// <para>The provider is not required to provide an implementation of this callback.
    /// If it does not provide this callback, the provider will not receive notifications when
    /// a file is about to be converted from a placeholder to a full file. </para>
    /// <para>If the provider does implement this callback, then it must set this property prior to
    /// calling <see cref="StartVirtualizing"/>.</para>
    /// </remarks>
    public virtual NotifyFilePreConvertToFull? OnNotifyFilePreConvertToFull
    {
        get => _notifyFilePreConvertToFullCallback;
        /// <exception cref="InvalidOperationException">
        /// The provider has already called <see cref="StartVirtualizing"/>.
        /// </exception>
        set { ConfirmNotStarted(); _notifyFilePreConvertToFullCallback = value; }
    }

    /// <summary>Retrieves the <see cref="IRequiredCallbacks"/> interface.</summary>
    public virtual IRequiredCallbacks? RequiredCallbacks => _requiredCallbacks;

    #endregion
    #region Other properties

    /// <summary>Allows the provider to retrieve the virtualization instance GUID.</summary>
    /// <remarks>
    /// A virtualization instance is identified with a GUID.  If the provider did not generate
    /// and store a GUID itself using the <see cref="MarkDirectoryAsVirtualizationRoot"/> method,
    /// then the VirtualizationInstance class generates one for it.  Either way, the provider
    /// can retrieve the GUID via this property.
    /// </remarks>
    public virtual Guid VirtualizationInstanceId { get { ConfirmStarted(); return _virtualizationInstanceId; } }

    /// <summary>Returns the maximum allowed length of a placeholder's contentID or provider ID.</summary>
    /// <remarks>
    /// See <see cref="WritePlaceholderInfo"/> or <see cref="UpdateFileIfNeeded"/> for more information.
    /// </remarks>
    public virtual int PlaceholderIdLength => (int) PrjPlaceholderId.Length;

    #endregion
    #region Public methods
    #pragma warning disable CS0618

    /// <summary>
    /// Starts the virtualization instance, making it available to service I/O and invoke callbacks
    /// on the provider.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     If the provider has implemented any optional callback delegates, it must set their
    ///     implementations into the <c>On...</c> properties prior to calling this method.
    ///     </para>
    ///     <para>
    ///     On Windows 10 version 1803 this method attempts to determine the sector alignment
    ///     requirements of the underlying storage device and stores that information internally
    ///     in the <see cref="VirtualizationInstance"/> instance.  This information is required
    ///     by the <see cref="CreateWriteBuffer(uint)"/> method to ensure that it can return data in
    ///     the <see cref="WriteFileData"/> method when the original reader is using unbuffered
    ///     I/O.  If this method cannot determine the sector alignment requirements of the
    ///     underlying storage device, it will throw a <see cref="IOException"/>
    ///     exception.
    ///     </para>
    ///     <para>
    ///     On Windows 10 version 1809 and later versions the alignment requirements are determined
    ///     by the system.
    ///     </para>
    /// </remarks>
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
    public virtual HResult StartVirtualizing(IRequiredCallbacks requiredCallbacks)
    {
        if (_virtualizationContextGc != 0) { return HResult.AlreadyInitialized; }

        _requiredCallbacks = requiredCallbacks;
        _virtualizationContextGc = (nint) GCHandle.Alloc(this);

        HResult startResult = HResult.Ok;
        if (ApiHelper.UseBetaApi) {
            FindBytesPerSectorAndAlignment();

            PrjCommandCallbacksInit(PrjCommandCallbacks.StructSize, out PrjCommandCallbacks callbacks);
            callbacks.PrjStartDirectoryEnumeration = DefaultCallbacks.PrjStartDirectoryEnumeration;
            callbacks.PrjEndDirectoryEnumeration   = DefaultCallbacks.PrjEndDirectoryEnumeration;
            callbacks.PrjGetDirectoryEnumeration   = DefaultCallbacks.PrjGetDirectoryEnumeration;
            callbacks.PrjGetPlaceholderInformation = DefaultCallbacks.PrjGetPlaceholderInformation;
            callbacks.PrjGetFileStream             = DefaultCallbacks.PrjGetFileStream;
            if (OnQueryFileName is not null) { callbacks.PrjQueryFileName   = DefaultCallbacks.PrjQueryFileName  ; }
            if (OnCancelCommand is not null) { callbacks.PrjCancelCommand   = DefaultCallbacks.PrjCancelCommand  ; }
            if (AnyNotifyCallback())         { callbacks.PrjNotifyOperation = DefaultCallbacks.PrjNotifyOperation; }

            const uint PrjFlagInstanceNegativePathCache = 2;
            VirtualizationInstExtendedParameters extendedParameters = default;
            extendedParameters.Flags = _enableNegativePathCache ? PrjFlagInstanceNegativePathCache : 0;
            extendedParameters.PoolThreadCount = _poolThreadCount;
            extendedParameters.ConcurrentThreadCount = _concurrentThreadCount;

            if (_notificationMappings.Count > 0) {
                var nativeNotificationMappings = new PrjNotificationMapping[_notificationMappings.Count];

                uint index = 0;
                foreach (NotificationMapping mapping in _notificationMappings) {
                    nativeNotificationMappings[index].NotificationRoot = mapping.NotificationRoot;
                    nativeNotificationMappings[index].NotificationBitMask = mapping.NotificationMask;
                    ++index;
                }
                extendedParameters.NotificationMappings = nativeNotificationMappings;

                VirtualizationInstExtendedParameters? extendedParameters_nullable = extendedParameters;
                startResult = PrjStartVirtualizationInstanceEx(
                    _virtualizationRootPath, in callbacks,
                    _virtualizationContextGc, in extendedParameters_nullable,
                    out _virtualizationContext
                );
            }
            else {
                startResult = PrjStartVirtualizationInstance(
                    _virtualizationRootPath, in callbacks,
                    extendedParameters.Flags, 0, extendedParameters.PoolThreadCount, extendedParameters.ConcurrentThreadCount,
                    _virtualizationContextGc,
                    out _virtualizationContext
                );
            }
        }
        else {
            PrjCallbacks callbacks = default;
            callbacks.StartDirectoryEnumerationCallback = DefaultCallbacks.PrjStartDirectoryEnumeration;
            callbacks.EndDirectoryEnumerationCallback   = DefaultCallbacks.PrjEndDirectoryEnumeration;
            callbacks.GetDirectoryEnumerationCallback   = DefaultCallbacks.PrjGetDirectoryEnumeration;
            callbacks.GetPlaceholderInfoCallback        = DefaultCallbacks.PrjGetPlaceholderInfo;
            callbacks.GetFileDataCallback               = DefaultCallbacks.PrjGetFileData;
            if (OnQueryFileName is not null) { callbacks.QueryFileNameCallback = DefaultCallbacks.PrjQueryFileName; }
            if (OnCancelCommand is not null) { callbacks.CancelCommandCallback = DefaultCallbacks.PrjCancelCommand; }
            if (AnyNotifyCallback())         { callbacks.NotificationCallback = DefaultCallbacks.PrjNotification; }

            PrjStartVirtualizingOptions startOptions = default;
            startOptions.Flags = _enableNegativePathCache ? PrjStartVirtualizingFlags.UseNegativePathCache : PrjStartVirtualizingFlags.None;
            startOptions.PoolThreadCount = _poolThreadCount;
            startOptions.ConcurrentThreadCount = _concurrentThreadCount;

            if (_notificationMappings.Count > 0) {
                var nativeNotificationMappings = new PrjNotificationMapping[_notificationMappings.Count];

                uint index = 0;
                foreach (NotificationMapping mapping in _notificationMappings) {
                    nativeNotificationMappings[index].NotificationRoot = mapping.NotificationRoot;
                    nativeNotificationMappings[index].NotificationBitMask = mapping.NotificationMask;
                    ++index;
                }
                startOptions.NotificationMappings = nativeNotificationMappings;
            }

            PrjStartVirtualizingOptions? startOptions_nallable = startOptions;
            startResult = PrjStartVirtualizing(
                _virtualizationRootPath, in callbacks,
                _virtualizationContextGc, in startOptions_nallable,
                out _virtualizationContext
            );
        }
        if (startResult.IsFailed()) { return startResult; }

        Guid instanceId = default;
        HResult getInfoResult = HResult.Ok;
        if (ApiHelper.UseBetaApi) {
            getInfoResult = PrjGetVirtualizationInstanceIdFromHandle(_virtualizationContext, out instanceId);
        }
        else {
            getInfoResult = PrjGetVirtualizationInstanceInfo(_virtualizationContext, out PrjVirtualizationInstanceInfo instanceInfo);
            instanceId = instanceInfo.InstanceId;
        }
        if (getInfoResult.IsFailed()) {
            StopVirtualizing();
            return getInfoResult;
        }

        _virtualizationInstanceId = instanceId;
        return HResult.Ok;
    }

    /// <summary>
    /// Stops the virtualization instance, making it unavailable to service I/O or invoke callbacks
    /// on the provider.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The virtualization instance is in an invalid state (it may already be stopped).
    /// </exception>
    public virtual void StopVirtualizing()
    {
        HResult hr = HResult.Ok;
        if (ApiHelper.UseBetaApi) {
            hr = PrjStopVirtualizationInstance(_virtualizationContext);
        }
        else {
            try {
                PrjStopVirtualizing(_virtualizationContext);
            }
            catch {
                const uint E_FAIL = 0x80004005;
                hr = unchecked((HResult) E_FAIL);
            }
        }
        if (hr.IsFailed()) { throw new InvalidOperationException("virtualization instance in invalid state"); }

        _virtualizationContext = 0;
        ((GCHandle) _virtualizationContextGc).Free();
        _virtualizationContextGc = 0;
    }

    /// <summary>
    /// Purges the virtualization instance's negative path cache, if it is active.
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
    public virtual HResult ClearNegativePathCache(out uint totalEntryNumber)
    {
        return PrjClearNegativePathCache(_virtualizationContext, out totalEntryNumber);
    }

    /// <summary>
    /// Sends file contents to ProjFS.
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
    public virtual HResult WriteFileData(Guid dataStreamId, IWriteBuffer? buffer, ulong byteOffset, uint length)
    {
        if (buffer is null) { return HResult.InvalidArg; }
        if (ApiHelper.UseBetaApi) {
            return PrjWriteFile(_virtualizationContext, in dataStreamId, buffer.Pointer, byteOffset, length);
        }
        else {
            return PrjWriteFileData(_virtualizationContext, in dataStreamId, buffer.Pointer, byteOffset, length);
        }
    }

    /// <summary>
    /// Enables a provider to delete a file or directory that has been cached on the local file system.
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
    public virtual HResult DeleteFile(string relativePath, UpdateType updateFlags, out UpdateFailureCause failureReason)
    {
        return PrjDeleteFile(_virtualizationContext, relativePath, updateFlags, out failureReason);
    }

    /// <summary>
    /// Sends file or directory metadata to ProjFS.
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
    ///     <para>
    ///     This value must be at most <see cref="PlaceholderIdLength"/> bytes in size.  Any data
    ///     beyond that length will be discarded.
    ///     </para>
    /// </param>
    /// <param name="providerId">
    ///     <para>
    ///     Optional provider-specific data.  ProjFS will pass this value back to the provider
    ///     when requesting file contents in the <see cref="IRequiredCallbacks.GetFileDataCallback"/> callback.  The
    ///     provider may use this value as its own unique identifier, for example as a version number
    ///     for the format of the <paramref name="contentId"/> value.
    ///     </para>
    ///     <para>
    ///     This value must be at most <see cref="PlaceholderIdLength"/> bytes in size.  Any data
    ///     beyond that length will be discarded.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para><see cref="HResult.Ok"/> if the placeholder information was successfully written.</para>
    ///     <para><see cref="HResult.OutOfMemory"/> if a buffer could not be allocated to communicate with ProjFS.</para>
    ///     <para><see cref="HResult.InvalidArg"/> if <paramref name="relativePath"/> is an empty string.</para>
    /// </returns>
    public virtual HResult WritePlaceholderInfo(
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
    ) {
        if (relativePath is null) { return HResult.InvalidArg; }
        if (ApiHelper.UseBetaApi) {
            PrjPlaceholderInformation placeholderInfo = CreatePlaceholderInfomation(
                creationTime, lastAccessTime, lastWriteTime, changeTime, fileAttributes,
                isDirectory ? 0 : endOfFile, isDirectory, contentId, providerId
            );
            return PrjWritePlaceholderInformation(
                _virtualizationContext, relativePath,
                in placeholderInfo, PrjPlaceholderInformation.StructSize
            );
        }
        else {
            PrjPlaceholderInfo placeholderInfo = CreatePlaceholderInfo(
                creationTime, lastAccessTime, lastWriteTime, changeTime, fileAttributes,
                isDirectory ? 0 : endOfFile, isDirectory, contentId, providerId
            );
            return PrjWritePlaceholderInfo(
                _virtualizationContext, relativePath,
                in placeholderInfo, PrjPlaceholderInfo.StructSize
            );
        }
    }

    /// <summary>
    /// Sends file or directory metadata to ProjFS.
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
    /// <param name="symlinkTargetOrNull">
    ///     <para>
    ///     Symlink target for extended info. If its value is null, then this method will have the same behavior as <see cref="WritePlaceholderInfo"/>.
    ///     </para>
    /// </param>
    /// <param name="contentId">
    ///     <para>
    ///     A content identifier, generated by the provider.  ProjFS will pass this value back to the
    ///     provider when requesting file contents in the <see cref="IRequiredCallbacks.GetFileDataCallback"/> callback.
    ///     This allows the provider to distinguish between different versions of the same file, i.e.
    ///     different file contents and/or metadata for the same file path.
    ///     </para>
    ///     <para>
    ///     This value must be at most <see cref="PlaceholderIdLength"/> bytes in size.  Any data
    ///     beyond that length will be discarded.
    ///     </para>
    /// </param>
    /// <param name="providerId">
    ///     <para>
    ///     Optional provider-specific data.  ProjFS will pass this value back to the provider
    ///     when requesting file contents in the <see cref="IRequiredCallbacks.GetFileDataCallback"/> callback.  The
    ///     provider may use this value as its own unique identifier, for example as a version number
    ///     for the format of the <paramref name="contentId"/> value.
    ///     </para>
    ///     <para>
    ///     This value must be at most <see cref="PlaceholderIdLength"/> bytes in size.  Any data
    ///     beyond that length will be discarded.
    ///     </para>
    /// </param>
    /// <returns>
    ///     <para><see cref="HResult.Ok"/> if the placeholder information was successfully written.</para>
    ///     <para><see cref="HResult.OutOfMemory"/> if a buffer could not be allocated to communicate with ProjFS.</para>
    ///     <para><see cref="HResult.InvalidArg"/> if <paramref name="relativePath"/> is an empty string.</para>
    /// </returns>
    public virtual HResult WritePlaceholderInfo2(
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
    ) {
        if (!ApiHelper.HasAdditionalApi) { throw new NotImplementedException("PrjWritePlaceholderInfo2 is not supported in this version of Windows"); }
        if (relativePath is null) { return HResult.InvalidArg; }

        PrjPlaceholderInfo placeholderInfo = CreatePlaceholderInfo(
            creationTime, lastAccessTime, lastWriteTime, changeTime, fileAttributes,
            isDirectory ? 0 : endOfFile, isDirectory, contentId, providerId
        );
        if (symlinkTargetOrNull is null) {
            return PrjWritePlaceholderInfo(
                _virtualizationContext, relativePath,
                in placeholderInfo, PrjPlaceholderInformation.StructSize
            );
        }

        PrjExtendedInfo extendedInfo = default;
        extendedInfo.InfoType = PrjExtInfoType.Symlink;
        extendedInfo.Symlink.TargetName = symlinkTargetOrNull;

        PrjExtendedInfo? extendedInfo_nullable = extendedInfo;
        return PrjWritePlaceholderInfo2(
            _virtualizationContext, relativePath,
            in placeholderInfo, PrjPlaceholderInformation.StructSize,
            in extendedInfo_nullable
        );
    }

    /// <summary>
    /// Updates an item that has been cached on the local file system.
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
    /// <para>
    /// This value must be at most <see cref="PlaceholderIdLength"/> bytes in size.  Any data
    /// beyond that length will be discarded.
    /// </para>
    /// </param>
    /// <param name="providerId">
    /// <para>
    /// Optional provider-specific data.  ProjFS will pass this value back to the provider
    /// when requesting file contents in the <see cref="IRequiredCallbacks.GetFileDataCallback"/> callback.  The
    /// provider may use this value as its own unique identifier, for example as a version number
    /// for the format of the <paramref name="contentId"/> value.
    /// </para>
    /// <para>
    /// This value must be at most <see cref="PlaceholderIdLength"/> bytes in size.  Any data
    /// beyond that length will be discarded.
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
    public virtual HResult UpdateFileIfNeeded(
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
    ) {
        if (ApiHelper.UseBetaApi) {
            PrjPlaceholderInformation placeholderInfo = CreatePlaceholderInfomation(
                creationTime, lastAccessTime, lastWriteTime, changeTime, fileAttributes,
                endOfFile, false, contentId, providerId
            );
            return PrjUpdatePlaceholderIfNeeded(
                _virtualizationContext, relativePath, in placeholderInfo,
                (uint) Marshal.OffsetOf<PrjPlaceholderInformation>(nameof(placeholderInfo.VariableData)),
                updateFlags, out failureReason
            );
        }
        else {
            PrjPlaceholderInfo placeholderInfo = CreatePlaceholderInfo(
                creationTime, lastAccessTime, lastWriteTime, changeTime, fileAttributes,
                endOfFile, false, contentId, providerId
            );
            return PrjUpdateFileIfNeeded(
                _virtualizationContext, relativePath,
                in placeholderInfo, PrjPlaceholderInfo.StructSize,
                updateFlags, out failureReason
            );
        }
    }

    /// <summary>
    /// Signals to ProjFS that the provider has completed processing a callback from which it
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
    public virtual HResult CompleteCommand(int commandId) => CompleteCommand(commandId, HResult.Ok);

    /// <summary>
    /// Signals to ProjFS that the provider has completed processing a callback from which it
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
    public virtual HResult CompleteCommand(int commandId, HResult completionResult)
    {
        unsafe {
            return PrjCompleteCommand(_virtualizationContext, commandId, completionResult, in Unsafe.AsRef<PrjCompleteCommandExtendedParameters>(null));
        }
    }

    /// <summary>
    /// Signals to ProjFS that the provider has completed processing a callback from which it
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
    public virtual HResult CompleteCommand(int commandId, IDirectoryEnumerationResults results)
    {
        PrjCompleteCommandExtendedParameters extendedParameters = default;
        extendedParameters.CommandType = PrjCompleteCommandType.Enumeration;
        extendedParameters.Enumeration.DirEntryBufferHandle = ((DirectoryEnumerationResults) results).DirEntryBufferHandle;
        return PrjCompleteCommand(_virtualizationContext, commandId, HResult.Ok, in extendedParameters);
    }

    /// <summary>
    /// Signals to ProjFS that the provider has completed processing a callback from which it
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
    public virtual HResult CompleteCommand(int commandId, NotificationType newNotificationMask)
    {
        PrjCompleteCommandExtendedParameters extendedParameters = default;
        extendedParameters.CommandType = PrjCompleteCommandType.Notification;
        extendedParameters.Notification.NotificationMask = newNotificationMask;
        return PrjCompleteCommand(_virtualizationContext, commandId, HResult.Ok, in extendedParameters);
    }

    /// <summary>
    /// Creates a <see cref="WriteBuffer"/> for use with <see cref="WriteFileData"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The <see cref="WriteBuffer"/> object ensures that any alignment requirements of the
    ///     underlying storage device are met when writing data with the <see cref="WriteFileData"/>
    ///     method.
    ///     </para>
    ///     <para>
    ///     Note that unlike most methods on <see cref="VirtualizationInstance"/>, this method
    ///     throws rather than return <see cref="HResult"/>.  This makes it convenient to use
    ///     in constructions like a <c>using</c> clause.
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
    public virtual IWriteBuffer CreateWriteBuffer(uint desiredBufferSize)
    {
        if (ApiHelper.UseBetaApi) {
            if (desiredBufferSize < _bytesPerSector) { desiredBufferSize = _bytesPerSector; }
            else {
                uint bufferRemainder = desiredBufferSize % _bytesPerSector;
                if (bufferRemainder != 0) { desiredBufferSize += _bytesPerSector - bufferRemainder; }
            }
            return new WriteBuffer(desiredBufferSize, _writeBufferAlignmentRequirement);
        }
        else {
            return new WriteBuffer(desiredBufferSize, _virtualizationContext);
        }
    }

    /// <summary>
    /// Creates a <see cref="WriteBuffer"/> for use with <see cref="WriteFileData"/>.
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
    ///     <para>
    ///     Note that unlike most methods on <see cref="VirtualizationInstance"/>, this method
    ///     throws rather than return <see cref="HResult"/>.  This makes it convenient to use
    ///     in constructions like a <c>using</c> clause.
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
    /// <exception cref="Win32Exception">
    /// An error occurred retrieving the sector size from ProjFS.
    /// </exception>
    /// <exception cref="OutOfMemoryException">
    /// A buffer could not be allocated.
    /// </exception>
    public virtual IWriteBuffer CreateWriteBuffer(ulong byteOffset, uint length, out ulong alignedByteOffset, out uint alignedLength)
    {
        uint bytesPerSector;
        if (ApiHelper.UseBetaApi) { bytesPerSector = _bytesPerSector; }
        else {
            HResult result = PrjGetVirtualizationInstanceInfo(_virtualizationContext, out PrjVirtualizationInstanceInfo instanceInfo);
            if (result.IsFailed()) {
                if (!Win32FromHResult(result, out uint error)) { error = ErrorInternalError; }
                throw FailedToMakeVirtualizationRoot((int) error, "failed to retrieve virtualization instance info for directory {0}");
            }

            bytesPerSector = instanceInfo.WriteAlignment;
        }

        alignedByteOffset = byteOffset & (0 - (ulong) bytesPerSector);

        ulong rangeEndOffset = byteOffset + length;
        ulong alignedRangeEndOffset = (rangeEndOffset + (bytesPerSector - 1)) & (0 - bytesPerSector);
        alignedLength = (uint) (alignedRangeEndOffset - alignedByteOffset);

        return CreateWriteBuffer(alignedLength);
    }

    /// <summary>
    /// Converts an existing directory to a hydrated directory placeholder.
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
    /// <para>
    /// This value must be at most <see cref="PlaceholderIdLength"/> bytes in size.  Any data
    /// beyond that length will be discarded.
    /// </para>
    /// </param>
    /// <param name="providerId">
    /// <para>
    /// Optional provider-specific data.  The provider may use this value as its own unique identifier,
    /// for example as a version number for the format of the <paramref name="contentId"/> value.
    /// </para>
    /// <para>
    /// This value must be at most <see cref="PlaceholderIdLength"/> bytes in size.  Any data
    /// beyond that length will be discarded.
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
    public virtual HResult MarkDirectoryAsPlaceholder(string targetDirectoryPath, byte[] contentId, byte[] providerId)
    {
        PrjPlaceholderVersionInfo versionInfo = default;
        versionInfo.ContentId = contentId;
        versionInfo.ProviderId = providerId;

        if (ApiHelper.UseBetaApi) {
            HResult result = PrjGetVirtualizationInstanceIdFromHandle(
                _virtualizationContext, out Guid virtualizationInstanceId
            );
            if (result != HResult.Ok) { return result; }
            return PrjConvertDirectoryToPlaceholder(
                _virtualizationRootPath, targetDirectoryPath, in versionInfo, 0, in virtualizationInstanceId
            );
        }
        else {
            HResult result = PrjGetVirtualizationInstanceInfo(
                _virtualizationContext, out PrjVirtualizationInstanceInfo instanceInfo
            );
            if (result.IsFailed()) { return result; }
            return PrjMarkDirectoryAsPlaceholder(
                _virtualizationRootPath, targetDirectoryPath, in versionInfo, in instanceInfo.InstanceId
            );
        }
    }

    /// <summary>
    /// Marks an existing directory as the provider's virtualization root.
    /// </summary>
    /// <remarks>
    /// A provider may wish to designate its virtualization root before it is ready or able to
    /// instantiate the <see cref="VirtualizationInstance"/> class.  In that case it may use this
    /// method to designate the root.  The provider must generate a GUID to identify the virtualization
    /// instance and pass it in <paramref name="virtualizationInstanceGuid"/>.  The
    /// <see cref="VirtualizationInstance"/> constructor will use that
    /// value to identify the provider to ProjFS.
    /// </remarks>
    /// <param name="rootPath">
    /// The full path to the directory to mark as the virtualization root.
    /// </param>
    /// <param name="virtualizationInstanceGuid">
    /// A GUID generated by the provider.  ProjFS uses this value to internally identify the provider.
    /// </param>
    /// <returns>
    /// <para><see cref="HResult.Ok"/> if the conversion succeeded.</para>
    /// <para><see cref="HResult.InvalidArg"/> if <paramref name="rootPath"/> is an empty string.</para>
    /// <para><see cref="HResult.Directory"/> if <paramref name="rootPath"/> does not specify a directory.</para>
    /// <para><see cref="HResult.ReparsePointEncountered"/> if <paramref name="rootPath"/> is already a placeholder or some other kind of reparse point.</para>
    /// <para><see cref="HResult.VirtualizationInvalidOp"/> if <paramref name="rootPath"/> is an ancestor or descendant of an existing virtualization root.</para>
    /// </returns>
    public static HResult MarkDirectoryAsVirtualizationRoot(string rootPath, Guid virtualizationInstanceGuid)
    {
        PrjPlaceholderVersionInfo versionInfo = default;

        if (ApiHelper.UseBetaApi) {
            const uint PrjFlagVirtualizationRoot = 0x0010;

            return PrjConvertDirectoryToPlaceholder(
                rootPath,
                "",
                in versionInfo,
                PrjFlagVirtualizationRoot,
                in virtualizationInstanceGuid
            );
        }
        else {
            return PrjMarkDirectoryAsPlaceholder(
                rootPath,
                null,
                in versionInfo,
                in virtualizationInstanceGuid
            );
        }
    }

    #pragma warning restore CS0618
    #endregion
    #region Private methods

    private void ConfirmStarted()
    {
        if (_virtualizationContext == 0) {
            throw new InvalidOperationException("operation invalid before virtualization instance is started");
        }
    }
    private void ConfirmNotStarted()
    {
        if (_virtualizationContext != 0) {
            throw new InvalidOperationException("operation invalid after virtualization instance is started");
        }
    }
    private void FindBytesPerSectorAndAlignment()
    {
        string exceptionMessage;

        string volumeName;
        unsafe {
            var pathBuffer = new ushort[MaxPath];
            var nameBuffer = new ushort[VolumePathLength + 1];
            fixed (ushort* pathPtr = pathBuffer) fixed (ushort* namePtr = nameBuffer) {

            if (!GetVolumePathName(_virtualizationRootPath, pathPtr, (uint) pathBuffer.Length * sizeof(ushort))) {
                exceptionMessage = string.Format(CultureInfo.InvariantCulture, "failed to get volume path name, error code: {0}", GetLastError());
                goto ThrowIOException;
            }
            bool success = GetVolumeNameForVolumeMountPoint(pathPtr, namePtr, (uint) nameBuffer.Length * sizeof(ushort));
            volumeName = new string((char*) namePtr);
            if (!success) {
                exceptionMessage = string.Format(CultureInfo.InvariantCulture, "failed to get volume name for volume mount point: {0}, error: {1}", volumeName, GetLastError());
                goto ThrowIOException;
            }
            if (volumeName.Length != VolumePathLength || volumeName[(int) VolumePathLength - 1] != '\\') {
                exceptionMessage = string.Format(CultureInfo.InvariantCulture, "volume name {0} is not in expected format", volumeName);
                goto ThrowIOException;
            }
        }}

        Handle rootHandle = CreateFile(
            volumeName,
            0, 0,
            0,
            OpenExisting, FileFlagBackupSemantics,
            0
        );
        if (rootHandle == -1) {
            exceptionMessage = string.Format(CultureInfo.InvariantCulture, "failed to get handle to {0}, error: {1}", _virtualizationRootPath, GetLastError());
            goto ThrowIOException;
        }

        FileStorageInfo storageInfo = default;
        unsafe {
            if (!GetFileInformationByHandleEx(rootHandle, FileStorageInfo.InfoType, &storageInfo, FileStorageInfo.StructSize)) {
                exceptionMessage = string.Format(CultureInfo.InvariantCulture, "failed to query sector size of volume, error: {0}", GetLastError());
                goto CloseHandleAndThrowIOException;
            }
        }

        FileAlignmentInfo alignmentInfo = default;
        unsafe {
            if (!GetFileInformationByHandleEx(rootHandle, FileAlignmentInfo.InfoType, &alignmentInfo, FileAlignmentInfo.StructSize)) {
                exceptionMessage = string.Format(CultureInfo.InvariantCulture, "failed to query device alignment, error: {0}", GetLastError());
                goto CloseHandleAndThrowIOException;
            }
        }

        _bytesPerSector = storageInfo.LogicalBytesPerSector;
        _writeBufferAlignmentRequirement = alignmentInfo.AlignmentRequirement + 1;

        CloseHandle(rootHandle);
        if (!IsPowerOf2(_writeBufferAlignmentRequirement)) {
            exceptionMessage = string.Format("failed to determine write buffer alignment requirement: {0} is not a power of 2", _writeBufferAlignmentRequirement);
            goto ThrowIOException;
        }

        return;

        CloseHandleAndThrowIOException:
            CloseHandle(rootHandle);
        ThrowIOException:
            throw new IOException(exceptionMessage);
    }

    #endregion
    #region Callback property storage

    private IRequiredCallbacks? _requiredCallbacks;

    private QueryFileName? _queryFileNameCallback;
    private CancelCommand? _cancelCommandCallback;

    private NotifyFileOpened?                            _notifyFileOpenedCallback;
    private NotifyNewFileCreated?                        _notifyNewFileCreatedCallback;
    private NotifyFileOverwritten?                       _notifyFileOverwrittenCallback;
    private NotifyPreDelete?                             _notifyPreDeleteCallback;
    private NotifyPreRename?                             _notifyPreRenameCallback;
    private NotifyPreCreateHardlink?                     _notifyPreCreateHardlinkCallback;
    private NotifyFileRenamed?                           _notifyFileRenamedCallback;
    private NotifyHardlinkCreated?                       _notifyHardlinkCreatedCallback;
    private NotifyFileHandleClosedNoModification?        _notifyFileHandleClosedNoModificationCallback;
    private NotifyFileHandleClosedFileModifiedOrDeleted? _notifyFileHandleClosedFileModifiedOrDeletedCallback;
    private NotifyFilePreConvertToFull?                  _notifyFilePreConvertToFullCallback;

    #endregion
    #region Member data variables

    // Values provided by the constructor.
    private string _virtualizationRootPath;
    private uint _poolThreadCount;
    private uint _concurrentThreadCount;
    private bool _enableNegativePathCache;
    private IReadOnlyCollection<NotificationMapping> _notificationMappings;

    // We keep a GC handle to the VirtualizationInstance object to pass through ProjFS as the instance context.
    private nint _virtualizationContextGc = 0;
    //private gcroot<VirtualizationInstance>* _virtualizationContextGc = nullptr;

    // Variables to support aligned I/O in Windows 10 version 1803.
    private uint _bytesPerSector;
    private uint _writeBufferAlignmentRequirement;

    private PrjNamespaceVirtualizationContext _virtualizationContext;
    private Guid _virtualizationInstanceId;

    #endregion

    private static class DefaultCallbacks
    {
        internal static HResult PrjStartDirectoryEnumeration(
            in PrjCallbackData callbackData,
            in Guid enumerationId
        ) {
            if (callbackData.InstanceContext == 0) { return HResult.InternalError; }
            VirtualizationInstance instance = (VirtualizationInstance) ((GCHandle) callbackData.InstanceContext).Target!;

            return instance.RequiredCallbacks!.StartDirectoryEnumerationCallback(
                callbackData.CammandId, enumerationId, callbackData.FilePathName,
                callbackData.TriggeringProccessId, GetTriggeringProcessName(callbackData)
            );
        }

        internal static HResult PrjGetDirectoryEnumeration(
            in PrjCallbackData callbackData,
            in Guid enumerationId,
            string? searchExpression,
            PrjDirEntryBufferHandle dirEntryBufferHandle
        ) {
           if (callbackData.InstanceContext == 0) { return HResult.InternalError; }
            VirtualizationInstance instance = (VirtualizationInstance) ((GCHandle) callbackData.InstanceContext).Target!;

            bool restartScan = (callbackData.Flags & PrjCallbackDataFlags.RestartScan) != 0;
            IDirectoryEnumerationResults enumerationData = new DirectoryEnumerationResults(dirEntryBufferHandle);
            return instance.RequiredCallbacks!.GetDirectoryEnumerationCallback(
                callbackData.CammandId, enumerationId,
                searchExpression ?? "", restartScan, enumerationData
            );
        }

        internal static HResult PrjEndDirectoryEnumeration(
            in PrjCallbackData callbackData,
            in Guid enumerationId
        ) {
            if (callbackData.InstanceContext == 0) { return HResult.InternalError; }
            VirtualizationInstance instance = (VirtualizationInstance) ((GCHandle) callbackData.InstanceContext).Target!;

            return instance.RequiredCallbacks!.EndDirectoryEnumerationCallback(enumerationId);
        }

        internal static HResult PrjGetPlaceholderInfo(
            in PrjCallbackData callbackData
        ) {
            if (callbackData.InstanceContext == 0) { return HResult.InternalError; }
            VirtualizationInstance instance = (VirtualizationInstance) ((GCHandle) callbackData.InstanceContext).Target!;

            return instance.RequiredCallbacks!.GetPlaceholderInfoCallback(
                callbackData.CammandId, callbackData.FilePathName,
                callbackData.TriggeringProccessId, GetTriggeringProcessName(callbackData)
            );
        }

        internal static HResult PrjGetFileData(
            in PrjCallbackData callbackData,
            ulong byteOffset,
            uint length
        ) {
            if (callbackData.InstanceContext == 0) { return HResult.InternalError; }
            VirtualizationInstance instance = (VirtualizationInstance) ((GCHandle) callbackData.InstanceContext).Target!;

            byte[] contentId, providerId;
            if (callbackData.VersionInfo.HasValue) {
                contentId = callbackData.VersionInfo.Value.ContentId;
                providerId = callbackData.VersionInfo.Value.ProviderId;
            }
            else { contentId = providerId = []; }

            return instance.RequiredCallbacks!.GetFileDataCallback(
                callbackData.CammandId, callbackData.FilePathName, byteOffset, length,
                callbackData.DataStreamId, contentId, providerId,
                callbackData.TriggeringProccessId, GetTriggeringProcessName(callbackData)
            );
        }

        internal static HResult PrjQueryFileName(
            in PrjCallbackData callbackData
        ) {
            if (callbackData.InstanceContext == 0) { return HResult.InternalError; }
            VirtualizationInstance instance = (VirtualizationInstance) ((GCHandle) callbackData.InstanceContext).Target!;

            if (instance.OnQueryFileName is null) { return HResult.InternalError; }
            return instance.OnQueryFileName(callbackData.FilePathName);
        }

        internal static HResult PrjNotification(
            in PrjCallbackData callbackData,
            bool isDirectory,
            PrjNotification notification,
            string? destinationFileName,
            ref PrjNotificationParameters notificationParameters
        ) {
            if (callbackData.InstanceContext == 0) { return HResult.InternalError; }
            VirtualizationInstance instance = (VirtualizationInstance) ((GCHandle) callbackData.InstanceContext).Target!;

            HResult result = HResult.Ok;
            switch (notification) {
                case ProjectedFSLib.PrjNotification.FileOpened:
                    if (instance.OnNotifyFileOpened is not null) {
                        if (instance.OnNotifyFileOpened(
                            callbackData.FilePathName, isDirectory,
                            callbackData.TriggeringProccessId, GetTriggeringProcessName(callbackData),
                            out NotificationType notificationType
                        )) { notificationParameters.FileRenamed.NotificationMask = notificationType; }
                        else { result = HResult.AccessDenied; }
                    }
                    break;

                case ProjectedFSLib.PrjNotification.NewFileCreated:
                    if (instance.OnNotifyNewFileCreated is not null) {
                        instance.OnNotifyNewFileCreated(
                            callbackData.FilePathName, isDirectory,
                            callbackData.TriggeringProccessId, GetTriggeringProcessName(callbackData),
                            out NotificationType notificationType
                        );
                        notificationParameters.PostCreate.NotificationMask = notificationType;
                    }
                    break;

                case ProjectedFSLib.PrjNotification.FileOverwritten:
                    if (instance.OnNotifyFileOverwritten is not null) {
                        instance.OnNotifyFileOverwritten(
                            callbackData.FilePathName, isDirectory,
                            callbackData.TriggeringProccessId, GetTriggeringProcessName(callbackData),
                            out NotificationType notificationType
                        );
                        notificationParameters.PostCreate.NotificationMask = notificationType;
                    }
                    break;

                case ProjectedFSLib.PrjNotification.PreDelete:
                    if (!(instance.OnNotifyPreDelete?.Invoke(
                        callbackData.FilePathName, isDirectory,
                        callbackData.TriggeringProccessId, GetTriggeringProcessName(callbackData)
                    ) ?? true)) { result = HResult.AccessDenied; }
                    break;

                case ProjectedFSLib.PrjNotification.PreRename:
                    if (!(instance.OnNotifyPreRename?.Invoke(
                        callbackData.FilePathName, destinationFileName,
                        callbackData.TriggeringProccessId, GetTriggeringProcessName(callbackData)
                    ) ?? true)) { result = HResult.AccessDenied; }
                    break;

                case ProjectedFSLib.PrjNotification.PreSetHardlink:
                    if (!(instance.OnNotifyPreCreateHardlink?.Invoke(
                        callbackData.FilePathName, destinationFileName,
                        callbackData.TriggeringProccessId, GetTriggeringProcessName(callbackData)
                    ) ?? true)) { result = HResult.AccessDenied; }
                    break;

                case ProjectedFSLib.PrjNotification.FileRenamed:
                    if (instance.OnNotifyFileRenamed is not null) {
                        instance.OnNotifyFileRenamed(
                            callbackData.FilePathName, destinationFileName, isDirectory,
                            callbackData.TriggeringProccessId, GetTriggeringProcessName(callbackData),
                            out NotificationType notificationType
                        );
                        notificationParameters.FileRenamed.NotificationMask = notificationType;
                    }
                    break;

                case ProjectedFSLib.PrjNotification.HardlinkCreated:
                    instance.OnNotifyHardlinkCreated?.Invoke(
                        callbackData.FilePathName, destinationFileName,
                        callbackData.TriggeringProccessId, GetTriggeringProcessName(callbackData)
                    );
                    break;

                case ProjectedFSLib.PrjNotification.FileHandleClosedNoModification:
                    instance.OnNotifyFileHandleClosedNoModification?.Invoke(
                        callbackData.FilePathName, isDirectory,
                        callbackData.TriggeringProccessId, GetTriggeringProcessName(callbackData)
                    );
                    break;

                case ProjectedFSLib.PrjNotification.FileHandleClosedFileModified:
                    instance.OnNotifyFileHandleClosedFileModifiedOrDeleted?.Invoke(
                        callbackData.FilePathName, isDirectory,
                        true, false,
                        callbackData.TriggeringProccessId, GetTriggeringProcessName(callbackData)
                    );
                    break;

                case ProjectedFSLib.PrjNotification.FileHandleClosedFileDeleted:
                    instance.OnNotifyFileHandleClosedFileModifiedOrDeleted?.Invoke(
                        callbackData.FilePathName, isDirectory,
                        notificationParameters.FileDeletedOnHandleClose.IsFileModified, true,
                        callbackData.TriggeringProccessId, GetTriggeringProcessName(callbackData)
                    );
                    break;

                case ProjectedFSLib.PrjNotification.FilePreConvertToFull:
                    if (!(instance.OnNotifyFilePreConvertToFull?.Invoke(
                        callbackData.FilePathName,
                        callbackData.TriggeringProccessId, GetTriggeringProcessName(callbackData)
                    ) ?? true)) { result = HResult.AccessDenied; }
                    break;

                default:
                    // Unexpected notification type
                    break;
            }
            return result;
        }

        internal static void PrjCancelCommand(
            in PrjCallbackData callbackData
        ) {
            if (callbackData.InstanceContext == 0) { return; }
            VirtualizationInstance instance = (VirtualizationInstance) ((GCHandle) callbackData.InstanceContext).Target!;

            if (instance.OnCancelCommand is not null) {
                instance.OnCancelCommand(callbackData.CammandId);
            }
        }

        internal static HResult PrjGetPlaceholderInformation(
            in PrjCallbackData callbackData,
            uint desiredAccess,
            uint shareMode,
            uint createDisposition,
            uint createOptions,
            string destinationFileName
        ) => PrjGetPlaceholderInfo(in callbackData);

        internal static HResult PrjGetFileStream(
            in PrjCallbackData callbackData,
            long byteOffset,
            uint length
        ) => PrjGetFileData(in callbackData, (ulong) byteOffset, length);

        internal static HResult PrjNotifyOperation(
            in PrjCallbackData callbackData,
            bool isDirectory,
            PrjNotification notificationType,
            string destinationFileName,
            ref PrjOperationParameters operationParameters
        ) {
            PrjNotificationParameters notificationParameters = default;
            if (notificationType == ProjectedFSLib.PrjNotification.FileHandleClosedFileDeleted) {
                notificationParameters.FileDeletedOnHandleClose.IsFileModified = operationParameters.FileDeletedOnHandleClose.IsFileModified;
            }

            HResult result = PrjNotification(in callbackData, isDirectory, notificationType, destinationFileName, ref notificationParameters);

            switch (notificationType) {
                case ProjectedFSLib.PrjNotification.FileOpened:
                case ProjectedFSLib.PrjNotification.NewFileCreated:
                case ProjectedFSLib.PrjNotification.FileOverwritten:
                    operationParameters.PostCreate.NotificationMask = notificationParameters.PostCreate.NotificationMask;
                    break;

                case ProjectedFSLib.PrjNotification.FileRenamed:
                    operationParameters.FileRenamed.NotificationMask = notificationParameters.FileRenamed.NotificationMask;
                    break;
            }
            return result;
        }
    }

    private bool AnyNotifyCallback()
    {
        return _notifyFileOpenedCallback                            is not null ||
               _notifyNewFileCreatedCallback                        is not null ||
               _notifyFileOverwrittenCallback                       is not null ||
               _notifyPreDeleteCallback                             is not null ||
               _notifyPreRenameCallback                             is not null ||
               _notifyPreCreateHardlinkCallback                     is not null ||
               _notifyFileRenamedCallback                           is not null ||
               _notifyHardlinkCreatedCallback                       is not null ||
               _notifyFileHandleClosedNoModificationCallback        is not null ||
               _notifyFileHandleClosedFileModifiedOrDeletedCallback is not null ||
               _notifyFilePreConvertToFullCallback                  is not null ;

    }

    #region Helper methods

    private static PrjPlaceholderInformation CreatePlaceholderInfomation(
        DateTime creationTime,
        DateTime lastAccessTime,
        DateTime lastWriteTime,
        DateTime changeTime,
        FileAttributes fileAttributes,
        long endOfFile,
        bool directory,
        byte[] contentId,
        byte[] providerId
    ) {
        PrjPlaceholderInformation placeholderInfo = default;

        placeholderInfo.FileBasicInfo.IsDirectory = directory;
        placeholderInfo.FileBasicInfo.FileSize = endOfFile;
        placeholderInfo.FileBasicInfo.CreationTime   = creationTime  .ToFileTime();
        placeholderInfo.FileBasicInfo.LastAccessTime = lastAccessTime.ToFileTime();
        placeholderInfo.FileBasicInfo.LastWriteTime  = lastWriteTime .ToFileTime();
        placeholderInfo.FileBasicInfo.ChangeTime     = changeTime    .ToFileTime();
        placeholderInfo.FileBasicInfo.FileAttributes = (uint) fileAttributes;

        placeholderInfo.EaInformation.EaBufferSize = 0;
        placeholderInfo.EaInformation.OffsetToFirstEa = uint.MaxValue;

        placeholderInfo.SecurityInformation.SecurityBufferSize = 0;
        placeholderInfo.SecurityInformation.OffsetToSecurityDescriptor = uint.MaxValue;

        placeholderInfo.StreamsInformation.SteamsInfoBufferSize = 0;
        placeholderInfo.StreamsInformation.OffsetToFirstStreamInfo = uint.MaxValue;

        placeholderInfo.VersionInfo.ProviderId = providerId;
        placeholderInfo.VersionInfo.ContentId  = contentId;

        return placeholderInfo;
    }
    private static PrjPlaceholderInfo CreatePlaceholderInfo(
        DateTime creationTime,
        DateTime lastAccessTime,
        DateTime lastWriteTime,
        DateTime changeTime,
        FileAttributes fileAttributes,
        long endOfFile,
        bool directory,
        byte[] contentId,
        byte[] providerId
    ) {
        PrjPlaceholderInfo placeholderInfo = default;

        placeholderInfo.FileBasicInfo.IsDirectory = directory;
        placeholderInfo.FileBasicInfo.FileSize = endOfFile;
        placeholderInfo.FileBasicInfo.CreationTime   = creationTime  .ToFileTime();
        placeholderInfo.FileBasicInfo.LastAccessTime = lastAccessTime.ToFileTime();
        placeholderInfo.FileBasicInfo.LastWriteTime  = lastWriteTime .ToFileTime();
        placeholderInfo.FileBasicInfo.ChangeTime     = changeTime    .ToFileTime();
        placeholderInfo.FileBasicInfo.FileAttributes = (uint) fileAttributes;

        placeholderInfo.VersionInfo.ProviderId = providerId;
        placeholderInfo.VersionInfo.ContentId  = contentId;

        return placeholderInfo;
    }

    private static bool IsPowerOf2(uint x) => (x & (x - 1)) == 0;

    private static string GetTriggeringProcessName(PrjCallbackData? callbackData) => callbackData?.TriggeringProccessImageFileName ?? "";

    #endregion
};

#region Support structures

// Support for smart HANDLE
internal class SmartHandle(Handle handle) : IDisposable
{
    public Handle Handle { get; set; } = handle;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing && Handle != -1) {
            CloseHandle(Handle);
            Handle = -1;
        }
    }
}

// #include'ing the header that defines this is tricky, so we just have the definition here.
[StructLayout(LayoutKind.Explicit, Size = 24)]
internal struct ReparseDataBuffer {
    internal unsafe struct SymbolicLinkReparseBufferType
    {
        internal ushort SubstituteNameOffset;
        internal ushort SubstituteNameLength;
        internal ushort PrintNameOffset;
        internal ushort PrintNameLength;
        internal uint Flags;
        internal fixed uint PathBuffer[1];
    }
    internal unsafe struct MountPointReparseBufferType
    {
        internal ushort SubstituteNameOffset;
        internal ushort SubstituteNameLength;
        internal ushort PrintNameOffset;
        internal ushort PrintNameLength;
        internal fixed uint PathBuffer[1];
    }
    internal unsafe struct GenericReparseBufferType
    {
        internal fixed uint PathBuffer[1];
    }

    [FieldOffset(0)]
    internal uint  ReparseTag;
    [FieldOffset(4)]
    internal ushort ReparseDataLength;
    [FieldOffset(6)]
    internal ushort Reserved;
    [FieldOffset(8)]
    internal SymbolicLinkReparseBufferType SymbolicLinkReparseBuffer;
    [FieldOffset(8)]
    internal MountPointReparseBufferType MountPointReparseBuffer;
    [FieldOffset(8)]
    internal GenericReparseBufferType GenericReparseBuffer;
}

#endregion