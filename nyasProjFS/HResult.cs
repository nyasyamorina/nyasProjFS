namespace nyasProjFS;

public enum HResult : int {
    /// <summary>success</summary>
    Ok = 0,
    /// <summary>the data necessary to complete this operation is not yet available</summary>
    Pending = unchecked((int) 0x800703E5),                   // = HRESULT_FROM_WIN32(ERROR_IO_PENDING)
    /// <summary>ran out of memory</summary>
    OutOfMemory = unchecked((int) 0x8007000E),
    /// <summary>the data area passed to a system call is too small</summary>
    InsufficientBuffer = unchecked((int) 0x8007007A),        // = HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER)
    /// <summary>the system cannot find the file specified</summary>
    FileNotFound = unchecked((int) 0x80070002),              // = HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND)
    /// <summary>the provider that supports file system virtualization is temporarily unavailable</summary>
    VirtualizationUnavaliable = unchecked((int) 0x80070171), // = HRESULT_FROM_WIN32(ERROR_FILE_SYSTEM_VIRTUALIZATION_UNAVAILABLE)
    /// <summary>the provider is in an invalid state that prevents it from servicing the callback
    /// (only use this if none of the other error codes is a better match)</summary>
    InternalError = unchecked((int) 0x8007054F),             // = HRESULT_FROM_WIN32(ERROR_INTERNAL_ERROR)

    // the following values are for communicating information to the provider.

    ///<summary>an attempt was made to perform an initialization operation when initialization has already been completed</summary>
    AlreadyInitialized = unchecked((int) 0x800704DF),        // = HRESULT_FROM_WIN32(ERROR_ALREADY_INITIALIZED)
    ///<summary>access is denied</summary>
    AccessDenied = unchecked((int) 0x80070005),              // = HRESULT_FROM_WIN32(ERROR_ACCESS_DENIED)
    ///<summary>an attempt has been made to remove a file or directory that cannot be deleted</summary>
    CannotDelete = unchecked((int) 0xC0000121 | 0x10000000), // = HRESULT_FROM_NT(STATUS_CANNOT_DELETE)
    ///<summary>the directory name is invalid (it may not be a directory)</summary>
    Directory = unchecked((int) 0x8007010B),                 // = HRESULT_FROM_WIN32(ERROR_DIRECTORY)
    ///<summary>the directory is not empty</summary>
    DirNotEmpty = unchecked((int) 0x80070091),               // = HRESULT_FROM_WIN32(ERROR_DIR_NOT_EMPTY)
    ///<summary>invalid handle (it may already be closed)</summary>
    Handle = unchecked((int) 0x80070006),
    ///<summary>one or more arguments are invalid</summary>
    InvalidArg = unchecked((int) 0x80070057),
    ///<summary>the system cannot find the path specified</summary>
    PathNotFound = unchecked((int) 0x80070003),              // = HRESULT_FROM_WIN32(ERROR_PATH_NOT_FOUND)
    ///<summary>the object manager encountered a reparse point while retrieving an object</summary>
    ReparsePointEncountered = unchecked((int) 0x8007112B),   // = HRESULT_FROM_WIN32(ERROR_REPARSE_POINT_ENCOUNTERED)
    ///<summary>the virtualization operation is not allowed on the file in its current state</summary>
    VirtualizationInvalidOp = unchecked((int) 0x80070181),   // = HRESULT_FROM_WIN32(ERROR_FILE_SYSTEM_VIRTUALIZATION_INVALID_OPERATION),
}

// HRESULT_FROM_WIN32(uint x) => x == 0 ? x : ((x & 0x0000FFFF) | 0x80070000)
// ERROR_IO_PENDING                                   = 0x03E5
// ERROR_INSUFFICIENT_BUFFER                          = 0x007A
// ERROR_FILE_NOT_FOUND                               = 0x0002
// ERROR_FILE_SYSTEM_VIRTUALIZATION_UNAVAILABLE       = 0x0171
// ERROR_INTERNAL_ERROR                               = 0x054F
// ERROR_ALREADY_INITIALIZED                          = 0x04DF
// ERROR_ACCESS_DENIED                                = 0x0005
// ERROR_DIRECTORY                                    = 0x010B
// ERROR_DIR_NOT_EMPTY                                = 0x0091
// ERROR_PATH_NOT_FOUND                               = 0x0003
// ERROR_REPARSE_POINT_ENCOUNTERED                    = 0x112B
// ERROR_FILE_SYSTEM_VIRTUALIZATION_INVALID_OPERATION = 0x0181