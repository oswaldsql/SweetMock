namespace SweetMock;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Builders;
using Microsoft.CodeAnalysis;
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

        var mockAttributes = context.SyntaxProvider
            .ForAttributeWithMetadataName("SweetMock.MockAttribute`1", (syntaxNode, _) => syntaxNode != null, GetAttributes)
            .Where(t => t is not null)
            .SelectMany((enumerable, _) => enumerable)
            .Collect();

        context.RegisterSourceOutput(mockAttributes, GenerateCode);
    }

    private void GenerateCode(SourceProductionContext context, ImmutableArray<AttributeData> attributes)
    {
        var mockBuilder = new MockBuilder();
        var uniqueAttributes = attributes.ToLookup(FirstGenericType, a => a, SymbolEqualityComparer.Default).Where(t => t.Key != null);
        foreach (var attribute in uniqueAttributes)
        {
            var fileName = attribute.Key.ToString().Replace("<", "_").Replace(">", "").Replace(", ", "_");

            foreach (var file in mockBuilder.BuildFiles((INamedTypeSymbol)attribute.Key))
            {
                context.AddSource($"{fileName}.{file.Name}.g.cs", SourceText.From(file.Content, Encoding.UTF8));
            }
        }
    }

    private static ITypeSymbol? FirstGenericType(AttributeData t)
    {
        var firstGenericType = t.AttributeClass?.TypeArguments.OfType<INamedTypeSymbol>().FirstOrDefault();
        return firstGenericType?.IsGenericType == true ? firstGenericType.OriginalDefinition : firstGenericType;
    }

    private IEnumerable<AttributeData> GetAttributes(GeneratorAttributeSyntaxContext arg1, CancellationToken arg2)
    {
        return arg1.Attributes;
    }
}