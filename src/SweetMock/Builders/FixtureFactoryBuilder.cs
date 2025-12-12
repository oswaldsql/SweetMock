namespace SweetMock.Builders;

using Generation;
using Utils;

public static class FixtureFactoryBuilder
{
    public static string BuildFixturesFactory(FixtureBuilder.FixtureMetadata[] source)
    {
        var fileScope = new CodeBuilder();

        fileScope.AddFileHeader()
            .Nullable()
            .Add("namespace SweetMock;")
            .AddGeneratedCodeAttrib()
            .Scope("internal static class Fixture", classScope => classScope
                .AddMultiple(source, BuildForFixture));

        return fileScope.ToString();
    }

    private static void BuildForFixture(CodeBuilder classScope, FixtureBuilder.FixtureMetadata symbol) =>
        classScope
            .Documentation(doc => doc
                .Summary($"Represents a test fixture designed for the {symbol.ToSeeCRef} class, leveraging mocked dependencies for unit testing.")
                .Parameter("config", "An optional configuration action to customize the mocked dependencies or fixture setup.")
                .Returns($"Returns a fixture object configured for testing the {symbol.ToSeeCRef} class.")
            )
            .Scope($"public static {symbol.Namespace}.FixtureFor_{symbol.Name}{symbol.Generics} {symbol.Name}{symbol.Generics}(System.Action<{symbol.Namespace}.FixtureFor_{symbol.Name}{symbol.Generics}.FixtureConfig>? config = null){symbol.Constraints}", methodScope =>
                methodScope
                    .Add($"var result = new {symbol.Namespace}.FixtureFor_{symbol.Name}{symbol.Generics}(config);")
                    .Add("return result;"));
}
