using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace nyasProjFS.ProjectedFSLib;

using PrjNamespaceVirtualizationContext = nint;
using PrjDirEntryBufferHandle = nint;

internal static partial class Api
{
    #region Virtualization instance APIs

    /// <param name="options">nullable</param>
    internal static unsafe HResult PrjStartVirtualizing(
        string virtualizationRootPath,
        in PrjCallbacks callbacks,
        nint instanceContext,
        in PrjStartVirtualizingOptions? options,
        out PrjNamespaceVirtualizationContext namespaceVirtualizationContext
    ) {
        // Although it is possible to pass in a null reference using `ref Unsafe.AsRef<T>(null)`,
        // but this approach only works for blittable structures.
        // So here we manually convert the null value of the non-blittable structure.

        namespaceVirtualizationContext = default;
        PrjStartVirtualizingOptionsUnmanaged __options_native = default;
        PrjStartVirtualizingOptionsUnmanaged* optionsPtr = null;
        HResult __retVal = default;
        try
        {
            // Marshal - Convert managed data to native data.
            if (options.HasValue) {
                __options_native = PrjStartVirtualizingOptionsMarsheller.ConvertToUnmanaged(options.Value);
                optionsPtr = &__options_native;
            }
            // Pin - Pin data in preparation for calling the P/Invoke.
            fixed (nint* __namespaceVirtualizationContext_native = &namespaceVirtualizationContext)
            fixed (PrjCallbacks* __callbacks_native = &callbacks)
            fixed (void* __virtualizationRootPath_native = &Utf16StringMarshaller.GetPinnableReference(virtualizationRootPath))
            {
                __retVal = __PInvoke((ushort*)__virtualizationRootPath_native, __callbacks_native, instanceContext, optionsPtr, __namespaceVirtualizationContext_native);
            }
        }
        finally
        {
            // CleanupCallerAllocated - Perform cleanup of caller allocated resources.
            if (optionsPtr != null) { PrjStartVirtualizingOptionsMarsheller.Free(ref __options_native); }
        }

        return __retVal;
        // Local P/Invoke
        [DllImport("ProjectedFSLib.dll", EntryPoint = "PrjStartVirtualizing", ExactSpelling = true)]
        static extern unsafe HResult __PInvoke(ushort* __virtualizationRootPath_native, PrjCallbacks* __callbacks_native, nint __instanceContext_native, PrjStartVirtualizingOptionsUnmanaged* __options_native, nint* __namespaceVirtualizationContext_native);
    }

    [LibraryImport("ProjectedFSLib.dll")]
    internal static partial void PrjStopVirtualizing(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext
    );

    [LibraryImport("ProjectedFSLib.dll")]
    internal static partial HResult PrjClearNegativePathCache(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        out uint totalEntryNumber
    );

    [LibraryImport("ProjectedFSLib.dll")]
    internal static partial HResult PrjGetVirtualizationInstanceInfo(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        out PrjVirtualizationInstanceInfo virtualizationInstanceInfo
    );

    #endregion
    #region Placeholder and File APIs

    /// <param name="versionInfo">nullable</param>
    [LibraryImport("ProjectedFSLib.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial HResult PrjMarkDirectoryAsPlaceholder(
        string rootPathName,
        string? targetPathName,
        in PrjPlaceholderVersionInfo versionInfo,
        in Guid virtualizationInstanceId
    );

    [LibraryImport("ProjectedFSLib.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial HResult PrjWritePlaceholderInfo(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        string destinationFileName,
        in PrjPlaceholderInfo placeholderInfo,
        uint placeholderInfoSize
    );

    /// <param name="extendedInfo">nullable</param>
    internal static unsafe HResult PrjWritePlaceholderInfo2(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        string destinationFileName,
        in PrjPlaceholderInfo placeholderInfo,
        uint placeholderInfoSize,
        in PrjExtendedInfo? extendedInfo
    ) {
        // Although it is possible to pass in a null reference using `ref Unsafe.AsRef<T>(null)`,
        // but this approach only works for blittable structures.
        // So here we manually convert the null value of the non-blittable structure.

        PrjPlaceholderInfoUnmanaged __placeholderInfo_native = default;
        PrjExtendedInfoUnmanaged __extendedInfo_native = default;
        PrjExtendedInfoUnmanaged* extendedInfoPtr = null;
        HResult __retVal = default;
        try
        {
            // Marshal - Convert managed data to native data.
            __placeholderInfo_native = PrjPlaceholderInfoMarshaller.ConvertToUnmanaged(placeholderInfo);
            if (extendedInfo.HasValue) {
                __extendedInfo_native = PrjExtendedInfoMarshaller.ConvertToUnmanaged(extendedInfo.Value);
                extendedInfoPtr = &__extendedInfo_native;
            }
            // Pin - Pin data in preparation for calling the P/Invoke.
            fixed (void* __destinationFileName_native = &Utf16StringMarshaller.GetPinnableReference(destinationFileName))
            {
                __retVal = __PInvoke(namespaceVirtualizationContext, (ushort*) __destinationFileName_native, &__placeholderInfo_native, placeholderInfoSize, extendedInfoPtr);
            }
        }
        finally
        {
            // CleanupCallerAllocated - Perform cleanup of caller allocated resources.
            if (extendedInfoPtr != null) { PrjExtendedInfoMarshaller.Free(ref __extendedInfo_native); }
        }

        return __retVal;
        // Local P/Invoke
        [DllImport("ProjectedFSLib.dll", EntryPoint = "PrjWritePlaceholderInfo2", ExactSpelling = true)]
        static extern unsafe HResult __PInvoke(nint __namespaceVirtualizationContext_native, ushort* __destinationFileName_native, PrjPlaceholderInfoUnmanaged* __placeholderInfo_native, uint __placeholderInfoSize_native, PrjExtendedInfoUnmanaged* __extendedInfo_native);
    }

    [LibraryImport("ProjectedFSLib.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial HResult PrjUpdateFileIfNeeded(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        string destinationFileName,
        in PrjPlaceholderInfo placeholderInfo,
        uint placeholderInfoSize,
        UpdateType updateFlags,
        out UpdateFailureCause failureReason
    );

    [LibraryImport("ProjectedFSLib.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial HResult PrjDeleteFile(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        string destinationFileName,
        UpdateType updateFlags,
        out UpdateFailureCause failureReason
    );

    [LibraryImport("ProjectedFSLib.dll")]
    internal static partial HResult PrjWriteFileData(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        in Guid dataStreamId,
        nint buffer,
        ulong byteOffset,
        uint length
    );

    [LibraryImport("ProjectedFSLib.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial HResult PrjGetOnDiskFileState(
        string destinationFileName,
        out OnDiskFileState fileState
    );

    [LibraryImport("ProjectedFSLib.dll")]
    internal static partial nint PrjAllocateAlignedBuffer(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        ulong size
    );

    [LibraryImport("ProjectedFSLib.dll")]
    internal static partial void PrjFreeAlignedBuffer(
        nint buffer
    );

    #endregion // Placeholder and File APIs
    #region Callback support

    /// <param name="extendedParameters">nullable</param>
    [LibraryImport("ProjectedFSLib.dll")]
    internal static partial HResult PrjCompleteCommand(
        PrjNamespaceVirtualizationContext namespaceVirtualizationContext,
        int commandId,
        HResult completionResult,
        in PrjCompleteCommandExtendedParameters extendedParameters
    );

    #endregion // Callback support
    #region Enumeration APIs

    /// <param name="fileBasicInfo">nullable</param>
    internal static unsafe HResult PrjFillDirEntryBuffer(
        string fileName,
        in PrjFileBasicInfo? fileBasicInfo,
        PrjDirEntryBufferHandle dirEntryBufferHandle
    ) {
        // Although it is possible to pass in a null reference using `ref Unsafe.AsRef<T>(null)`,
        // but this approach only works for blittable structures.
        // So here we manually convert the null value of the non-blittable structure.

        PrjFileBasicInfoUnmanaged __fileBasicInfo_native;
        PrjFileBasicInfoUnmanaged* fileBasicInfoPtr = null;
        HResult __retVal;
        // Marshal - Convert managed data to native data.
        if (fileBasicInfo.HasValue) {
            __fileBasicInfo_native = PrjFileBasicInfoMarshaller.ConvertToUnmanaged(fileBasicInfo.Value);
            fileBasicInfoPtr = &__fileBasicInfo_native;
        }
        // Pin - Pin data in preparation for calling the P/Invoke.
        fixed (void* __fileName_native = &Utf16StringMarshaller.GetPinnableReference(fileName))
        {
            __retVal = __PInvoke((ushort*)__fileName_native, fileBasicInfoPtr, dirEntryBufferHandle);
        }

        return __retVal;
        // Local P/Invoke
        [DllImport("ProjectedFSLib.dll", EntryPoint = "PrjFillDirEntryBuffer", ExactSpelling = true)]
        static extern unsafe HResult __PInvoke(ushort* __fileName_native, PrjFileBasicInfoUnmanaged* __fileBasicInfo_native, nint __dirEntryBufferHandle_native);
    }

    /// <param name="fileBasicInfo">nullable</param>
    /// <param name="extendedInfo">nullable</param>
    internal static unsafe HResult PrjFillDirEntryBuffer2(
        PrjDirEntryBufferHandle dirEntryBufferHandle,
        string fileName,
        in PrjFileBasicInfo? fileBasicInfo,
        in PrjExtendedInfo? extendedInfo
    ) {
        PrjExtendedInfoUnmanaged __extendedInfo_native = default;
        PrjExtendedInfoUnmanaged* extendedInfoPtr = null;
        PrjFileBasicInfoUnmanaged __fileBasicInfo_native = default;
        PrjFileBasicInfoUnmanaged* fileBasicInfoPtr = null;
        HResult __retVal = default;
        try
        {
            // Marshal - Convert managed data to native data.
            if (extendedInfo.HasValue) {
                __extendedInfo_native = PrjExtendedInfoMarshaller.ConvertToUnmanaged(extendedInfo.Value);
                extendedInfoPtr = &__extendedInfo_native;
            }
            if (fileBasicInfo.HasValue) {
                __fileBasicInfo_native = PrjFileBasicInfoMarshaller.ConvertToUnmanaged(fileBasicInfo.Value);
                fileBasicInfoPtr = &__fileBasicInfo_native;
            }
            // Pin - Pin data in preparation for calling the P/Invoke.
            fixed (void* __fileName_native = &Utf16StringMarshaller.GetPinnableReference(fileName))
            {
                __retVal = __PInvoke(dirEntryBufferHandle, (ushort*)__fileName_native, fileBasicInfoPtr, extendedInfoPtr);
            }
        }
        finally
        {
            // CleanupCallerAllocated - Perform cleanup of caller allocated resources.
            if (extendedInfoPtr != null) { PrjExtendedInfoMarshaller.Free(ref __extendedInfo_native); }
        }

        return __retVal;
        // Local P/Invoke
        [DllImport("ProjectedFSLib.dll", EntryPoint = "PrjFillDirEntryBuffer2", ExactSpelling = true)]
        static extern unsafe HResult __PInvoke(nint __dirEntryBufferHandle_native, ushort* __fileName_native, PrjFileBasicInfoUnmanaged* __fileBasicInfo_native, PrjExtendedInfoUnmanaged* __extendedInfo_native);
    }

    [LibraryImport("ProjectedFSLib.dll", StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static partial bool PrjFileNameMatch(
        string fileNameToCheck,
        string pattern
    );

    [LibraryImport("ProjectedFSLib.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial int PrjFileNameCompare(
        string fileName1,
        string fileName2
    );

    [LibraryImport("ProjectedFSLib.dll", StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static partial bool PrjDoesNameContainWildCards(
        string fileName
    );

    #endregion // Enumeration APIs
}