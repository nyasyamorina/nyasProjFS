namespace nyasProjFS;

/// <summary>
/// Represents a path relative to a virtualization root and the notification bit mask that should apply to it.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="NotificationMapping"/> object describes a "notification mapping", which is a pairing between a directory
/// (referred to as a "notification root") and a set of notifications, expressed as a bit mask, which
/// ProjFS should send for that directory and its descendants.
/// </para>
/// <para>
/// The provider passes zero or more <see cref="NotificationMapping"/> objects to the <paramref name="notificationMappings"/>
/// parameter of the <c>VirtualizationInstance.StartVirtualizing</c> method to configure
/// notifications for the virtualization root.
/// </para>
/// <para>
/// If the provider does not specify any notification mappings, ProjFS will default to sending the
/// notifications <see cref="NotificationType.FileOpened"/>, <see cref="NotificationType.NewFileCreated"/>,
/// and <see cref="NotificationType.FileOverwritten"/> for all files and directories
/// in the virtualization instance.
/// </para>
/// <para>
/// The <see cref="NotificationRoot"/> property holds the notification root.  It is specified
/// relative to the virtualization root, with an empty string representing the virtualization root
/// itself.
/// </para>
/// <para>
/// If the provider specifies multiple notification mappings, and some are descendants of others,
/// the mappings must be specified in descending depth.  Notification mappings at deeper levels
/// override higher-level mappings for their descendants.
/// </para>
/// <para>
/// For example, consider the following virtualization instance layout, with C:\VirtRoot as the
/// virtualization root:
/// <code>
/// C:\VirtRoot
/// +--- baz
/// \--- foo
///      +--- subdir1
///      \--- subdir2
/// </code>
/// The provider wants:
/// <list type="bullet">
/// <item>
/// <description>Notification of new file/directory creates for most of the virtualization instance</description>
/// </item>
/// <item>
/// <description>Notification of new file/directory creates, file opens, and file deletes for C:\VirtRoot\foo</description>
/// </item>
/// <item>
/// <description>No notifications for C:\VirtRoot\foo\subdir1</description>
/// </item>
/// </list>
/// The provider could describe this with the following pseudocode:
/// <code>
/// List&lt;NotificationMapping&gt; notificationMappings = new List&lt;NotificationMapping&gt;()
/// {
///     // Configure default notifications
///     new NotificationMapping(NotificationType.NewFileCreated,
///                             string.Empty),
///     // Configure notifications for C:\VirtRoot\foo
///     new NotificationMapping(NotificationType.NewFileCreated | NotificationType.FileOpened | NotificationType.FileHandleClosedFileDeleted,
///                             "foo"),
///     // Configure notifications for C:\VirtRoot\foo\subdir1
///     new NotificationMapping(NotificationType.None,
///                             "foo\\subdir1"),
/// };
///
/// // Call VirtualizationRoot.StartVirtualizing() passing in the notificationMappings List.
/// </code>
/// </para>
/// </remarks>
public class NotificationMapping
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationMapping"/> class with the
    /// <see cref="NotificationRoot"/> property set to the virtualization root (i.e. <c>null</c>)
    /// and the <see cref="NotificationMask"/> property set to <see cref="NotificationType.None"/>.
    /// </summary>
    public NotificationMapping()
    {
        NotificationMask = NotificationType.None;
        _notificationRoot = "";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationMapping"/> class with the
    /// specified property values.
    /// </summary>
    /// <param name="notificationMask">The set of notifications that ProjFS should return for the
    /// virtualization root specified in <paramref name="notificationRoot"/>.</param>
    /// <param name="notificationRoot">The path to the notification root, relative to the virtualization
    /// root.  The virtualization root itself must be specified as an empty string.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="notificationRoot"/> is <c>.</c> or begins with <c>.\</c>.  <paramref name="notificationRoot"/>
    /// must be specified relative to the virtualization root, with the virtualization root itself
    /// specified as an empty string.
    /// </exception>
    public NotificationMapping(NotificationType notificationMask, string notificationRoot)
    {
        NotificationMask = notificationMask;
        _notificationRoot = EnsureRoot(notificationRoot);
    }

    /// <summary>
    /// A bit vector of <c>NotificationType</c> values.
    /// </summary>
    /// <value>
    /// ProjFS will send to the provider the specified notifications for operations performed on
    /// the directory specified by the <see cref="NotificationRoot"/> property and its descendants.
    /// </value>
    public NotificationType NotificationMask { get; set; }

    /// <summary>
    /// A path to a directory, relative to the virtualization root.  The virtualization root itself
    /// must be specified as an empty string.
    /// </summary>
    /// <value>
    /// ProjFS will send to the provider the notifications specified in <see cref="NotificationMask"/>
    /// for this directory and its descendants.
    /// </value>
    /// <exception cref="ArgumentException">
    /// The notification root value is <c>.</c> or begins with <c>.\</c>.  The notification root
    /// must be specified relative to the virtualization root, with the virtualization root itself
    /// specified as an empty string.
    /// </exception>
    public string NotificationRoot { get => _notificationRoot; set => _notificationRoot = EnsureRoot(value); }

    private string _notificationRoot;
    private static string EnsureRoot(string root)
    {
        if (root == "." || root.StartsWith(".\\")) {
            throw new ArgumentException(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                                      "notificationRoot cannot be \".\" or begin with \".\\\""));
        }
        return root;
    }
}