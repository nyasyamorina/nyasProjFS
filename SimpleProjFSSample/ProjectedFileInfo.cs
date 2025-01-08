namespace SimpleProjFSSample;

public class ProjectedFileInfo(
    string name,
    string fullName,
    long size,
    bool isDirectory,
    DateTime creationTime,
    DateTime lastAccessTime,
    DateTime lastWriteTime,
    DateTime changeTime,
    FileAttributes attributes
) {
    public ProjectedFileInfo(
        string name,
        string fullName,
        long size,
        bool isDirectory
    ) : this(
        name: name,
        fullName: fullName,
        size: size,
        isDirectory: isDirectory,
        creationTime: DateTime.UtcNow,
        lastAccessTime: DateTime.UtcNow,
        lastWriteTime: DateTime.UtcNow,
        changeTime: DateTime.UtcNow,
        attributes: isDirectory ? FileAttributes.Directory : FileAttributes.Normal
    ) { }

    public string Name { get; } = name;
    public string FullName { get; } = fullName;
    public long Size { get; } = isDirectory ? 0 : size;
    public bool IsDirectory { get; } = isDirectory;
    public DateTime CreationTime { get; } = creationTime;
    public DateTime LastAccessTime { get; } = lastAccessTime;
    public DateTime LastWriteTime { get; } = lastWriteTime;
    public DateTime ChangeTime { get; } = changeTime;
    public FileAttributes Attributes { get; } = isDirectory ? (attributes | FileAttributes.Directory) : (attributes & ~FileAttributes.Directory);
}