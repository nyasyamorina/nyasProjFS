using System.Collections.Concurrent;
using nyasProjFS;

namespace SimpleProjFSSample;

/// <summary>
/// This is a simple file system "reflector" provider.  It projects files and directories from
/// a directory called the "layer root" into the virtualization root, also called the "scratch root".
/// </summary>
public class SimpleProvider
{
    public static SimpleProvider? Run(CommandLineOptions opts)
    {
        Logger.Info("Creating provider...");

        SimpleProvider provider;
        try {
            provider = new(opts);
        }
        catch (Exception) {
            Logger.Fatal("Failed to create SimpleProvider.");
            throw;
        }

        Logger.Info("Starting provider...");

        if (!provider.StartVirtualization()) {
            Logger.Error("Could not start provider.");
            return null;
        }

        Logger.Info("Success");
        return provider;
    }

    // These variables hold the layer and scratch paths.
    private readonly string scratchRoot;
    private readonly string layerRoot;

    private readonly VirtualizationInstance virtualizationInstance;
    private readonly ConcurrentDictionary<Guid, ActiveEnumeration> activeEnumerations;

    private NotificationCallbacks notificationCallbacks;

    public CommandLineOptions Options { get; }

    public SimpleProvider(CommandLineOptions options)
    {
        this.scratchRoot = options.VirtRoot;
        this.layerRoot = options.SourceRoot;

        this.Options = options;

        // If in test mode, enable notification callbacks.
        if (this.Options.TestMode) {
            this.Options.EnableNotifications = true;
        }

        // Enable notifications if the user requested them.
        List<NotificationMapping> notificationMappings;
        if (this.Options.EnableNotifications) {
            string rootName = "";
            notificationMappings = [ new(
                NotificationType.FileOpened |
                NotificationType.NewFileCreated |
                NotificationType.FileOverwritten |
                NotificationType.PreDelete |
                NotificationType.PreRename |
                NotificationType.PreCreateHardlink |
                NotificationType.FileRenamed |
                NotificationType.HardlinkCreated |
                NotificationType.FileHandleClosedNoModification |
                NotificationType.FileHandleClosedFileModified |
                NotificationType.FileHandleClosedFileDeleted |
                NotificationType.FilePreConvertToFull,
                rootName
            ) ];
        }
        else {
            notificationMappings = new List<NotificationMapping>();
        }

        try {
            // This will create the virtualization root directory if it doesn't already exist.
            this.virtualizationInstance = new VirtualizationInstance(
                this.scratchRoot,
                poolThreadCount: 0,
                concurrentThreadCount: 0,
                enableNegativePathCache: false,
                notificationMappings: notificationMappings);
        }
        catch (Exception) {
            Logger.Fatal("Failed to create VirtualizationInstance.");
            throw;
        }

        // Set up notifications.
        notificationCallbacks = new(
            this,
            this.virtualizationInstance,
            notificationMappings
        );

        Logger.Info($"Created instance. Layer [{layerRoot}], Scratch [{scratchRoot}]");

        if (this.Options.TestMode) {
            Logger.Info("Provider set in TEST MODE.");
        }

        this.activeEnumerations = new ConcurrentDictionary<Guid, ActiveEnumeration>();
    }

    public bool StartVirtualization()
    {
        // Optional callbacks
        this.virtualizationInstance.OnQueryFileName = QueryFileNameCallback;

        RequiredCallbacks requiredCallbacks = new(this);
        HResult hr = this.virtualizationInstance.StartVirtualizing(requiredCallbacks);
        if (hr != HResult.Ok) {
            Logger.Error($"Failed to start virtualization instance: {hr}");
            return false;
        }

        // If we're running in test mode, signal the test that it may proceed.  If this fails
        // it means we had some problem accessing the shared event that the test set up, so we'll
        // stop the provider.
        if (!SignalIfTestMode("ProviderTestProceed")) {
            this.virtualizationInstance.StopVirtualizing();
            return false;
        }

        return true;
    }

    public void StopVirtualization()
    {
        this.virtualizationInstance.StopVirtualizing();
    }

    private static bool IsEnumerationFilterSet(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter) || filter == "*") {
            return false;
        }
        return true;
    }

    internal bool SignalIfTestMode(string eventName)
    {
        if (this.Options.TestMode) {
            try {
                EventWaitHandle waitHandle = EventWaitHandle.OpenExisting(eventName);
                // Tell the test that it is allowed to proceed.
                waitHandle.Set();
            }
            catch (WaitHandleCannotBeOpenedException ex) {
                Logger.Error($"Test mode specified but wait event does not exist.  Clearing test mode. {Environment.NewLine}{ex}");
                this.Options.TestMode = false;
            }
            catch (UnauthorizedAccessException ex) {
                Logger.Fatal($"Opening event {eventName} {Environment.NewLine}{ex}");
                return false;
            }
            catch (Exception ex) {
                Logger.Fatal($"Opening event {eventName} {Environment.NewLine}{ex}");
                return false;
            }
        }
        return true;
    }

    protected string GetFullPathInLayer(string? relativePath) => Path.Combine(this.layerRoot, relativePath ?? "");

    protected bool DirectoryExistsInLayer(string relativePath)
    {
        string layerPath = this.GetFullPathInLayer(relativePath);
        DirectoryInfo dirInfo = new(layerPath);
        return dirInfo.Exists;
    }

    protected bool FileExistsInLayer(string relativePath)
    {
        string layerPath = this.GetFullPathInLayer(relativePath);
        FileInfo fileInfo = new(layerPath);
        return fileInfo.Exists;
    }

    protected ProjectedFileInfo? GetFileInfoInLayer(string relativePath)
    {
        string layerPath = this.GetFullPathInLayer(relativePath);
        string? layerParentPath = Path.GetDirectoryName(layerPath);
        string layerName = Path.GetFileName(relativePath);

        if (this.FileOrDirectoryExistsInLayer(layerParentPath, layerName, out ProjectedFileInfo? fileInfo)) {
            return fileInfo;
        }
        return null;
    }

    protected IEnumerable<ProjectedFileInfo> GetChildItemsInLayer(string? relativePath)
    {
        string fullPathInLayer = GetFullPathInLayer(relativePath);
        DirectoryInfo dirInfo = new(fullPathInLayer);

        if (!dirInfo.Exists) { yield break; }

        foreach (FileSystemInfo fileSystemInfo in dirInfo.GetFileSystemInfos()) {
            // We only handle files and directories, not symlinks.
            if ((fileSystemInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory) {
                yield return new ProjectedFileInfo(
                    fileSystemInfo.Name,
                    fileSystemInfo.FullName,
                    size: 0,
                    isDirectory: true,
                    creationTime: fileSystemInfo.CreationTime,
                    lastAccessTime: fileSystemInfo.LastAccessTime,
                    lastWriteTime: fileSystemInfo.LastWriteTime,
                    changeTime: fileSystemInfo.LastWriteTime,
                    attributes: fileSystemInfo.Attributes
                );
            }
            else {
                FileInfo fileInfo = (fileSystemInfo as FileInfo)!;
                yield return new ProjectedFileInfo(
                    fileInfo.Name,
                    fileSystemInfo.FullName,
                    size: fileInfo.Length,
                    isDirectory: false,
                    creationTime: fileSystemInfo.CreationTime,
                    lastAccessTime: fileSystemInfo.LastAccessTime,
                    lastWriteTime: fileSystemInfo.LastWriteTime,
                    changeTime: fileSystemInfo.LastWriteTime,
                    attributes: fileSystemInfo.Attributes
                );
            }
        }
    }

    protected HResult HydrateFile(string relativePath, uint bufferSize, Func<byte[], uint, bool> tryWriteBytes)
    {
        string layerPath = this.GetFullPathInLayer(relativePath);
        if (!File.Exists(layerPath)) { return HResult.FileNotFound; }

        // Open the file in the layer for read.
        using FileStream fs = new FileStream(layerPath, FileMode.Open, FileAccess.Read);
        long remainingDataLength = fs.Length;
        byte[] buffer = new byte[bufferSize];

        while (remainingDataLength > 0) {
            // Read from the file into the read buffer.
            int bytesToCopy = (int)Math.Min(remainingDataLength, buffer.Length);
            if (fs.Read(buffer, 0, bytesToCopy) != bytesToCopy) { return HResult.InternalError; }
            // Write the bytes we just read into the scratch.
            if (!tryWriteBytes(buffer, (uint)bytesToCopy)) { return HResult.InternalError; }
            remainingDataLength -= bytesToCopy;
        }
        return HResult.Ok;
    }

    private bool FileOrDirectoryExistsInLayer(string? layerParentPath, string layerName, out ProjectedFileInfo? fileInfo)
    {
        fileInfo = null;

        // Check whether the parent directory exists in the layer.
        DirectoryInfo dirInfo = new(layerParentPath ?? "");
        if (!dirInfo.Exists) { return false; }

        // Get the FileSystemInfo for the entry in the layer that matches the name, using ProjFS's
        // name matching rules.
        FileSystemInfo? fileSystemInfo = dirInfo.GetFileSystemInfos().FirstOrDefault(fsInfo => Utils.IsFileNameMatch(fsInfo.Name, layerName));
        if (fileSystemInfo == null) { return false; }

        bool isDirectory = (fileSystemInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory;

        fileInfo = new(
            name: fileSystemInfo.Name,
            fullName: fileSystemInfo.FullName,
            size: isDirectory ? 0 : new FileInfo(Path.Combine(layerParentPath ?? "", layerName)).Length,
            isDirectory: isDirectory,
            creationTime: fileSystemInfo.CreationTime,
            lastAccessTime: fileSystemInfo.LastAccessTime,
            lastWriteTime: fileSystemInfo.LastWriteTime,
            changeTime: fileSystemInfo.LastWriteTime,
            attributes: fileSystemInfo.Attributes
        );
        return true;
    }

    #region Callback implementations

    // To keep all the callback implementations together we implement the required callbacks in
    // the SimpleProvider class along with the optional QueryFileName callback.  Then we have the
    // IRequiredCallbacks implementation forward the calls to here.

    internal HResult StartDirectoryEnumerationCallback(
        int commandId,
        Guid enumerationId,
        string relativePath,
        uint triggeringProcessId,
        string triggeringProcessImageFileName
    ) {
        Logger.Info($"----> StartDirectoryEnumerationCallback Path [{relativePath}]");

        // Enumerate the corresponding directory in the layer and ensure it is sorted the way
        // ProjFS expects.
        ActiveEnumeration activeEnumeration = new(
            [.. GetChildItemsInLayer(relativePath).OrderBy(file => file.Name, new ProjFSSorter())]
        );

        // Insert the layer enumeration into our dictionary of active enumerations, indexed by
        // enumeration ID.  GetDirectoryEnumerationCallback will be able to find this enumeration
        // given the enumeration ID and return the contents to ProjFS.
        if (!this.activeEnumerations.TryAdd(enumerationId, activeEnumeration)) { return HResult.InternalError; }

        Logger.Info($"<---- StartDirectoryEnumerationCallback {HResult.Ok}");
        return HResult.Ok;
    }

    internal HResult GetDirectoryEnumerationCallback(
        int commandId,
        Guid enumerationId,
        string filterFileName,
        bool restartScan,
        IDirectoryEnumerationResults enumResult
    ) {
        Logger.Info($"----> GetDirectoryEnumerationCallback filterFileName [{filterFileName}]");

        // Find the requested enumeration.  It should have been put there by StartDirectoryEnumeration.
        if (!this.activeEnumerations.TryGetValue(enumerationId, out ActiveEnumeration? enumeration)) {
            Logger.Fatal($"      GetDirectoryEnumerationCallback {HResult.InternalError}");
            return HResult.InternalError;
        }

        if (restartScan) {
            // The caller is restarting the enumeration, so we reset our ActiveEnumeration to the
            // first item that matches filterFileName.  This also saves the value of filterFileName
            // into the ActiveEnumeration, overwriting its previous value.
            enumeration.RestartEnumeration(filterFileName);
        }
        else {
            // The caller is continuing a previous enumeration, or this is the first enumeration
            // so our ActiveEnumeration is already at the beginning.  TrySaveFilterString()
            // will save filterFileName if it hasn't already been saved (only if the enumeration
            // is restarting do we need to re-save filterFileName).
            enumeration.TrySaveFilterString(filterFileName);
        }

        int numEntriesAdded = 0;
        HResult hr = HResult.Ok;

        while (enumeration.IsCurrentValid) {
            ProjectedFileInfo fileInfo = enumeration.Current;

            if (!TryGetTargetIfReparsePoint(fileInfo, fileInfo.FullName, out string? targetPath)) {
                hr = HResult.InternalError;
                break;
            }

            // A provider adds entries to the enumeration buffer until it runs out, or until adding
            // an entry fails. If adding an entry fails, the provider remembers the entry it couldn't
            // add. ProjFS will call the GetDirectoryEnumerationCallback again, and the provider
            // must resume adding entries, starting at the last one it tried to add. SimpleProvider
            // remembers the entry it couldn't add simply by not advancing its ActiveEnumeration.
            if (AddFileInfoToEnum(enumResult, fileInfo, targetPath)) {
                Logger.Debug($"----> GetDirectoryEnumerationCallback Added {fileInfo.Name} {fileInfo.IsDirectory} {targetPath}");
                ++numEntriesAdded;
                enumeration.MoveNext();
            }
            else {
                Logger.Debug($"----> GetDirectoryEnumerationCallback NOT added {fileInfo.Name} {fileInfo.IsDirectory} {targetPath}");
                if (numEntriesAdded == 0) { hr = HResult.InsufficientBuffer; }
                break;
            }
        }

        if (hr == HResult.Ok) {
            Logger.Info($"<---- GetDirectoryEnumerationCallback {hr} [Added entries: {numEntriesAdded}]");
        }
        else {
            Logger.Error($"<---- GetDirectoryEnumerationCallback {hr} [Added entries: {numEntriesAdded}]");
        }
        return hr;
    }

    private bool AddFileInfoToEnum(IDirectoryEnumerationResults enumResult, ProjectedFileInfo fileInfo, string? targetPath)
    {
        if (EnvironmentHelper.IsSymlinkSupportAvailable) {
            return enumResult.Add(
                fileName: fileInfo.Name,
                fileSize: fileInfo.Size,
                isDirectory: fileInfo.IsDirectory,
                fileAttributes: fileInfo.Attributes,
                creationTime: fileInfo.CreationTime,
                lastAccessTime: fileInfo.LastAccessTime,
                lastWriteTime: fileInfo.LastWriteTime,
                changeTime: fileInfo.ChangeTime,
                symlinkTargetOrNull: targetPath
            );
        }
        else {
            return enumResult.Add(
                fileName: fileInfo.Name,
                fileSize: fileInfo.Size,
                isDirectory: fileInfo.IsDirectory,
                fileAttributes: fileInfo.Attributes,
                creationTime: fileInfo.CreationTime,
                lastAccessTime: fileInfo.LastAccessTime,
                lastWriteTime: fileInfo.LastWriteTime,
                changeTime: fileInfo.ChangeTime
            );
        }
    }

    internal HResult EndDirectoryEnumerationCallback(Guid enumerationId)
    {
        Logger.Info("----> EndDirectoryEnumerationCallback");

        if (!this.activeEnumerations.TryRemove(enumerationId, out ActiveEnumeration? enumeration)) {
            return HResult.InternalError;
        }

        Logger.Info($"<---- EndDirectoryEnumerationCallback {HResult.Ok}");
        return HResult.Ok;
    }

    internal HResult GetPlaceholderInfoCallback(
        int commandId,
        string relativePath,
        uint triggeringProcessId,
        string triggeringProcessImageFileName
    ) {
        Logger.Info($"----> GetPlaceholderInfoCallback [{relativePath}]");
        Logger.Info($"  Placeholder creation triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]");

        HResult hr = HResult.Ok;
        ProjectedFileInfo? fileInfo = this.GetFileInfoInLayer(relativePath);
        if (fileInfo == null) { hr = HResult.FileNotFound; }
        else {
            string layerPath = this.GetFullPathInLayer(relativePath);
            if (!TryGetTargetIfReparsePoint(fileInfo, layerPath, out string? targetPath)) {
                hr = HResult.InternalError;
            }
            else {
                hr = WritePlaceholderInfo(relativePath, fileInfo, targetPath);
            }
        }

        Logger.Info($"<---- GetPlaceholderInfoCallback {hr}");
        return hr;
    }

    private HResult WritePlaceholderInfo(string relativePath, ProjectedFileInfo fileInfo, string? targetPath)
    {
        if (EnvironmentHelper.IsSymlinkSupportAvailable) {
            return this.virtualizationInstance.WritePlaceholderInfo2(
                relativePath: Path.Combine(Path.GetDirectoryName(relativePath) ?? "", fileInfo.Name),
                creationTime: fileInfo.CreationTime,
                lastAccessTime: fileInfo.LastAccessTime,
                lastWriteTime: fileInfo.LastWriteTime,
                changeTime: fileInfo.ChangeTime,
                fileAttributes: fileInfo.Attributes,
                endOfFile: fileInfo.Size,
                isDirectory: fileInfo.IsDirectory,
                symlinkTargetOrNull: targetPath,
                contentId: [0],
                providerId: [1]
            );
        }
        else {
            return this.virtualizationInstance.WritePlaceholderInfo(
                relativePath: Path.Combine(Path.GetDirectoryName(relativePath) ?? "", fileInfo.Name),
                creationTime: fileInfo.CreationTime,
                lastAccessTime: fileInfo.LastAccessTime,
                lastWriteTime: fileInfo.LastWriteTime,
                changeTime: fileInfo.ChangeTime,
                fileAttributes: fileInfo.Attributes,
                endOfFile: fileInfo.Size,
                isDirectory: fileInfo.IsDirectory,
                contentId: [0],
                providerId: [1]
            );
        }
    }

    internal HResult GetFileDataCallback(
        int commandId,
        string relativePath,
        ulong byteOffset,
        uint length,
        Guid dataStreamId,
        byte[] contentId,
        byte[] providerId,
        uint triggeringProcessId,
        string triggeringProcessImageFileName
    ) {
        Logger.Info($"----> GetFileDataCallback relativePath [{relativePath}]");
        Logger.Info($"  triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]");

        HResult hr = HResult.Ok;
        if (!this.FileExistsInLayer(relativePath)) { hr = HResult.FileNotFound; }
        else {
            // We'll write the file contents to ProjFS no more than 64KB at a time.
            uint desiredBufferSize = Math.Min(64 * 1024, length);
            try {
                // We could have used VirtualizationInstance.CreateWriteBuffer(uint), but this
                // illustrates how to use its more complex overload.  This method gets us a
                // buffer whose underlying storage is properly aligned for unbuffered I/O.
                using IWriteBuffer writeBuffer = this.virtualizationInstance.CreateWriteBuffer(
                    byteOffset,
                    desiredBufferSize,
                    out ulong alignedWriteOffset,
                    out uint alignedBufferSize
                );
                // Get the file data out of the layer and write it into ProjFS.
                hr = this.HydrateFile(
                    relativePath,
                    alignedBufferSize,
                    (readBuffer, bytesToCopy) =>
                    {
                        // readBuffer contains what HydrateFile() read from the file in the
                        // layer.  Now seek to the beginning of the writeBuffer and copy the
                        // contents of readBuffer into writeBuffer.
                        writeBuffer.Stream.Seek(0, SeekOrigin.Begin);
                        writeBuffer.Stream.Write(readBuffer, 0, (int)bytesToCopy);

                        // Write the data from the writeBuffer into the scratch via ProjFS.
                        HResult writeResult = this.virtualizationInstance.WriteFileData(
                            dataStreamId,
                            writeBuffer,
                            alignedWriteOffset,
                            bytesToCopy
                        );

                        if (writeResult != HResult.Ok) {
                            Logger.Error($"VirtualizationInstance.WriteFileData failed: {writeResult}");
                            return false;
                        }

                        alignedWriteOffset += bytesToCopy;
                        return true;
                    }
                );

                if (hr != HResult.Ok) { return HResult.InternalError; }
            }
            catch (OutOfMemoryException ex) {
                Logger.Error($"Out of memory {Environment.NewLine}{ex}");
                hr = HResult.OutOfMemory;
            }
            catch (Exception ex) {
                Logger.Error($"Exception {Environment.NewLine}{ex}");
                hr = HResult.InternalError;
            }
        }

        Logger.Info($"<---- return status {hr}");
        return hr;
    }

    private HResult QueryFileNameCallback(string relativePath)
    {
        Logger.Info($"----> QueryFileNameCallback relativePath [{relativePath}]");

        HResult hr = HResult.Ok;
        string? parentDirectory = Path.GetDirectoryName(relativePath);
        string childName = Path.GetFileName(relativePath);
        if (this.GetChildItemsInLayer(parentDirectory).Any(child => Utils.IsFileNameMatch(child.Name, childName))) {
            hr = HResult.Ok;
        }
        else {
            hr = HResult.FileNotFound;
        }

        Logger.Info($"<---- QueryFileNameCallback {hr}");
        return hr;
    }

    private bool TryGetTargetIfReparsePoint(ProjectedFileInfo fileInfo, string fullPath, out string? targetPath)
    {
        targetPath = null;

        if ((fileInfo.Attributes & FileAttributes.ReparsePoint) != 0 /* TODO: Check for reparse point type */) {
            if (!FileSystemApi.TryGetReparsePointTarget(fullPath, out targetPath)) { return false; }
            if (Path.IsPathRooted(targetPath)) {
                string targetRelativePath = FileSystemApi.TryGetPathRelativeToRoot(this.layerRoot, targetPath, fileInfo.IsDirectory);
                // GetFullPath is used to get rid of relative path components (such as .\)
                targetPath = Path.GetFullPath(Path.Combine(this.scratchRoot, targetRelativePath));
                return true;
            }
        }
        return true;
    }

    #endregion


    private class RequiredCallbacks : IRequiredCallbacks
    {
        private readonly SimpleProvider provider;

        public RequiredCallbacks(SimpleProvider provider) => this.provider = provider;

        // We implement the callbacks in the SimpleProvider class.

        public HResult StartDirectoryEnumerationCallback(
            int commandId,
            Guid enumerationId,
            string relativePath,
            uint triggeringProcessId,
            string triggeringProcessImageFileName
        ) {
            return this.provider.StartDirectoryEnumerationCallback(
                commandId,
                enumerationId,
                relativePath,
                triggeringProcessId,
                triggeringProcessImageFileName
            );
        }

        public HResult GetDirectoryEnumerationCallback(
            int commandId,
            Guid enumerationId,
            string filterFileName,
            bool restartScan,
            IDirectoryEnumerationResults enumResult
        ) {
            return this.provider.GetDirectoryEnumerationCallback(
                commandId,
                enumerationId,
                filterFileName,
                restartScan,
                enumResult);
        }

        public HResult EndDirectoryEnumerationCallback(Guid enumerationId)
        {
            return this.provider.EndDirectoryEnumerationCallback(enumerationId);
        }

        public HResult GetPlaceholderInfoCallback(
            int commandId,
            string relativePath,
            uint triggeringProcessId,
            string triggeringProcessImageFileName
        ) {
            return this.provider.GetPlaceholderInfoCallback(
                commandId,
                relativePath,
                triggeringProcessId,
                triggeringProcessImageFileName);
        }

        public HResult GetFileDataCallback(
            int commandId,
            string relativePath,
            ulong byteOffset,
            uint length,
            Guid dataStreamId,
            byte[] contentId,
            byte[] providerId,
            uint triggeringProcessId,
            string triggeringProcessImageFileName
        ) {
            return this.provider.GetFileDataCallback(
                commandId,
                relativePath,
                byteOffset,
                length,
                dataStreamId,
                contentId,
                providerId,
                triggeringProcessId,
                triggeringProcessImageFileName
            );
        }
    }
}