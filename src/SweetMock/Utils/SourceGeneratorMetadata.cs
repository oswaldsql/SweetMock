namespace SweetMock;

using System;

public class SourceGeneratorMetadata
{
    static SourceGeneratorMetadata()
    {
        var assembly = typeof(SweetMockSourceGenerator).Assembly;
        Version = assembly.GetName().Version;
    }

    public static Version Version { get; }
}