namespace SweetMock;

using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class ClassPatternMatcher : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        return;

        // Step 1: Collect classes in the current project
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax, // Look for class declarations
                transform: static (syntaxContext, _) =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)syntaxContext.Node;
                    var semanticModel = syntaxContext.SemanticModel;
                    var symbol = semanticModel.GetDeclaredSymbol(classDeclaration);

                    // Filter matching the pattern, e.g., class name starts with 'A'
                    return symbol is not null && symbol.Name.StartsWith("A") ? symbol : null; // Replace "A" with your desired pattern
                })
            .Where(symbol => symbol != null);

        // Step 2: Collect classes from referenced dependencies
        var externalClasses = context.CompilationProvider.Select((compilation, _) =>
        {
            var result = new List<ITypeSymbol>();
            foreach (var reference in compilation.References)
            {
                var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
                if (assemblySymbol is null) continue;

                var assembly = compilation.GetAssemblyOrModuleSymbol(compilation.GetMetadataReference(assemblySymbol)) as IAssemblySymbol;
                if (assembly is null) continue;

                // Add types from referenced assemblies
                foreach (var type in assembly.GlobalNamespace.GetNamespaceTypes())
                {
                    if (type.Name.StartsWith("A")) // Replace "A" with your desired pattern
                    {
                        result.Add(type);
                    }
                }
            }
            return result;
        });

        // Combine both collections
        var allClasses = classDeclarations.Collect().Combine(externalClasses);

        // Generate or log results
        context.RegisterSourceOutput(allClasses, (sourceProductionContext, items) =>
        {
            var (projectClasses, dependencyClasses) = items;

            var result = new StringBuilder();

            // Project classes
            result.AppendLine("// Classes in the current project:");
            foreach (var classSymbol in projectClasses)
            {
                result.AppendLine($"// {classSymbol!.ToDisplayString()}");
            }

            // External classes
            result.AppendLine("// Classes in dependencies:");
            foreach (var classSymbol in dependencyClasses)
            {
                result.AppendLine($"// {classSymbol.ToDisplayString()}");
            }

            sourceProductionContext.AddSource("ClassPatternMatches", SourceText.From(result.ToString(), Encoding.UTF8));
        });
    }
}

// Utility extension to retrieve all namespace types
internal static class NamespaceExtensions
{
    public static IEnumerable<ITypeSymbol> GetNamespaceTypes(this INamespaceSymbol namespaceSymbol)
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            switch (member)
            {
                case INamespaceSymbol childNamespace:
                    foreach (var nestedType in childNamespace.GetNamespaceTypes())
                    {
                        yield return nestedType;
                    }
                    break;
                case ITypeSymbol type:
                    yield return type;
                    break;
            }
        }
    }
}
