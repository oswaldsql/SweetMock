namespace SweetMock;

using System.Collections.Immutable;
using Builders;
using Exceptions;
using Generation;
using Utils;

[Generator]
public class SweetMockSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
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

    private static void BuildFileList(SourceProductionContext spc, ((ImmutableArray<AttributeData> Left, ImmutableArray<AttributeData> Right) Left, ImmutableArray<AttributeData> Right) attributes)
    {
        var fixtures = attributes.Right;
        var mocks = attributes.Left.Right;
        var customMocks = attributes.Left.Left;

        var collectedMocks = CollectedMocks(mocks, fixtures, customMocks);

        var mockInfos = AddBuildInMocks(spc, collectedMocks).ToList();

        mockInfos.AddRange(AddMocks(spc, collectedMocks));
        mockInfos.AddRange(AddCustomMocks(customMocks));

        AddFixtures(fixtures, spc, mockInfos);
        AddMockFactory(spc, mockInfos);
    }

    private static IEnumerable<MockInfo> AddBuildInMocks(SourceProductionContext spc, List<MockTypeWithLocation> collectedMocks) =>
        BuildInMockBuilder.CreateBuildInMocks(collectedMocks, spc);

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
                MockBuilder.DiagnoseType(mockType, spc, mock.Where(t => t.Explicit).Select(t => t.Attribute));
            }

            if (MockBuilder.CanBeMocked(mockType))
            {
                MockInfo? mockInfo = null;
                try
                {
                    var fileName = TypeToFileName(mockType);

                    var code = mockBuilder.BuildFiles(mockType, out mockInfo);
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

                if (mockInfo != null)
                {
                    yield return mockInfo;
                }
            }
        }
    }

    private static IEnumerable<MockInfo> AddCustomMocks(ImmutableArray<AttributeData> customMocks)
    {
        var lookup = customMocks.ToLookup(FirstGenericType, SymbolEqualityComparer.Default);
        foreach (var mock in lookup)
        {
            var mockType = (INamedTypeSymbol)mock.Key!;
            var implementation = (INamedTypeSymbol)mock.First().AttributeClass!.TypeArguments[1].OriginalDefinition;

            yield return MockInfo.Wrapped(mockType, implementation);
        }
    }

    private static void AddMockFactory(SourceProductionContext spc, List<MockInfo> mocks)
    {
        var code = FactoryClassBuilder.Build(mocks);
        spc.AddSource("SweetMock.Mock.g.cs", code);
    }

    private static void AddFixtures(ImmutableArray<AttributeData> fixtures, SourceProductionContext spc, List<MockInfo> mockInfos)
    {
        var groupAttributes = fixtures.ToLookup(FirstGenericType, NamedSymbolEqualityComparer.Default);

        var fixtureMetadata = groupAttributes.Select(t => new FixtureBuilder.FixtureMetadata(t.Key!, t.ToArray())).ToArray();

        var factoryCode = FixtureFactoryBuilder.BuildFixturesFactory(fixtureMetadata);
        spc.AddSource("SweetMock.Fixture.g.cs", factoryCode);

        foreach (var fixture in fixtureMetadata)
        {
            try
            {
                var fixtureCode = FixtureBuilder.BuildFixture(fixture, mockInfos);

                spc.AddSource(fixture.FileName, fixtureCode);
            }
            catch (SweetMockException e)
            {
                spc.AddUnsupportedMethodDiagnostic(fixture.Attributes, e.Message);
            }
            catch (Exception e)
            {
                spc.AddUnknownExceptionOccured(fixture.Attributes, e.Message);
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

        foreach (var customMockType in customMocks.Select(customMock => FirstGenericType(customMock)))
        {
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
}

internal record MockTypeWithLocation(ITypeSymbol? Type, AttributeData Attribute, bool Explicit);
