using System.Runtime.InteropServices;
using static nyasProjFS.ProjectedFSLib.CallbackTypes;
using static nyasProjFS.ProjectedFSLib.Deprecated.CallbackTypes;

namespace nyasProjFS.ProjectedFSLib.Deprecated;

internal struct PrjCommandCallbacks
{
    internal static uint StructSize { get; } = (uint) Marshal.SizeOf<PrjCommandCallbacks>();

    // initialized by PrjCommandCallbacksInit()
    internal uint Size;
    // function pointers for unmanaged code usage
    internal nint PrjStartDirectoryEnumerationPtr;
    internal nint PrjEndDirectoryEnumerationPtr;
    internal nint PrjGetDirectoryEnumerationPtr;
    internal nint PrjGetPlaceholderInformationPtr;
    internal nint PrjGetFileStreamPtr;
    internal nint PrjQueryFileNamePtr;
    internal nint PrjNotifyOperationPtr;
    internal nint PrjCancelCommandPtr;

    // the provider must implement the following callbacks
    internal PrjStartDirectoryEnumeration PrjStartDirectoryEnumeration {
        set => PrjStartDirectoryEnumerationPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    internal PrjEndDirectoryEnumeration   PrjEndDirectoryEnumeration {
        set => PrjEndDirectoryEnumerationPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    internal PrjGetDirectoryEnumeration   PrjGetDirectoryEnumeration {
        set => PrjGetDirectoryEnumerationPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    internal PrjGetPlaceholderInformation PrjGetPlaceholderInformation {
        set => PrjGetPlaceholderInformationPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    internal PrjGetFileStream             PrjGetFileStream {
        set => PrjGetFileStreamPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    //  Optional.  If the provider does not implement this callback, ProjFS will invoke the directory
    //  enumeration callbacks to determine the existence of a file path in the provider's store.
    internal PrjQueryFileName             PrjQueryFileName {
        set => PrjQueryFileNamePtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    //  Optional.  If the provider does not implement this callback, it will not get any notifications from ProjFS.
    internal PrjNotifyOperation           PrjNotifyOperation {
        set => PrjNotifyOperationPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    //  Optional.  If the provider does not implement this callback, operations in ProjFS will not be able to be canceled.
    internal PrjCancelCommand             PrjCancelCommand {
        set => PrjCancelCommandPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
}