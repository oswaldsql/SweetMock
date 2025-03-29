namespace SweetMock.BuilderTests.Util;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class SymbolHelper
{
    public static INamedTypeSymbol GetClassSymbol(string sourceCode, string className)
    {
        // Parse the source code
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        // Create a compilation
        var compilation = CSharpCompilation.Create("MyCompilation",
            new[] { syntaxTree },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });

        // Get the semantic model
        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        // Find the class declaration
        var classDeclaration = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.Text == className);

        if (classDeclaration == null)
        {
            return null;
        }

        // Get the symbol for the class
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        return classSymbol;
    }
}