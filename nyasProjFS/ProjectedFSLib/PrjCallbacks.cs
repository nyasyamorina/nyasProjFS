using System.Runtime.InteropServices;
using static nyasProjFS.ProjectedFSLib.CallbackTypes;

namespace nyasProjFS.ProjectedFSLib;

public struct PrjCallbacks
{
    // function pointers for unmanaged code usage
    public nint StartDirectoryEnumerationCallbackPtr;
    public nint EndDirectoryEnumerationCallbackPtr;
    public nint GetDirectoryEnumerationCallbackPtr;
    public nint GetPlaceholderInfoCallbackPtr;
    public nint GetFileDataCallbackPtr;
    public nint QueryFileNameCallbackPtr;
    public nint NotificationCallbackPtr;
    public nint CancelCommandCallbackPtr;

    // the provider must implement the following callbacks
    public PrjStartDirectoryEnumeration  StartDirectoryEnumerationCallback {
        set => StartDirectoryEnumerationCallbackPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    public PrjEndDirectoryEnumeration    EndDirectoryEnumerationCallback {
        set => EndDirectoryEnumerationCallbackPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    public PrjGetDirectoryEnumeration    GetDirectoryEnumerationCallback {
        set => GetDirectoryEnumerationCallbackPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    public PrjGetPlaceholderInfo         GetPlaceholderInfoCallback {
        set => GetPlaceholderInfoCallbackPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    public PrjGetFileData                GetFileDataCallback {
        set => GetFileDataCallbackPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    //  Optional.  If the provider does not implement this callback, ProjFS will invoke the directory
    //  enumeration callbacks to determine the existence of a file path in the provider's store.
    public PrjQueryFileName              QueryFileNameCallback {
        set => QueryFileNameCallbackPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    //  Optional.  If the provider does not implement this callback, it will not get any notifications from ProjFS.
    public CallbackTypes.PrjNotification NotificationCallback {
        set => NotificationCallbackPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    //  Optional.  If the provider does not implement this callback, operations in ProjFS will not be able to be canceled.
    public PrjCancelCommand              CancelCommandCallback {
        set => CancelCommandCallbackPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
}