using System.Runtime.InteropServices;
using static nyasProjFS.ProjectedFSLib.CallbackTypes;

namespace nyasProjFS.ProjectedFSLib;

internal struct PrjCallbacks
{
    // function pointers for unmanaged code usage
    internal nint StartDirectoryEnumerationCallbackPtr;
    internal nint EndDirectoryEnumerationCallbackPtr;
    internal nint GetDirectoryEnumerationCallbackPtr;
    internal nint GetPlaceholderInfoCallbackPtr;
    internal nint GetFileDataCallbackPtr;
    internal nint QueryFileNameCallbackPtr;
    internal nint NotificationCallbackPtr;
    internal nint CancelCommandCallbackPtr;

    // the provider must implement the following callbacks
    internal PrjStartDirectoryEnumeration  StartDirectoryEnumerationCallback {
        set => StartDirectoryEnumerationCallbackPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    internal PrjEndDirectoryEnumeration    EndDirectoryEnumerationCallback {
        set => EndDirectoryEnumerationCallbackPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    internal PrjGetDirectoryEnumeration    GetDirectoryEnumerationCallback {
        set => GetDirectoryEnumerationCallbackPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    internal PrjGetPlaceholderInfo         GetPlaceholderInfoCallback {
        set => GetPlaceholderInfoCallbackPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    internal PrjGetFileData                GetFileDataCallback {
        set => GetFileDataCallbackPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    //  Optional.  If the provider does not implement this callback, ProjFS will invoke the directory
    //  enumeration callbacks to determine the existence of a file path in the provider's store.
    internal PrjQueryFileName              QueryFileNameCallback {
        set => QueryFileNameCallbackPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    //  Optional.  If the provider does not implement this callback, it will not get any notifications from ProjFS.
    internal CallbackTypes.PrjNotification NotificationCallback {
        set => NotificationCallbackPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    //  Optional.  If the provider does not implement this callback, operations in ProjFS will not be able to be canceled.
    internal PrjCancelCommand              CancelCommandCallback {
        set => CancelCommandCallbackPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
}