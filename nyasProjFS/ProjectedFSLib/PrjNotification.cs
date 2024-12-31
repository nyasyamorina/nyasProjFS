namespace nyasProjFS.ProjectedFSLib;

public enum PrjNotification : int {
    FileOpened                     = 0x00000002,
    NewFileCreated                 = 0x00000004,
    FileOverwritten                = 0x00000008,
    PreDelete                      = 0x00000010,
    PreRename                      = 0x00000020,
    PreSetHardlink                 = 0x00000040,
    FileRenamed                    = 0x00000080,
    HardlinkCreated                = 0x00000100,
    FileHandleClosedNoModification = 0x00000200,
    FileHandleClosedFileModified   = 0x00000400,
    FileHandleClosedFileDeleted    = 0x00000800,
    FilePreConvertToFull           = 0x00001000,
}