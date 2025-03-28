namespace SweetMock.Builders;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

public class MockBuilder
{

    public IEnumerable<BuildResult> BuildFiles(INamedTypeSymbol target)
    {
        try
        {
            ValidateTargetIsValid(target);

            var mockDetails = GetMockDetails(target);

            return BuildResults(mockDetails).ToArray();
        }
        catch (Exception e)
        {
            return [new("Error", "/*" + e + "*/")];
        }
    }

    private void ValidateTargetIsValid(INamedTypeSymbol target)
    {
        if(target.DeclaredAccessibility == Accessibility.Private) throw new Exception("Class must not be private");
        if (target.IsSealed) throw new Exception("Target must be not be sealed");
        if (target.IsStatic) throw new Exception("Target must be not be static");
        if(target.IsAnonymousType) throw new Exception("Target must not be an anonymous type");
    }

    private static IEnumerable<BuildResult> BuildResults(MockDetails mockDetails)
    {
        var code = BaseClassBuilder.Build(mockDetails);
        yield return new("Base", code.ToString());

        var factories = new FactoryClassBuilder().Build(mockDetails);
        yield return new("Factory", factories);

        var logFilters = new LogExtensionsBuilder().BuildLogExtensions(mockDetails);
        yield return new("Logging", logFilters);
    }

    private static MockDetails GetMockDetails(INamedTypeSymbol target)
    {
        var sourceName = target.ToString();
        var interfaceNamespace = target.ContainingNamespace.ToString();
        var mockType = "MockOf_" + target.Name;
        var mockName = "MockOf_" + target.Name;
        var constraints = "";

        var typeArguments = target.TypeArguments;
        if (typeArguments.Length > 0)
        {
            var types = string.Join(", ", typeArguments.Select(t => t.Name));
            mockType = $"MockOf_{target.Name}<{types}>";
            constraints = typeArguments.ToConstraints();
        }

        return new(target, interfaceNamespace, sourceName, mockType, mockName, constraints);
    }
}

public record MockDetails(INamedTypeSymbol Target, string Namespace, string SourceName, string MockType, string MockName, string Constraints);

public record BuildResult(string Name, string Content);
