using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace nyasProjFS.ProjectedFSLib.Deprecated;

using PrjVirtualizationInstanceHandle = nint;

internal static partial class Api
{
    #region Virtualization instance APIs

    [Obsolete("use PrjStartVirtualizing instead of PrjStartVirtualizationInstance")]
    [LibraryImport("ProjectedFSLib.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial HResult PrjStartVirtualizationInstance(
        string virtualizationRootPath,
        in PrjCommandCallbacks callbacks,
        uint flags,
        uint globalNotificationMask,
        uint poolThreadCount,
        uint concurrentThreadCount,
        nint instanceContext,
        out PrjVirtualizationInstanceHandle virtualizationInstanceHandle
    );

    /// <param name="extendedParameters">nullable</param>
    [Obsolete("use PrjStartVirtualizing instead of PrjStartVirtualizationInstanceEx")]
    internal static unsafe HResult PrjStartVirtualizationInstanceEx(
        string virtualizationRootPath,
        in PrjCommandCallbacks callbacks,
        nint instanceContext,
        in VirtualizationInstExtendedParameters? extendedParameters,
        out PrjVirtualizationInstanceHandle virtualizationInstanceHandle
    ) {
        // Although it is possible to pass in a null reference using `ref Unsafe.AsRef<T>(null)`,
        // but this approach only works for blittable structures.
        // So here we manually convert the null value of the non-blittable structure.

        virtualizationInstanceHandle = default;
        VirtualizationInstExtendedParametersUnmanaged __extendedParameters_native = default;
        VirtualizationInstExtendedParametersUnmanaged* extendedParametersPtr = null;
        HResult __retVal = default;
        try
        {
            // Marshal - Convert managed data to native data.
            if (extendedParameters.HasValue) {
                __extendedParameters_native = VirtualizationInstExtendedParametersMarshaller.ConvertToUnmanaged(extendedParameters.Value);
                extendedParametersPtr = &__extendedParameters_native;
            }
            // Pin - Pin data in preparation for calling the P/Invoke.
            fixed (nint* __virtualizationInstanceHandle_native = &virtualizationInstanceHandle)
            fixed (PrjCommandCallbacks* __callbacks_native = &callbacks)
            fixed (void* __virtualizationRootPath_native = &Utf16StringMarshaller.GetPinnableReference(virtualizationRootPath))
            {
                __retVal = __PInvoke((ushort*)__virtualizationRootPath_native, __callbacks_native, instanceContext, &__extendedParameters_native, __virtualizationInstanceHandle_native);
            }
        }
        finally
        {
            // CleanupCallerAllocated - Perform cleanup of caller allocated resources.
            if (extendedParametersPtr != null) { VirtualizationInstExtendedParametersMarshaller.Free(ref __extendedParameters_native); }
        }

        return __retVal;
        // Local P/Invoke
        [DllImport("ProjectedFSLib.dll", EntryPoint = "PrjStartVirtualizationInstanceEx", ExactSpelling = true)]
        static extern unsafe HResult __PInvoke(ushort* __virtualizationRootPath_native, PrjCommandCallbacks* __callbacks_native, nint __instanceContext_native, VirtualizationInstExtendedParametersUnmanaged* __extendedParameters_native, nint* __virtualizationInstanceHandle_native);
    }

    [Obsolete("use PrjStopVirtualizing instead of PrjStopVirtualizationInstance")]
    [LibraryImport("ProjectedFSLib.dll")]
    internal static partial HResult PrjStopVirtualizationInstance(
        PrjVirtualizationInstanceHandle virtualizationInstanceHandle
    );

    [Obsolete("use PrjGetVirtualizationInstanceInfo instead of PrjGetVirtualizationInstanceIdFromHandle")]
    [LibraryImport("ProjectedFSLib.dll")]
    internal static partial HResult PrjGetVirtualizationInstanceIdFromHandle(
        PrjVirtualizationInstanceHandle virtualizationInstanceHandle,
        out Guid virtualizationInstanceId
    );

    #endregion // Virtualization instance APIs
    #region Placeholder and File APIs

    /// <param name="versionInfo">nullable</param>
    [Obsolete("use PrjMarkDirectoryAsPlaceholder instead of PrjConvertDirectoryToPlaceholder")]
    [LibraryImport("ProjectedFSLib.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial HResult PrjConvertDirectoryToPlaceholder(
        string rootPathName,
        string targetPathName,
        in PrjPlaceholderVersionInfo versionInfo,
        uint flags,
        in Guid virtualizationInstanceId
    );

    [Obsolete("use PrjWritePlaceholderInfo instead of PrjWritePlaceholderInformation")]
    [LibraryImport("ProjectedFSLib.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial HResult PrjWritePlaceholderInformation(
        PrjVirtualizationInstanceHandle virtualizationInstanceHandle,
        string destinationFileName,
        in PrjPlaceholderInformation placeholderInformation,
        uint length
    );

    [Obsolete("use PrjUpdateFileIfNeeded instead of PrjUpdatePlaceholderIfNeeded")]
    [LibraryImport("ProjectedFSLib.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial HResult PrjUpdatePlaceholderIfNeeded(
        PrjVirtualizationInstanceHandle virtualizationInstanceHandle,
        string destinationFileName,
        in PrjPlaceholderInformation placeholderInformation,
        uint length,
        UpdateType updateFlags,
        out UpdateFailureCause failureReason
    );

    [Obsolete("Use PrjWriteFileData instead of PrjWriteFile")]
    [LibraryImport("ProjectedFSLib.dll")]
    internal static partial HResult PrjWriteFile(
        PrjVirtualizationInstanceHandle virtualizationInstanceHandle,
        in Guid streamId,
        nint buffer,
        ulong byteOffset,
        uint length
    );

    #endregion // Placeholder and File APIs
    #region Callback support

    [Obsolete("PrjCommandCallbacksInit is deprecated and will not exist in future versions of Windows")]
    [LibraryImport("ProjectedFSLib.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial HResult PrjCommandCallbacksInit(
        uint callbackSize,
        out PrjCommandCallbacks callbacks
    );

    #endregion // Callback support
}