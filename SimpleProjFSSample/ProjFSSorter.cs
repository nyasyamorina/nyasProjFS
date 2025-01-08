using System.Collections.Generic;
using nyasProjFS;

namespace SimpleProjFSSample;

/// <summary>
/// Implements IComparer using <see cref="Utils.FileNameCompare(string, string)"/>.
/// </summary>
internal class ProjFSSorter : Comparer<string>
{
    public override int Compare(string? x, string? y) => Utils.FileNameCompare(x ?? "", y ?? "");
}