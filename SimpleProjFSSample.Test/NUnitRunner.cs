﻿using NUnitLite;
using System.Reflection;

namespace SimpleProjFSSample.Test;

class NUnitRunner(string[] args)
{
    private readonly List<string> args = [.. args];

    public string? GetCustomArgWithParam(string arg)
    {
        string? match = this.args.Where(a => a.StartsWith(arg + "=")).SingleOrDefault();
        if (match is null) { return null; }

        this.args.Remove(match);
        return match.Substring(arg.Length + 1);
    }

    public bool HasCustomArg(string arg)
    {
        // We also remove it as we're checking, because nunit wouldn't understand what it means
        return this.args.Remove(arg);
    }

    public int RunTests(ICollection<string> includeCategories, ICollection<string> excludeCategories)
    {
        string filters = GetFiltersArgument(includeCategories, excludeCategories);
        if (filters.Length > 0) {
            this.args.Add("--where");
            this.args.Add(filters);
        }

        DateTime now = DateTime.Now;
        int result = new AutoRun(Assembly.GetEntryAssembly()).Execute(this.args.ToArray());

        Console.WriteLine("Completed test pass in {0}", DateTime.Now - now);
        Console.WriteLine();

        return result;
    }

    private static string GetFiltersArgument(ICollection<string> includeCategories, ICollection<string> excludeCategories)
    {
        string filters = string.Empty;
        if (includeCategories != null && includeCategories.Any()) {
            filters = "(" + string.Join("||", includeCategories.Select(x => $"cat=={x}")) + ")";
        }

        if (excludeCategories != null && excludeCategories.Any()) {
            filters += (filters.Length > 0 ? "&&" : string.Empty) + string.Join("&&", excludeCategories.Select(x => $"cat!={x}"));
        }

        return filters;
    }
}