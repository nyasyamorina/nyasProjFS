namespace nyasProjFS.ProjectedFSLib_Deprecated;

public struct PrjCommandCallbacks
{
    // initialized by PrjCommandCallbacksInit()
    public uint Size;
    // the provider must implement the following callbacks
    public ProjectedFSLib.CallbackTypes.PrjStartDirectoryEnumeration PrjStartDirectoryEnumeration;
    public ProjectedFSLib.CallbackTypes.PrjEndDirectoryEnumeration   PrjEndDirectoryEnumeration;
    public ProjectedFSLib.CallbackTypes.PrjGetDirectoryEnumeration   PrjGetDirectoryEnumeration;
    public CallbackTypes.PrjGetPlaceholderInformation                PrjGetPlaceholderInformation;
    public CallbackTypes.PrjGetFileStream                            PrjGetFileStream;
    //  Optional.  If the provider does not implement this callback, ProjFS will invoke the directory
    //  enumeration callbacks to determine the existence of a file path in the provider's store.
    public ProjectedFSLib.CallbackTypes.PrjQueryFileName             PrjQueryFileName;
    //  Optional.  If the provider does not implement this callback, it will not get any notifications
    //  from ProjFS.
    public CallbackTypes.PrjNotifyOperation                          PrjNotifyOperation;
    //  Optional.  If the provider does not implement this callback, operations in ProjFS will not
    //  be able to be canceled.
    public ProjectedFSLib.CallbackTypes.PrjCancelCommand             PrjCancelCommand;
}