namespace SweetMock;

using System.Collections.Immutable;
using System.Text;
using Builders;
using Exceptions;
using Generation;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Utils;

[Generator]
public class SweetMockSourceGenerator : IIncrementalGenerator
{
    private static readonly SymbolDisplayFormat Format = new();

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

        var mockInfos = AddMocks(spc, collectedMocks).ToList();
        mockInfos.AddRange(AddCustomMocks(spc, customMocks));
        AddFixtures(fixtures, spc, mockInfos);
        AddMockFactory(spc, mockInfos);
    }

    private static void AddMockFactory(SourceProductionContext spc, List<MockInfo> mocks)
    {
        var code = FactoryClassBuilder.Build(mocks);
        spc.AddSource("SweetMock.Mock.g.cs", code);
    }

    private static IEnumerable<MockInfo> AddCustomMocks(SourceProductionContext spc, ImmutableArray<AttributeData> customMocks)
    {
        var lookup = customMocks.ToLookup(data => FirstGenericType(data), SymbolEqualityComparer.Default);
        foreach (var mock in lookup)
        {
            var mockType = (INamedTypeSymbol)mock.Key!;

            var implementation = (INamedTypeSymbol)mock.First().AttributeClass!.TypeArguments[1].OriginalDefinition;
            yield return new(mockType, implementation.ContainingNamespace + "." + implementation.ToDisplayString(Format), MockKind.Wrapper, implementation);
        }
    }

    private static IEnumerable<MockInfo> AddMocks(SourceProductionContext spc, List<MockTypeWithLocation> collectedMocks)
    {
        var mockBuilder = new MockBuilder();
        var requestedMocks = collectedMocks.ToLookup(t => t.Type, a => a, SymbolEqualityComparer.Default);
        foreach (var mock in requestedMocks)
        {
            var mockType = (INamedTypeSymbol)mock.Key!;
            var attributes = mock.Select(t => t.Attribute).ToArray();

            if (mock.Any(t => t.Explicit))
            {
                MockBuilder.DiagnoseType(mockType, spc, mock.Where(t => t.Explicit == true).Select(t => t.Attribute));
            }

            if (MockBuilder.CanBeMocked(mockType))
            {
                try
                {
                    var fileName = TypeToFileName(mockType);

                    var code = mockBuilder.BuildFiles(mockType);
                    spc.AddSource($"{fileName}.g.cs", code);
                }
                catch (SweetMockException e)
                {
                    spc.AddUnsupportedMethodDiagnostic(attributes, e.Message);
                }
                catch (Exception e)
                {
                    spc.AddUnknownExceptionOccured(attributes, e.Message);
                }

                yield return new(mockType, mockType.ContainingNamespace + ".MockOf_" + mockType.ToDisplayString(Format), MockKind.Generated);
            }
        }
    }

    private static void AddFixtures(ImmutableArray<AttributeData> fixtures, SourceProductionContext spc, List<MockInfo> mockInfos)
    {
        var fix = fixtures.ToLookup(FirstGenericType, a => a, SymbolEqualityComparer.Default);

        var factoryCode = FixtureBuilder.BuildFixturesFactory(fix.Select(t => t.Key).OfType<INamedTypeSymbol>());
        spc.AddSource("SweetMock.Fixture.g.cs", factoryCode);

        foreach (var fixture in fix)
        {
            try
            {
                var fixtureType = fixture.Key!;
                var prefix = TypeToFileName(fixtureType);

                var fixtureFilename = prefix + ".Fixture.g.cs";
                var fixtureCode = FixtureBuilder.BuildFixture(fixtureType, mockInfos);

                spc.AddSource(fixtureFilename, fixtureCode);
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

    private static string TypeToFileName(ISymbol attributeKey) => attributeKey.ToCRef().Replace("<", "_").Replace(">", "").Replace(", ", "_");

    private static List<MockTypeWithLocation> CollectedMocks(ImmutableArray<AttributeData> mocks, ImmutableArray<AttributeData> fixtures, ImmutableArray<AttributeData> customMocks)
    {
        var collectedMocks = mocks.Select(t => new MockTypeWithLocation(FirstGenericType(t), t, true)).ToList();
        foreach (var fixture in fixtures)
        {
            var fixtureType = FirstGenericType(fixture);
            if (fixtureType != null)
            {
                var requiredMocks = FixtureBuilder.GetRequiredMocks(fixtureType);
                collectedMocks.AddRange(requiredMocks.Select(requiredMock => new MockTypeWithLocation(requiredMock, fixture, false)));
            }
        }

        foreach (var customMock in customMocks)
        {
            var customMockType = FirstGenericType(customMock);
            collectedMocks.RemoveAll(t => SymbolEqualityComparer.Default.Equals(customMockType, t.Type));
        }

        return collectedMocks;
    }

    private static INamedTypeSymbol? FirstGenericType(AttributeData t)
    {
        var firstGenericType = t.AttributeClass?.TypeArguments.FirstOrDefault();
        if (firstGenericType is INamedTypeSymbol symbol)
        {
            return symbol.IsGenericType ? symbol.OriginalDefinition : symbol;
        }

        return null;
    }

    private record MockTypeWithLocation(ITypeSymbol? Type, AttributeData Attribute, bool Explicit);
}

public record MockInfo(INamedTypeSymbol Target, string MockClass, MockKind Kind, INamedTypeSymbol? Implementation = null);

public enum MockKind
{
    Generated,
    Wrapper,
    Direct
}
