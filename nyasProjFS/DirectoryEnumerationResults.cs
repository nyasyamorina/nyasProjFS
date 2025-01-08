using nyasProjFS.ProjectedFSLib;

namespace nyasProjFS;

using PrjDirEntryBufferHandle = nint;

///<summary>Helper class for providing the results of a directory enumeration.</summary>
///<remarks>
/// ProjFS passes an instance of this class to the provider in the <paramref name="result"/>
/// parameter of its implementation of a <c>GetDirectoryEnumerationCallback</c> delegate.  The provider
/// calls one of its <c>Add</c> methods for each item in the enumeration
/// to add it to the result set.
///</remarks>
public class DirectoryEnumerationResults : IDirectoryEnumerationResults
{
    internal DirectoryEnumerationResults(PrjDirEntryBufferHandle bufferHandle)
    {
        _dirEntryBufferHandle = bufferHandle;
    }

    // Provides access to the native handle to the directory entry buffer.
    // Used internally by VirtualizationInstance.CompleteCommand(int, IDirectoryEnumerationResults).
    internal PrjDirEntryBufferHandle DirEntryBufferHandle => _dirEntryBufferHandle;

    /// <summary>Adds one entry to a directory enumeration result.</summary>
    /// <remarks>
    ///     <para>
    ///     In its implementation of a <c>GetDirectoryEnumerationCallback</c> delegate the provider
    ///     calls this method for each matching file or directory in the enumeration.
    ///     </para>
    ///     <para>
    ///     If the provider calls this <c>Add</c> overload, then the timestamps reported to the caller
    ///     of the enumeration are the current system time.  If the provider wants the caller to see other
    ///     timestamps, it must use the other <c>Add</c> overload.
    ///     </para>
    ///     <para>
    ///     If this method returns <c>false</c>, the provider returns <see cref="HResult.Ok"/> and waits for
    ///     the next <c>GetDirectoryEnumerationCallback</c>.  Then it resumes filling the enumeration with
    ///     the entry it was trying to add when it got <c>false</c>.
    ///     </para>
    ///     <para>
    ///     If the method returns <c>false</c> for the first file or directory in the enumeration, the
    ///     provider returns <see cref="HResult.InsufficientBuffer"/> from the <c>GetDirectoryEnumerationCallback</c>
    ///     method.
    ///     </para>
    /// </remarks>
    /// <param name="fileName">The name of the file or directory.</param>
    /// <param name="fileSize">The size of the file.</param>
    /// <param name="isDirectory"><c>true</c> if this item is a directory, <c>false</c> if it is a file.</param>
    /// <returns>
    ///     <para>
    ///     <c>true</c> if the entry was successfully added to the enumeration buffer, <c>false</c> otherwise.
    ///     </para>
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="fileName"/> is null or empty.
    /// </exception>
    public virtual bool Add(string fileName, long fileSize, bool isDirectory)
    {
        ValidateFileName(fileName);

        PrjFileBasicInfo? basicInfo = new PrjFileBasicInfo {
            IsDirectory = isDirectory,
            FileSize = fileSize,
        };
        HResult hr = Api.PrjFillDirEntryBuffer(fileName, in basicInfo, _dirEntryBufferHandle);
        return hr.IsSuccess();
    }

    /// <summary>Adds one entry to a directory enumeration result.</summary>
    /// <remarks>
    ///     <para>
    ///     In its implementation of a <c>GetDirectoryEnumerationCallback</c> delegate the provider
    ///     calls this method for each matching file or directory in the enumeration.
    ///     </para>
    ///     <para>
    ///     If this method returns <c>false</c>, the provider returns <see cref="HResult.Ok"/> and waits for
    ///     the next <c>GetDirectoryEnumerationCallback</c>.  Then it resumes filling the enumeration with
    ///     the entry it was trying to add when it got <c>false</c>.
    ///     </para>
    ///     <para>
    ///     If the method returns <c>false</c> for the first file or directory in the enumeration, the
    ///     provider returns <see cref="HResult.InsufficientBuffer"/> from the <c>GetDirectoryEnumerationCallback</c>
    ///     method.
    ///     </para>
    /// </remarks>
    /// <param name="fileName">The name of the file or directory.</param>
    /// <param name="fileSize">The size of the file.</param>
    /// <param name="isDirectory"><c>true</c> if this item is a directory, <c>false</c> if it is a file.</param>
    /// <param name="fileAttributes">The file attributes.</param>
    /// <param name="creationTime">The time the file was created.</param>
    /// <param name="lastAccessTime">The time the file was last accessed.</param>
    /// <param name="lastWriteTime">The time the file was last written to.</param>
    /// <param name="changeTime">The time the file was last changed.</param>
    /// <returns>
    ///     <para>
    ///     <c>true</c> if the entry was successfully added to the enumeration buffer, <c>false</c> otherwise.
    ///     </para>
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="fileName"/> is null or empty.
    /// </exception>
    public virtual bool Add(
        string fileName,
        long fileSize,
        bool isDirectory,
        FileAttributes fileAttributes,
        DateTime creationTime,
        DateTime lastAccessTime,
        DateTime lastWriteTime,
        DateTime changeTime
    ) {
        ValidateFileName(fileName);

        PrjFileBasicInfo? basicInfo = BuildFileBasicInfo(fileSize, isDirectory, fileAttributes, creationTime, lastAccessTime, lastWriteTime, changeTime);
        HResult hr = Api.PrjFillDirEntryBuffer(fileName, in basicInfo, _dirEntryBufferHandle);
        return hr.IsSuccess();
    }

    /// <summary>Adds one entry to a directory enumeration result.</summary>
    /// <remarks>
    ///     <para>
    ///     In its implementation of a <c>GetDirectoryEnumerationCallback</c> delegate the provider
    ///     calls this method for each matching file or directory in the enumeration.
    ///     </para>
    ///     <para>
    ///     If this method returns <c>false</c>, the provider returns <see cref="HResult.Ok"/> and waits for
    ///     the next <c>GetDirectoryEnumerationCallback</c>.  Then it resumes filling the enumeration with
    ///     the entry it was trying to add when it got <c>false</c>.
    ///     </para>
    ///     <para>
    ///     If the method returns <c>false</c> for the first file or directory in the enumeration, the
    ///     provider returns <see cref="HResult.InsufficientBuffer"/> from the <c>GetDirectoryEnumerationCallback</c>
    ///     method.
    ///     </para>
    /// </remarks>
    /// <param name="fileName">The name of the file or directory.</param>
    /// <param name="fileSize">The size of the file.</param>
    /// <param name="isDirectory"><c>true</c> if this item is a directory, <c>false</c> if it is a file.</param>
    /// <param name="fileAttributes">The file attributes.</param>
    /// <param name="creationTime">The time the file was created.</param>
    /// <param name="lastAccessTime">The time the file was last accessed.</param>
    /// <param name="lastWriteTime">The time the file was last written to.</param>
    /// <param name="changeTime">The time the file was last changed.</param>
    /// <param name="symlinkTargetOrNull">Specifies the symlink target path if the file is a symlink.</param>
    /// <returns>
    ///     <para>
    ///     <c>true</c> if the entry was successfully added to the enumeration buffer, <c>false</c> otherwise.
    ///     </para>
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="fileName"/> is null or empty.
    /// </exception>
    public virtual bool Add(
        string fileName,
        long fileSize,
        bool isDirectory,
        FileAttributes fileAttributes,
        DateTime creationTime,
        DateTime lastAccessTime,
        DateTime lastWriteTime,
        DateTime changeTime,
        string? symlinkTargetOrNull
    ) {
        // This API is supported in Windows 10 version 2004 and above.
        if (!ApiHelper.HasAdditionalApi) {
            throw new NotImplementedException("PrjFillDirEntryBuffer2 is not supported in this version of Windows.");
        }
        ValidateFileName(fileName);

        PrjFileBasicInfo? basicInfo = BuildFileBasicInfo(fileSize, isDirectory, fileAttributes, creationTime, lastAccessTime, lastWriteTime, changeTime);

        PrjExtendedInfo? extendedInfo = null;
        if (symlinkTargetOrNull is not null) {
            PrjExtendedInfo tmp = default;
            tmp.InfoType = PrjExtInfoType.Symlink;
            tmp.Symlink.TargetName = symlinkTargetOrNull;
            extendedInfo = tmp;
        }

        HResult hr = Api.PrjFillDirEntryBuffer2(_dirEntryBufferHandle, fileName, in basicInfo, in extendedInfo);
        return hr.IsSuccess();
    }

    private PrjDirEntryBufferHandle _dirEntryBufferHandle;

    private static void ValidateFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) {
            throw new ArgumentException(string.Format(System.Globalization.CultureInfo.InvariantCulture, "fileName cannot be empty."));
        }
    }

    private static PrjFileBasicInfo? BuildFileBasicInfo(
        long fileSize,
        bool isDirectory,
        FileAttributes fileAttributes,
        DateTime creationTime,
        DateTime lastAccessTime,
        DateTime lastWriteTime,
        DateTime changeTime
    ) {
        PrjFileBasicInfo basicInfo = new() {
            FileAttributes = (uint) fileAttributes,
            IsDirectory = isDirectory,
            FileSize = fileSize,
        };

        if (creationTime != DateTime.MinValue) {
            basicInfo.CreationTime = creationTime.ToFileTime();
        }
        if (lastAccessTime != DateTime.MinValue) {
            basicInfo.LastAccessTime = lastAccessTime.ToFileTime();
        }
        if (lastWriteTime != DateTime.MinValue) {
            basicInfo.LastWriteTime = lastWriteTime.ToFileTime();
        }
        if (changeTime != DateTime.MinValue) {
            basicInfo.ChangeTime = changeTime.ToFileTime();
        }

        return basicInfo;
    }
}