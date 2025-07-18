namespace SweetMock;

using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Builders;
using Exceptions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Utils;

[Generator]
public class SweetMockSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        foreach (var name in ResourceReader.GetResourceNames(t => t.StartsWith("SweetMock.BaseFiles")))
        {
            var content = ResourceReader.ReadEmbeddedResource(name);
            content = content.Replace("{{SweetMockVersion}}", SourceGeneratorMetadata.Version.ToString());
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                name,
                SourceText.From(content, Encoding.UTF8)));
        }

        var fixtureAttributes = context.SyntaxProvider
            .ForAttributeWithMetadataName("SweetMock.FixtureAttribute`1", (syntaxNode, _) => syntaxNode != null, GetAttributes)
            .Where(t => t is not null)
            .SelectMany((enumerable, _) => enumerable)
            .Collect();

        context.RegisterSourceOutput(fixtureAttributes, (c, datas) =>
        {
            foreach (var ad in datas.ToLookup(FirstGenericType, a => a, SymbolEqualityComparer.Default))
            {
                try
                {
                    if (ad.Key != null)
                    {
                        var prefix = TypeToFileName(ad.Key);
                        var factoryFilename = prefix + ".FixtureFactory.g.cs";
                        var code2 = FixtureBuilder.BuildFixtureFactory(ad.Key);

                        var fixtureFilename = prefix + ".Fixture.g.cs";
                        var code = FixtureBuilder.BuildFixture(ad.Key);

                        c.AddSource(fixtureFilename, code);
                        c.AddSource(factoryFilename, code2);
                    }
                }
                catch (SweetMockException e)
                {
                    c.AddUnsupportedMethodDiagnostic(ad, e.Message);
                }
                catch (Exception e)
                {
                    c.AddUnknownExceptionOccured(ad, e.Message);
                }
            }
        });

        var mockAttributes = context.SyntaxProvider
            .ForAttributeWithMetadataName("SweetMock.MockAttribute`1", (syntaxNode, _) => syntaxNode != null, GetAttributes)
            .Where(t => t is not null)
            .SelectMany((enumerable, _) => enumerable)
            .Collect();

        context.RegisterSourceOutput(mockAttributes, GenerateMocks);
    }

    private static void GenerateMocks(SourceProductionContext context, ImmutableArray<AttributeData> attributes)
    {
        var mockBuilder = new MockBuilder();
        var uniqueAttributes = attributes.ToLookup(FirstGenericType, a => a, SymbolEqualityComparer.Default);
        foreach (var attribute in uniqueAttributes)
        {
            if (!ValidateType(attribute.Key, context, attribute))
            {
                continue;
            }

            try
            {
                {
                    var attributeKey = attribute.Key;
                    var fileName = TypeToFileName(attributeKey);

                    foreach (var file in mockBuilder.BuildFiles((INamedTypeSymbol)attribute.Key))
                    {
                        context.AddSource($"{fileName}.{file.Name}.g.cs", SourceText.From(file.Content, Encoding.UTF8));
                    }
                }
            }
            catch (SweetMockException e)
            {
                context.AddUnsupportedMethodDiagnostic(attributes, e.Message);
            }
            catch (Exception e)
            {
                context.AddUnknownExceptionOccured(attributes, e.Message);
            }
        }
    }

    private static string TypeToFileName(ISymbol? attributeKey) => attributeKey!.ToString().Replace("<", "_").Replace(">", "").Replace(", ", "_");

    private static readonly HashSet<TypeKind> ValidKinds = [TypeKind.Class, TypeKind.Interface];
    private static bool ValidateType(ISymbol? symbol, SourceProductionContext context, IEnumerable<AttributeData> attributes)
    {
        if (symbol is null)
        {
            context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must be a class or interface.");
            return false;
        }

        if (symbol is INamedTypeSymbol target)
        {
            if (!ValidKinds.Contains(target.TypeKind))
            {
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must be a class or interface.");
                return false;
            }

            if (target.IsRecord)
            {
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must not be a record type.");
                return false;
            }

            if (target.DeclaredAccessibility == Accessibility.Private)
            {
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must not be a private class.");
                return false;
            }

            if (target.IsSealed)
            {
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must not be a sealed class.");
                return false;
            }

            if (target.IsStatic)
            {
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking target must not be a static class.");
                return false;
            }

            if (target.TypeKind == TypeKind.Class && target.Constructors.All(t => t.DeclaredAccessibility == Accessibility.Private))
            {
                context.AddUnsupportedTargetDiagnostic(attributes, "Mocking classes must have at least one accessible constructor.");
                return false;
            }

            if (target.GetMembers().Length == 0)
            {
                context.AddUnintendedTargetDiagnostic(attributes, "Mocking target contains no members.");
            }
        }

        return true;
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

    private static IEnumerable<AttributeData> GetAttributes(GeneratorAttributeSyntaxContext arg1, CancellationToken arg2) =>
        arg1.Attributes;
}
