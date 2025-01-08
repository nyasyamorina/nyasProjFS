using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace nyasProjFS.ProjectedFSLib;

using PrjNamespaceVirtualizationContext = nint;

[NativeMarshalling(typeof(PrjCallbackDataMarsheller))]
internal struct PrjCallbackData
{
    internal uint Size;
    internal PrjCallbackDataFlags Flags;
    internal PrjNamespaceVirtualizationContext NamespaceVirtualizationContext;
    internal int CammandId;
    internal Guid FileId;
    internal Guid DataStreamId;
    internal string FilePathName;
    internal PrjPlaceholderVersionInfo? VersionInfo;
    internal uint TriggeringProccessId;
    internal string? TriggeringProccessImageFileName;
    internal nint InstanceContext;
}

// ?? `MarshalAs` is not working with `LibraryImport` ??

internal unsafe struct PrjCallbackDataUnmanaged
{
    internal uint Size;
    internal PrjCallbackDataFlags Flags;
    internal PrjNamespaceVirtualizationContext NamespaceVirtualizationContext;
    internal int CammandId;
    internal Guid FileId;
    internal Guid DataStreamId;
    internal ushort* FilePathName;
    internal PrjPlaceholderVersionInfoUnmanaged* VersionInfo;
    internal uint TriggeringProccessId;
    internal ushort* TriggeringProccessImageFileName;
    internal void* InstanceContext;
}

[CustomMarshaller(typeof(PrjCallbackData), MarshalMode.UnmanagedToManagedIn, typeof(PrjCallbackDataMarsheller))]
internal static class PrjCallbackDataMarsheller
{
    internal static unsafe PrjCallbackData ConvertToManaged(in PrjCallbackDataUnmanaged unmanaged)
    {
        string filePathName = Utf16StringMarshaller.ConvertToManaged(unmanaged.FilePathName)
            ?? throw new NullReferenceException("got an unexpect null in PrjCallbackData.FilePathName");
        string? triggeringProccessImageFileName = Utf16StringMarshaller.ConvertToManaged(unmanaged.TriggeringProccessImageFileName);
        PrjPlaceholderVersionInfo? versionInfo = null;
        if (unmanaged.VersionInfo != null) {
            ref PrjPlaceholderVersionInfoUnmanaged info = ref Unsafe.AsRef<PrjPlaceholderVersionInfoUnmanaged>(unmanaged.VersionInfo);
            versionInfo = PrjPlaceholderVersionInfoMarshaller.ConvertToManaged(in info);
        }

        return new() {
            Size = unmanaged.Size,
            Flags = unmanaged.Flags,
            NamespaceVirtualizationContext = unmanaged.NamespaceVirtualizationContext,
            CammandId = unmanaged.CammandId,
            FileId = unmanaged.FileId,
            DataStreamId = unmanaged.DataStreamId,
            FilePathName = filePathName,
            VersionInfo = versionInfo,
            TriggeringProccessId = unmanaged.TriggeringProccessId,
            TriggeringProccessImageFileName = triggeringProccessImageFileName,
            InstanceContext = (nint) unmanaged.InstanceContext,
        };
    }
}