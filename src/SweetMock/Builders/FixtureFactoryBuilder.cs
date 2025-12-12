namespace SweetMock.Builders;

using Generation;
using Utils;

public static class FixtureFactoryBuilder
{
    public static string BuildFixturesFactory(IEnumerable<INamedTypeSymbol> source)
    {
        var fileScope = new CodeBuilder();
        fileScope.AddFileHeader()
            .Nullable()
            .Add("namespace SweetMock;")
            .AddGeneratedCodeAttrib()
            .Scope("internal static class Fixture", classScope =>
            {
                foreach (var symbol in source)
                {
                    BuildForFixture(classScope, symbol);
                }
            });

        return fileScope.ToString();
    }

    private static void BuildForFixture(CodeBuilder classScope, INamedTypeSymbol symbol)
    {
        var generics = symbol.GetTypeGenerics();
        var constraints = symbol.ToConstraints();

        classScope
            .Documentation(doc => doc
                .Summary($"Represents a test fixture designed for the {symbol.ToSeeCRef()} class, leveraging mocked dependencies for unit testing.")
                .Parameter("config", "An optional configuration action to customize the mocked dependencies or fixture setup.")
                .Returns($"Returns a fixture object configured for testing the {symbol.ToSeeCRef()} class.")
            )
            .Scope($"public static {symbol.ContainingNamespace}.FixtureFor_{symbol.Name}{generics} {symbol.Name}{generics}(System.Action<{symbol.ContainingNamespace}.FixtureFor_{symbol.Name}{generics}.FixtureConfig>? config = null){constraints}", methodScope =>
                methodScope
                    .Add($"var result = new {symbol.ContainingNamespace}.FixtureFor_{symbol.Name}{generics}(config);")
                    .Add("return result;"));
    }
}
