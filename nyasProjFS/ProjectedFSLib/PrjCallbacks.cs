using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using static nyasProjFS.ProjectedFSLib.CallbackTypes.Underlying;

namespace nyasProjFS.ProjectedFSLib;

[NativeMarshalling(typeof(PrjCallbacksMarshaller))]
internal struct PrjCallbacks
{
    // the provider must implement the following callbacks
    internal PrjStartDirectoryEnumeration?  StartDirectoryEnumerationCallback;
    internal PrjEndDirectoryEnumeration?    EndDirectoryEnumerationCallback;
    internal PrjGetDirectoryEnumeration?    GetDirectoryEnumerationCallback;
    internal PrjGetPlaceholderInfo?         GetPlaceholderInfoCallback;
    internal PrjGetFileData?                GetFileDataCallback;
    // Optional.  If the provider does not implement this callback, ProjFS will invoke the directory
    // enumeration callbacks to determine the existence of a file path in the provider's store.
    internal PrjQueryFileName?              QueryFileNameCallback;
    // Optional.  If the provider does not implement this callback, it will not get any notifications from ProjFS.
    internal CallbackTypes.Underlying.PrjNotification? NotificationCallback;
    // Optional.  If the provider does not implement this callback, operations in ProjFS will not be able to be canceled.
    internal PrjCancelCommand?              CancelCommandCallback;
}


internal struct PrjCallbacksUnmanaged
{
    internal nint StartDirectoryEnumerationCallbackPtr;
    internal nint EndDirectoryEnumerationCallbackPtr;
    internal nint GetDirectoryEnumerationCallbackPtr;
    internal nint GetPlaceholderInfoCallbackPtr;
    internal nint GetFileDataCallbackPtr;
    internal nint QueryFileNameCallbackPtr;
    internal nint NotificationCallbackPtr;
    internal nint CancelCommandCallbackPtr;
}

[CustomMarshaller(typeof(PrjCallbacks), MarshalMode.ManagedToUnmanagedIn, typeof(PrjCallbacksMarshaller))]
internal static class PrjCallbacksMarshaller
{
    internal static PrjCallbacksUnmanaged ConvertToUnmanaged(in PrjCallbacks managed)
    {
        PrjCallbacksUnmanaged unmanaged = default;
        unmanaged.StartDirectoryEnumerationCallbackPtr = GetFuncPtr(managed.StartDirectoryEnumerationCallback);
        unmanaged.EndDirectoryEnumerationCallbackPtr   = GetFuncPtr(managed.EndDirectoryEnumerationCallback);
        unmanaged.GetDirectoryEnumerationCallbackPtr   = GetFuncPtr(managed.GetDirectoryEnumerationCallback);
        unmanaged.GetPlaceholderInfoCallbackPtr        = GetFuncPtr(managed.GetPlaceholderInfoCallback);
        unmanaged.GetFileDataCallbackPtr               = GetFuncPtr(managed.GetFileDataCallback);
        unmanaged.QueryFileNameCallbackPtr             = GetFuncPtr(managed.QueryFileNameCallback);
        unmanaged.NotificationCallbackPtr              = GetFuncPtr(managed.NotificationCallback);
        unmanaged.CancelCommandCallbackPtr             = GetFuncPtr(managed.CancelCommandCallback);
        return unmanaged;
    }

    private static nint GetFuncPtr<T>(T? delegateFunc) where T : notnull => delegateFunc is null ? 0 : Marshal.GetFunctionPointerForDelegate(delegateFunc);
}