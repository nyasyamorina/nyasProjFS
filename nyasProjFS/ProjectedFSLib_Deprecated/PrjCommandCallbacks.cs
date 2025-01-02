using System.Runtime.InteropServices;
using static nyasProjFS.ProjectedFSLib.CallbackTypes;
using static nyasProjFS.ProjectedFSLib_Deprecated.CallbackTypes;

namespace nyasProjFS.ProjectedFSLib_Deprecated;

public struct PrjCommandCallbacks
{
    public static uint StructSize { get; } = (uint) Marshal.SizeOf<PrjCommandCallbacks>();

    // initialized by PrjCommandCallbacksInit()
    public uint Size;
    // function pointers for unmanaged code usage
    public nint PrjStartDirectoryEnumerationPtr;
    public nint PrjEndDirectoryEnumerationPtr;
    public nint PrjGetDirectoryEnumerationPtr;
    public nint PrjGetPlaceholderInformationPtr;
    public nint PrjGetFileStreamPtr;
    public nint PrjQueryFileNamePtr;
    public nint PrjNotifyOperationPtr;
    public nint PrjCancelCommandPtr;

    // the provider must implement the following callbacks
    public PrjStartDirectoryEnumeration PrjStartDirectoryEnumeration {
        set => PrjStartDirectoryEnumerationPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    public PrjEndDirectoryEnumeration   PrjEndDirectoryEnumeration {
        set => PrjEndDirectoryEnumerationPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    public PrjGetDirectoryEnumeration   PrjGetDirectoryEnumeration {
        set => PrjGetDirectoryEnumerationPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    public PrjGetPlaceholderInformation PrjGetPlaceholderInformation {
        set => PrjGetPlaceholderInformationPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    public PrjGetFileStream             PrjGetFileStream {
        set => PrjGetFileStreamPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    //  Optional.  If the provider does not implement this callback, ProjFS will invoke the directory
    //  enumeration callbacks to determine the existence of a file path in the provider's store.
    public PrjQueryFileName             PrjQueryFileName {
        set => PrjQueryFileNamePtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    //  Optional.  If the provider does not implement this callback, it will not get any notifications from ProjFS.
    public PrjNotifyOperation           PrjNotifyOperation {
        set => PrjNotifyOperationPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
    //  Optional.  If the provider does not implement this callback, operations in ProjFS will not be able to be canceled.
    public PrjCancelCommand             PrjCancelCommand {
        set => PrjCancelCommandPtr = Marshal.GetFunctionPointerForDelegate(value);
    }
}