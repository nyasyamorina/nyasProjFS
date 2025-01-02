using System.Runtime.InteropServices;

namespace nyasProjFS;

using Handle = nint;

internal static partial class Win32Native
{
    [LibraryImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial Handle CreateFile(
        [MarshalAs(UnmanagedType.LPWStr)] string fileNamme,
        uint desiredAccess,
        uint shareMode,
        /* unused */ nint securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        Handle hTemplateFile
    );

    [LibraryImport("kernel32.dll", EntryPoint = "GetVolumePathNameW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static unsafe partial bool GetVolumePathName(
        [MarshalAs(UnmanagedType.LPWStr)] string fileNamme,
        ushort* out_VolumePathName,
        uint bufferLength
    );

    [LibraryImport("kernel32.dll", EntryPoint = "GetVolumeNameForVolumeMountPointW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static unsafe partial bool GetVolumeNameForVolumeMountPoint(
        ushort* volumeMountPoint,
        ushort* out_volumeName,
        uint bufferLength
    );

    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static unsafe partial bool GetFileInformationByHandleEx(
        Handle file,
        FileInfoByHandleClass fileInformationClass,
        void* fileInformation,
        uint bufferSize
    );

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static unsafe partial bool DeviceIoControl(
        Handle hDevice,
        uint ioControlCode,
        /* unused */ nint inBuffer,
        uint inBufferSize,
        void* outBuffer,
        uint outBufferSize,
        /* unused */ uint* bytesReturned,
        /* unused */ nint overlapped
    );

    [LibraryImport("kernel32.dll", SetLastError = true)]
    internal static partial void CloseHandle(Handle handle);

    internal static int GetLastError() => Marshal.GetLastPInvokeError();
}