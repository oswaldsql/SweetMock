namespace SweetMock.Utils;

public static class SourceGeneratorMetadata
{
    static SourceGeneratorMetadata()
    {
        var assembly = typeof(SweetMockSourceGenerator).Assembly;
        Version = assembly.GetName().Version;
    }

    public static Version Version { get; }
}
