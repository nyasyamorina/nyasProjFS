using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using static nyasProjFS.ProjectedFSLib.CallbackTypes.Underlying;
using static nyasProjFS.ProjectedFSLib.Deprecated.CallbackTypes.Underlying;

namespace nyasProjFS.ProjectedFSLib.Deprecated;

[NativeMarshalling(typeof(PrjCommandCallbacksMarshaller))]
internal struct PrjCommandCallbacks
{
    internal static uint StructSize { get; } = (uint) Marshal.SizeOf<PrjCommandCallbacksUnmanaged>();

    // the provider must implement the following callbacks
    internal uint Size;
    internal PrjStartDirectoryEnumeration? StartDirectoryEnumerationCallback;
    internal PrjEndDirectoryEnumeration?   EndDirectoryEnumerationCallback;
    internal PrjGetDirectoryEnumeration?   GetDirectoryEnumerationCallback;
    internal PrjGetPlaceholderInformation? GetPlaceholderInformationCallback;
    internal PrjGetFileStream?             GetFileStreamCallback;
    //  Optional.  If the provider does not implement this callback, ProjFS will invoke the directory
    //  enumeration callbacks to determine the existence of a file path in the provider's store.
    internal PrjQueryFileName?             QueryFileNameCallback;
    //  Optional.  If the provider does not implement this callback, it will not get any notifications from ProjFS.
    internal PrjNotifyOperation?           NotifyOperationCallback;
    //  Optional.  If the provider does not implement this callback, operations in ProjFS will not be able to be canceled.
    internal PrjCancelCommand?             CancelCommandCallback;
}


internal struct PrjCommandCallbacksUnmanaged
{
    internal uint Size;
    internal nint StartDirectoryEnumerationCallbackPtr;
    internal nint EndDirectoryEnumerationCallbackPtr;
    internal nint GetDirectoryEnumerationCallbackPtr;
    internal nint GetPlaceholderInformationCallbackPtr;
    internal nint GetFileStreamCallbackPtr;
    internal nint QueryFileNameCallbackPtr;
    internal nint NotifyOperationCallbackPtr;
    internal nint CancelCommandCallbackPtr;
}

[CustomMarshaller(typeof(PrjCommandCallbacks), MarshalMode.Default, typeof(PrjCommandCallbacksMarshaller))]
internal static class PrjCommandCallbacksMarshaller
{
    internal static PrjCommandCallbacksUnmanaged ConvertToUnmanaged(in PrjCommandCallbacks managed)
    {
        PrjCommandCallbacksUnmanaged unmanaged = default;
        unmanaged.Size = managed.Size;
        unmanaged.StartDirectoryEnumerationCallbackPtr = GetFuncPtr(managed.StartDirectoryEnumerationCallback);
        unmanaged.EndDirectoryEnumerationCallbackPtr   = GetFuncPtr(managed.EndDirectoryEnumerationCallback);
        unmanaged.GetDirectoryEnumerationCallbackPtr   = GetFuncPtr(managed.GetDirectoryEnumerationCallback);
        unmanaged.GetPlaceholderInformationCallbackPtr = GetFuncPtr(managed.GetPlaceholderInformationCallback);
        unmanaged.GetFileStreamCallbackPtr             = GetFuncPtr(managed.GetFileStreamCallback);
        unmanaged.QueryFileNameCallbackPtr             = GetFuncPtr(managed.QueryFileNameCallback);
        unmanaged.NotifyOperationCallbackPtr           = GetFuncPtr(managed.NotifyOperationCallback);
        unmanaged.CancelCommandCallbackPtr             = GetFuncPtr(managed.CancelCommandCallback);
        return unmanaged;
    }

    internal static PrjCommandCallbacks ConvertToManaged(in PrjCommandCallbacksUnmanaged unmanaged)
    {
        PrjCommandCallbacks managed = default;
        managed.Size = unmanaged.Size;
        managed.StartDirectoryEnumerationCallback = GetDelegateFunc<PrjStartDirectoryEnumeration>(unmanaged.StartDirectoryEnumerationCallbackPtr);
        managed.EndDirectoryEnumerationCallback   = GetDelegateFunc<PrjEndDirectoryEnumeration  >(unmanaged.EndDirectoryEnumerationCallbackPtr);
        managed.GetDirectoryEnumerationCallback   = GetDelegateFunc<PrjGetDirectoryEnumeration  >(unmanaged.GetDirectoryEnumerationCallbackPtr);
        managed.GetPlaceholderInformationCallback = GetDelegateFunc<PrjGetPlaceholderInformation>(unmanaged.GetPlaceholderInformationCallbackPtr);
        managed.GetFileStreamCallback             = GetDelegateFunc<PrjGetFileStream            >(unmanaged.GetFileStreamCallbackPtr);
        managed.QueryFileNameCallback             = GetDelegateFunc<PrjQueryFileName            >(unmanaged.QueryFileNameCallbackPtr);
        managed.NotifyOperationCallback           = GetDelegateFunc<PrjNotifyOperation          >(unmanaged.NotifyOperationCallbackPtr);
        managed.CancelCommandCallback             = GetDelegateFunc<PrjCancelCommand            >(unmanaged.CancelCommandCallbackPtr);
        return managed;
    }

    private static nint GetFuncPtr<T>(T? delegateFunc) where T : notnull => delegateFunc is null ? 0 : Marshal.GetFunctionPointerForDelegate(delegateFunc);
    private static T? GetDelegateFunc<T>(nint funcPtr) where T : notnull => funcPtr == 0 ? default : throw new ArgumentException($"unexpect function pointer came from unmanaged code: {nameof(T)}");
}