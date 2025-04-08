namespace SweetMock.Utils;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class ResourceReader
{
    public static IEnumerable<string> GetResourceNames(Func<string, bool> predicate)
    {
        var assembly = typeof(ResourceReader).Assembly;
        return assembly.GetManifestResourceNames().Where(predicate);
    }

    public static string ReadEmbeddedResource(string resourceName)
    {
        var assembly = typeof(ResourceReader).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
