namespace SweetMock;

using System.Collections.Immutable;
using System.Text;
using Builders;
using Exceptions;
using Microsoft.CodeAnalysis.Text;
using Utils;

[Generator]
public class SweetMockSourceGenerator : IIncrementalGenerator
{
    private record MockTypeWithLocation(ITypeSymbol? Type, AttributeData Attribute);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        AddBaseFiles(context);

        var fixtureAttributes = context.SyntaxProvider
            .ForAttributeWithMetadataName("SweetMock.FixtureAttribute`1", (node, _) => node != null, (syntaxContext, _) => syntaxContext.Attributes)
            .Where(attribute => !attribute.IsDefaultOrEmpty)
            .SelectMany((fixture, _) => fixture)
            .Collect()
            ;

        var mockAttributes = context.SyntaxProvider
            .ForAttributeWithMetadataName("SweetMock.MockAttribute`1", (node, _) => node != null, (syntaxContext, _) => syntaxContext.Attributes)
            .Where(attribute => !attribute.IsDefaultOrEmpty)
            .SelectMany((mock, _) => mock)
            .Collect()
            ;

        var customMockAttributes = context.SyntaxProvider
            .ForAttributeWithMetadataName("SweetMock.MockAttribute`2", (node, _) => node != null, (syntaxContext, _) => syntaxContext.Attributes)
            .Where(attribute => !attribute.IsDefaultOrEmpty)
            .SelectMany((customMock, _) => customMock)
            .Collect()
            ;

        var flattenedAttributes = customMockAttributes.Combine(mockAttributes).Combine(fixtureAttributes);

        context.RegisterSourceOutput(flattenedAttributes, BuildFileList);
    }

    private static void AddBaseFiles(IncrementalGeneratorInitializationContext context)
    {
        foreach (var name in ResourceReader.GetResourceNames(t => t.StartsWith("SweetMock.BaseFiles")))
        {
            var content = ResourceReader.ReadEmbeddedResource(name);
            content = content.Replace("{{SweetMockVersion}}", SourceGeneratorMetadata.Version.ToString());
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                name,
                SourceText.From(content, Encoding.UTF8)));
        }
    }

    private static void BuildFileList(SourceProductionContext spc, ((ImmutableArray<AttributeData> Left, ImmutableArray<AttributeData> Right) Left, ImmutableArray<AttributeData> Right) attributes)
    {
        var fixtures = attributes.Right;
        var mocks = attributes.Left.Right;
        var customMocks = attributes.Left.Left;

        var collectedMocks = CollectedMocks(mocks, fixtures, customMocks);

        AddFixtures(fixtures, spc);
        GenerateMocks(spc, collectedMocks);
    }

    private static void GenerateMocks(SourceProductionContext spc, List<MockTypeWithLocation> collectedMocks)
    {
        var mockBuilder = new MockBuilder();
        var uniqueAttributes = collectedMocks.ToLookup(t => t.Type, a => a.Attribute, SymbolEqualityComparer.Default);
        foreach (var attribute in uniqueAttributes)
        {
            var mockType = attribute.Key;
            MockBuilder.DiagnoseType(mockType, spc, attribute);
            if (MockBuilder.CanBeMocked(mockType))
            {
                try
                {
                    var fileName = TypeToFileName(mockType!);

                    foreach (var file in mockBuilder.BuildFiles((INamedTypeSymbol)mockType!))
                    {
                        spc.AddSource($"{fileName}.{file.Name}.g.cs", SourceText.From(file.Content, Encoding.UTF8));
                    }
                }
                catch (SweetMockException e)
                {
                    spc.AddUnsupportedMethodDiagnostic(attribute, e.Message);
                }
                catch (Exception e)
                {
                    spc.AddUnknownExceptionOccured(attribute, e.Message);
                }
            }
        }
    }

    private static void AddFixtures(ImmutableArray<AttributeData> fixtures, SourceProductionContext spc)
    {
        foreach (var fixture in fixtures.ToLookup(FirstGenericType, a => a, SymbolEqualityComparer.Default).Where(t => t.Key != null))
        {
            try
            {
                var fixtureType = fixture.Key!;
                var prefix = TypeToFileName(fixtureType);
                var factoryFilename = prefix + ".FixtureFactory2.g.cs";
                var code2 = FixtureBuilder.BuildFixtureFactory(fixtureType);

                var fixtureFilename = prefix + ".Fixture2.g.cs";
                var code = FixtureBuilder.BuildFixture(fixtureType);

                spc.AddSource(fixtureFilename, code);
                spc.AddSource(factoryFilename, code2);
            }
            catch (SweetMockException e)
            {
                spc.AddUnsupportedMethodDiagnostic(fixture, e.Message);
            }
            catch (Exception e)
            {
                spc.AddUnknownExceptionOccured(fixture, e.Message);
            }
        }
    }

    private static string TypeToFileName(ISymbol attributeKey) => attributeKey.ToString().Replace("<", "_").Replace(">", "").Replace(", ", "_");

    private static List<MockTypeWithLocation> CollectedMocks(ImmutableArray<AttributeData> mocks, ImmutableArray<AttributeData> fixtures, ImmutableArray<AttributeData> customMocks)
    {
        var collectedMocks = mocks.Select(t => new MockTypeWithLocation(FirstGenericType(t), t)).ToList();
        foreach (var fixture in fixtures)
        {
            var fixtureType = FirstGenericType(fixture);
            if (fixtureType != null)
            {
                var requiredMocks = FixtureBuilder.GetRequiredMocks(fixtureType);
                collectedMocks.AddRange(requiredMocks.Select(requiredMock => new MockTypeWithLocation(requiredMock, fixture)));
            }
        }

        foreach (var customMock in customMocks)
        {
            var customMockType = FirstGenericType(customMock);
            collectedMocks.RemoveAll(t => SymbolEqualityComparer.Default.Equals(customMockType, t.Type));
        }

        return collectedMocks;
    }

    private static ITypeSymbol? FirstGenericType(AttributeData t)
    {
        var firstGenericType = t.AttributeClass?.TypeArguments.FirstOrDefault();
        if (firstGenericType is INamedTypeSymbol symbol)
        {
            return symbol.IsGenericType ? symbol.OriginalDefinition : symbol;
        }

        return null;
    }
}
