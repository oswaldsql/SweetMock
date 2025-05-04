namespace SweetMock.Generation;

using System;

internal static class CodeBuilderExtensions
{
    public static void AddToConfig(this CodeBuilder source, Action<CodeBuilder> action)
    {
        source.Scope("internal partial class Config", action);
    }
}
