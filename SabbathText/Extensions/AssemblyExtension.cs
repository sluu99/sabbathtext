using System;
using System.IO;
using System.Reflection;

/// <summary>
/// Extension methods for assembly
/// </summary>
public static class AssemblyExtension
{
    /// <summary>
    /// Gets the assembly directory
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns>The assembly directory.</returns>
    public static string GetDirectory(this Assembly assembly)
    {
        string codeBase = assembly.CodeBase;
        UriBuilder uri = new UriBuilder(codeBase);
        string path = Uri.UnescapeDataString(uri.Path);
        return Path.GetDirectoryName(path);
    }
}
