namespace nyasProjFS;

/// <summary>
/// Defines values for file system operation notifications ProjFS can send to a provider.
/// </summary>
/// <remarks>
///     <para>
///     ProjFS can send notifications of file system activity to a provider.  When the provider
///     starts a virtualization instance it specifies which notifications it wishes to receive.
///     It may also specify a new set of notifications for a file when it is created or renamed.
///     The provider must set implementations of <c>Notify...Callback</c> delegates in the <c>OnNotify...</c>
///     properties of <c>ProjFS.VirtualizationInstance</c> in order to receive the notifications
///     for which it registers.
///     </para>
///     <para>
///     ProjFS sends notifications for files and directories managed by an active virtualization
///     instance. That is, ProjFS will send notifications for the virtualization root and its
///     descendants.  Symbolic links and junctions within the virtualization root are not traversed
///     when determining what constitutes a descendant of the virtualization root.
///     </para>
///     <para>
///     ProjFS sends notifications only for the primary data stream of a file.  ProjFS does not
///     send notifications for operations on alternate data streams.
///     </para>
///     <para>
///     ProjFS does not send notifications for an inactive virtualization instance.  A virtualization
///     instance is inactive if any one of the following is true:
///         <list type="bullet">
///             <item>
///                 <description>
///                 The provider has not yet started it by calling <c>ProjFS.VirtualizationInstance.StartVirtualizing</c>.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                 The provider has stopped the instance by calling <c>ProjFS.VirtualizationInstance.StopVirtualizing</c>.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                 The provider process has exited.
///                 </description>
///             </item>
///         </list>
///     </para>
///     <para>
///     The provider may specify which notifications it wishes to receive when starting a virtualization
///     instance, or in response to a notification that allows a new notification mask to be set.
///     The provider specifies a default set of notifications that it wants ProjFS to send for the
///     virtualization instance when it starts the instance.  The provider specifies the default
///     notifications via the <paramref name="notificationMappings"/> parameter of the
///     <c>ProjFS.VirtualizationInstance</c> constructor, which may specify different notification
///     masks for different subtrees of the virtualization instance.
///     </para>
///     <para>
///     The provider may choose to supply a different notification mask in response to a notification
///     of file open, create, overwrite, or rename.  ProjFS will continue to send these notifications
///     for the given file until all handles to the file are closed.  After that it will revert
///     to the default set of notifications.  Naturally if the default set of notifications does
///     not specify that ProjFS should notify for open, create, etc., the provider will not get
///     the opportunity to specify a different mask for those operations.
///     </para>
/// </remarks>
[Flags]
public enum NotificationType : uint {
    /// <summary>Indicates that the provider does not want any notifications.
    /// This value overrides all others</summary>
    None                           = 0x00000000,
    SuppressNotifications          = 0x00000001,
    /// <summary>indicates that ProjFS should call the provider's <c>OnNotifyFileOpened</c> callback
    /// when a handle is created to an existing file or directory</summary>
    FileOpened                     = 0x00000002,
    /// <summary>indicates that ProjFS should call the provider's <c>OnNotifyNewFileCreated</c> callback
    /// when a new file or directory is created</summary>
    NewFileCreated                 = 0x00000004,
    /// <summary>indicates that ProjFS should call the provider's <c>OnNotifyFileOverwritten</c> callback
    /// when an existing file is superseded or overwritten</summary>
    FileOverwritten                = 0x00000008,
    /// <summary>indicates that ProjFS should call the provider's <c>OnNotifyPreDelete</c> callback
    /// when a file or directory is about to be deleted</summary>
    PreDelete                      = 0x00000010,
    /// <summary>indicates that ProjFS should call the provider's <c>OnNotifyPreRename</c> callback
    /// when a file or directory is about to be renamed</summary>
    PreRename                      = 0x00000020,
    /// <summary>indicates that ProjFS should call the provider's <c>OnNotifyPreCreateHardlink</c> callback
    /// when a hard link is about to be created for a file</summary>
    PreCreateHardlink              = 0x00000040,
    /// <summary>indicates that ProjFS should call the provider's <c>OnNotifyFileRenamed</c> callback
    /// when a file or directory has been renamed</summary>
    FileRenamed                    = 0x00000080,
    /// <summary>indicates that ProjFS should call the provider's <c>OnNotifyHardlinkCreated</c> callback
    /// when a hard link has been created for a file</summary>
    HardlinkCreated                = 0x00000100,
    /// <summary>indicates that ProjFS should call the provider's <c>OnNotifyFileHandleClosedNoModification</c>
    /// callback when a handle is closed on a file or directory and the closing handle neither modified nor deleted it</summary>
    FileHandleClosedNoModification = 0x00000200,
    /// <summary>indicates that ProjFS should call the provider's <c>OnNotifyFileHandleClosedFileModifiedOrDeleted</c>
    /// callback when a handle is closed on a file or directory and the closing handle was used to modify it</summary>
    FileHandleClosedFileModified   = 0x00000400,
    /// <summary>indicates that ProjFS should call the provider's <c>OnNotifyFileHandleClosedFileModifiedOrDeleted</c>
    /// callback when a handle is closed on a file or directory and it is deleted as part of closing the handle</summary>
    FileHandleClosedFileDeleted    = 0x00000800,
    /// <summary>indicates that ProjFS should call the provider's <c>OnNotifyFilePreConvertToFull</c>
    /// callback when it is about to convert a placeholder to a full file</summary>
    FilePreConvertToFull           = 0x00001000,
    /// <summary>This value is not used when calling the <c>VirtualizationInstance</c> constructor.
    /// It is only returned from <c>OnNotify...</c> callbacks that have a <paramref name="notificationMask"/>
    /// parameter, and indicates that the provider wants to continue to receive the notifications it
    /// registered for when starting the virtualization instance.</summary>
    UseExistingMask                = 0xFFFFFFFF,
}