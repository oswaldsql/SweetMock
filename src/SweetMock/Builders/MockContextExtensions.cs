namespace SweetMock.Builders;

public static class MockContextExtensions
{
    extension(ITypeSymbol type)
    {
        internal bool IsGenericTask() =>
            type.ToString().StartsWith("System.Threading.Tasks.Task<") &&
            ((INamedTypeSymbol)type).TypeArguments.Length > 0;

        internal bool IsGenericValueTask() =>
            type.ToString().StartsWith("System.Threading.Tasks.ValueTask<") &&
            ((INamedTypeSymbol)type).TypeArguments.Length > 0;
    }
}
