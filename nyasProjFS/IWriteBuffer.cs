namespace nyasProjFS;

/// <summary>
/// Interface to allow for easier unit testing of a virtualization provider.
/// </summary>
/// <remarks>
/// This class defines the interface implemented by the <c>Microsoft.Windows.ProjFS.WriteBuffer</c>
/// class.  This interface class is provided for use by unit tests to mock up the interface to ProjFS.
/// </remarks>
public interface IWriteBuffer : IDisposable
{
    /// <summary>
    /// When overridden in a derived class, gets the allocated length of the buffer.
    /// </summary>
    public long Length { get; }

    /// <summary>
    /// When overridden in a derived class, gets a <see cref="UnmanagedMemoryStream"/>
    /// representing the internal buffer.
    /// </summary>
    public UnmanagedMemoryStream Stream { get; }

    /// <summary>
    /// When overridden in a derived class, gets a pointer to the internal buffer.
    /// </summary>
    public nint Pointer { get; }
}