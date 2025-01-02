namespace nyasProjFS.ProjectedFSLib;

public unsafe struct PrjPlaceholderVersionInfo
{
    public fixed byte ProviderId[PrjPlaceholderId.Length];
    public fixed byte ContentId[PrjPlaceholderId.Length];
}