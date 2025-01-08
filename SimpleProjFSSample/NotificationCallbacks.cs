﻿using nyasProjFS;

namespace SimpleProjFSSample;

class NotificationCallbacks
{
    private readonly SimpleProvider provider;

    public NotificationCallbacks(
        SimpleProvider provider,
        VirtualizationInstance virtInstance,
        IReadOnlyCollection<NotificationMapping> notificationMappings
    ) {
        this.provider = provider;

        // Look through notificationMappings for all the set notification bits.  Supply a callback
        // for each set bit.
        NotificationType notification = NotificationType.None;
        foreach (NotificationMapping mapping in notificationMappings) {
            notification |= mapping.NotificationMask;
        }

        if ((notification & NotificationType.FileOpened) == NotificationType.FileOpened) {
            virtInstance.OnNotifyFileOpened = NotifyFileOpenedCallback;
        }
        if ((notification & NotificationType.NewFileCreated) == NotificationType.NewFileCreated) {
            virtInstance.OnNotifyNewFileCreated = NotifyNewFileCreatedCallback;
        }
        if ((notification & NotificationType.FileOverwritten) == NotificationType.FileOverwritten) {
            virtInstance.OnNotifyFileOverwritten = NotifyFileOverwrittenCallback;
        }
        if ((notification & NotificationType.PreDelete) == NotificationType.PreDelete) {
            virtInstance.OnNotifyPreDelete = NotifyPreDeleteCallback;
        }
        if ((notification & NotificationType.PreRename) == NotificationType.PreRename) {
            virtInstance.OnNotifyPreRename = NotifyPreRenameCallback;
        }
        if ((notification & NotificationType.PreCreateHardlink) == NotificationType.PreCreateHardlink) {
            virtInstance.OnNotifyPreCreateHardlink = NotifyPreCreateHardlinkCallback;
        }
        if ((notification & NotificationType.FileRenamed) == NotificationType.FileRenamed) {
            virtInstance.OnNotifyFileRenamed = NotifyFileRenamedCallback;
        }
        if ((notification & NotificationType.HardlinkCreated) == NotificationType.HardlinkCreated) {
            virtInstance.OnNotifyHardlinkCreated = NotifyHardlinkCreatedCallback;
        }
        if ((notification & NotificationType.FileHandleClosedNoModification) == NotificationType.FileHandleClosedNoModification) {
            virtInstance.OnNotifyFileHandleClosedNoModification = NotifyFileHandleClosedNoModificationCallback;
        }
        if (((notification & NotificationType.FileHandleClosedFileModified) == NotificationType.FileHandleClosedFileModified) ||
            ((notification & NotificationType.FileHandleClosedFileDeleted) == NotificationType.FileHandleClosedFileDeleted)) {
            virtInstance.OnNotifyFileHandleClosedFileModifiedOrDeleted = NotifyFileHandleClosedFileModifiedOrDeletedCallback;
        }
        if ((notification & NotificationType.FilePreConvertToFull) == NotificationType.FilePreConvertToFull) {
            virtInstance.OnNotifyFilePreConvertToFull = NotifyFilePreConvertToFullCallback;
        }
    }

    public bool NotifyFileOpenedCallback(
        string relativePath,
        bool isDirectory,
        uint triggeringProcessId,
        string triggeringProcessImageFileName,
        out NotificationType notificationMask
    ) {
        Logger.Info($"NotifyFileOpenedCallback [{relativePath}]");
        Logger.Info($"  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]");

        notificationMask = NotificationType.UseExistingMask;
        provider.SignalIfTestMode("FileOpened");
        return true;
    }


    public void NotifyNewFileCreatedCallback(
        string relativePath,
        bool isDirectory,
        uint triggeringProcessId,
        string triggeringProcessImageFileName,
        out NotificationType notificationMask
    ) {
        Logger.Info($"NotifyNewFileCreatedCallback [{relativePath}]");
        Logger.Info($"  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]");

        notificationMask = NotificationType.UseExistingMask;
        provider.SignalIfTestMode("NewFileCreated");
    }

    public void NotifyFileOverwrittenCallback(
        string relativePath,
        bool isDirectory,
        uint triggeringProcessId,
        string triggeringProcessImageFileName,
        out NotificationType notificationMask
    ) {
        Logger.Info($"NotifyFileOverwrittenCallback [{relativePath}]");
        Logger.Info($"  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]");

        notificationMask = NotificationType.UseExistingMask;
        provider.SignalIfTestMode("FileOverwritten");
    }

    public bool NotifyPreDeleteCallback(
        string relativePath,
        bool isDirectory,
        uint triggeringProcessId,
        string triggeringProcessImageFileName
    ) {
        Logger.Info($"NotifyPreDeleteCallback [{relativePath}]");
        Logger.Info($"  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]");

        provider.SignalIfTestMode("PreDelete");
        return provider.Options.DenyDeletes ? false : true;
    }

    public bool NotifyPreRenameCallback(
        string relativePath,
        string? destinationPath,
        uint triggeringProcessId,
        string triggeringProcessImageFileName
    ) {
        Logger.Info($"NotifyPreRenameCallback [{relativePath}] [{destinationPath}]");
        Logger.Info($"  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]");

        provider.SignalIfTestMode("PreRename");
        return true;
    }

    public bool NotifyPreCreateHardlinkCallback(
        string relativePath,
        string? destinationPath,
        uint triggeringProcessId,
        string triggeringProcessImageFileName
    ) {
        Logger.Info($"NotifyPreCreateHardlinkCallback [{relativePath}] [{destinationPath}]");
        Logger.Info($"  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]");

        provider.SignalIfTestMode("PreCreateHardlink");
        return true;
    }

    public void NotifyFileRenamedCallback(
        string relativePath,
        string? destinationPath,
        bool isDirectory,
        uint triggeringProcessId,
        string triggeringProcessImageFileName,
        out NotificationType notificationMask
    ) {
        Logger.Info($"NotifyFileRenamedCallback [{relativePath}] [{destinationPath}]");
        Logger.Info($"  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]");

        notificationMask = NotificationType.UseExistingMask;
        provider.SignalIfTestMode("FileRenamed");
    }

    public void NotifyHardlinkCreatedCallback(
        string relativePath,
        string? destinationPath,
        uint triggeringProcessId,
        string triggeringProcessImageFileName
    ) {
        Logger.Info($"NotifyHardlinkCreatedCallback [{relativePath}] [{destinationPath}]");
        Logger.Info($"  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]");

        provider.SignalIfTestMode("HardlinkCreated");
    }

    public void NotifyFileHandleClosedNoModificationCallback(
        string relativePath,
        bool isDirectory,
        uint triggeringProcessId,
        string triggeringProcessImageFileName
    ) {
        Logger.Info($"NotifyFileHandleClosedNoModificationCallback [{relativePath}]");
        Logger.Info($"  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]");

        provider.SignalIfTestMode("FileHandleClosedNoModification");
    }

    public void NotifyFileHandleClosedFileModifiedOrDeletedCallback(
        string relativePath,
        bool isDirectory,
        bool isFileModified,
        bool isFileDeleted,
        uint triggeringProcessId,
        string triggeringProcessImageFileName
    ) {
        Logger.Info($"NotifyFileHandleClosedFileModifiedOrDeletedCallback [{relativePath}]");
        Logger.Info($"  Modified: {isFileModified}, Deleted: {isFileDeleted} ");
        Logger.Info($"  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]");

        provider.SignalIfTestMode("FileHandleClosedFileModifiedOrDeleted");
    }

    public bool NotifyFilePreConvertToFullCallback(
        string relativePath,
        uint triggeringProcessId,
        string triggeringProcessImageFileName
    ) {
        Logger.Info($"NotifyFilePreConvertToFullCallback [{relativePath}]");
        Logger.Info($"  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]");

        provider.SignalIfTestMode("FilePreConvertToFull");
        return true;
    }
}