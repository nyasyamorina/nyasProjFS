namespace nyasProjFS;

using System.Runtime.InteropServices;
using PrjNamespaceVirtualizationContext = nint;

/// <summary>
/// Helper class to ensure correct alignment when providing file contents for a placeholder.
/// </summary>
/// <remarks>
/// <para>
/// The provider does not instantiate this class directly.  It uses the
/// <c>ProjFS.VirtualizationInstance.CreateWriteBuffer</c> method to obtain a properly initialized
/// instance of this class.
/// </para>
/// <para>
/// The <c>ProjFS.VirtualizationInstance.WriteFileData</c> method requires a data buffer containing
/// file data for a placeholder so that ProjFS can convert the placeholder to a hydrated placeholder
/// (see <c>ProjFS.OnDiskFileState</c> for a discussion of file states).  Internally ProjFS uses
/// the user's FILE_OBJECT to write this data to the file.  Because the user may have opened the
/// file for unbuffered I/O, and unbuffered I/O imposes certain alignment requirements, this
/// class is provided to abstract out those details.
/// </para>
/// <para>
/// When the provider starts its virtualization instance, the <c>VirtualizationInstance</c> class
/// queries the alignment requirements of the underlying physical storage device and uses this
/// information to return a properly-initialized instance of this class from its <c>CreateWriteBuffer</c>
/// method.
/// </para>
/// </remarks>
public class WriteBuffer : IWriteBuffer
{
    internal unsafe WriteBuffer(uint bufferSize, uint alignment)
    {
        _buffer = (nint) NativeMemory.AlignedAlloc(bufferSize, alignment);
        if (_buffer == 0) { throw new OutOfMemoryException("unable to allocate WriteBuffer"); }
        _namespaceCtx = PrjNamespaceVirtualizationContext.Zero;
        _stream = new((byte*) _buffer, bufferSize, bufferSize, FileAccess.Write);
    }

    internal unsafe WriteBuffer(uint bufferSize, PrjNamespaceVirtualizationContext namespaceCtx)
    {
        _buffer = ApiHelper.PrjAllocateAlignedBuffer!(namespaceCtx, bufferSize);
        if (_buffer == 0) { throw new OutOfMemoryException("unable to allocate WriteBuffer"); }
        _namespaceCtx = namespaceCtx;
        _stream = new((byte*) _buffer, bufferSize, bufferSize, FileAccess.Write);
    }

    /// <summary>frees the internal buffer</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>gets the allocated length of the buffer</summary>
    public virtual long Length { get => _stream.Length; }
    /// <summary>gets a <see cref="UnmanagedMemoryStream"/> representing the internal buffer</summary>
    public virtual UnmanagedMemoryStream Stream { get => _stream; }
    /// <summary>gets a pointer to the internal buffer</summary>
    public virtual nint Pointer { get => (nint) _buffer; }

    ~WriteBuffer()
    {
        if (_namespaceCtx != PrjNamespaceVirtualizationContext.Zero) {
            ApiHelper.PrjFreeAlignedBuffer!((nint) _buffer);
        }
        else unsafe {
            NativeMemory.AlignedFree((void*) _buffer); // in <corecrt_malloc.h>
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing) {
            _stream.Dispose();
        }
    }

    private readonly UnmanagedMemoryStream _stream;
    private readonly nint _buffer;
    private readonly PrjNamespaceVirtualizationContext _namespaceCtx;
}