namespace nyasProjFS.ProjectedFSLib;

public struct PrjCallbacks
{
    public CallbackTypes.PrjStartDirectoryEnumeration StartDirectoryEnumerationCallback;
    public CallbackTypes.PrjEndDirectoryEnumeration   EndDirectoryEnumerationCallback;
    public CallbackTypes.PrjGetDirectoryEnumeration   GetDirectoryEnumerationCallback;
    public CallbackTypes.PrjGetPlaceholderInfo        GetPlaceholderInfoCallback;
    public CallbackTypes.PrjGetFileData               GetFileDataCallback;
    public CallbackTypes.PrjQueryFileName             QueryFileNameCallback;
    public CallbackTypes.PrjNotification              NotificationCallback;
    public CallbackTypes.PrjCancelCommand             CancelCommandCallback;
}