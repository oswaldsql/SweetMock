namespace SweetMock.Utils;

using System;
using System.Linq;
using Microsoft.CodeAnalysis;

public static class ISymbolExtensions
{
    internal static bool HasOutOrRef(this IMethodSymbol method) =>
            method.Parameters.Any(p => p.RefKind is RefKind.Out or RefKind.Ref);

    internal static bool IsReturningTask(this IMethodSymbol method) =>
        method.ReturnType.ToString().Equals("System.Threading.Tasks.Task");

    internal static bool IsReturningGenericTask(this IMethodSymbol method) =>
        method.ReturnType.ToString().StartsWith("System.Threading.Tasks.Task<") &&
        ((INamedTypeSymbol)method.ReturnType).TypeArguments.Length > 0;

    internal static bool IsReturningValueTask(this IMethodSymbol method) =>
        method.ReturnType.ToString().Equals("System.Threading.Tasks.ValueTask");

    internal static bool IsReturningGenericValueTask(this IMethodSymbol method) =>
        method.ReturnType.ToString().StartsWith("System.Threading.Tasks.ValueTask<") &&
        ((INamedTypeSymbol)method.ReturnType).TypeArguments.Length > 0;

    internal static bool HasParameters(this IMethodSymbol method) => method.Parameters.Length > 0;

    internal static string EscapeToHtml(this string text) => text.Replace("<", "&lt;").Replace(">", "&gt;");

    internal static OverwriteString Overwrites(this ISymbol symbol)
    {
        if (symbol.ContainingType.TypeKind == TypeKind.Interface)
        {
            return new(symbol.ContainingSymbol + ".", "", "");
        }

        return new("", symbol.AccessibilityString() + " ", "override ");
    }

    internal static ParameterStrings ParameterStrings(this IMethodSymbol method)
    {
        var parameters = method.Parameters.Select(t => new ParameterInfo(t.Type.ToString(), t.Name, t.OutAsString(), t.Name)).ToList();

        var methodParameters = parameters.ToString(p => $"{p.OutString}{p.Type} {p.Name}");

        if (method.IsGenericMethod)
        {
            parameters.AddRange(method.TypeArguments.Select(typeArgument => new ParameterInfo("System.Type", "typeOf_" + typeArgument.Name, "", "typeof(" + typeArgument.Name + ")")));
        }

        var parameterList = parameters.ToString(p => $"{p.OutString}{p.Type} {p.Name}");
        var typeList = parameters.ToString(p => $"{p.Type}");
        var nameList = parameters.ToString(p => $"{p.OutString}{p.Function}");

        return new ParameterStrings (methodParameters, parameterList, typeList, nameList);
    }
}

public record OverwriteString(string containingSymbol, string accessibilityString, string overrideString);
internal record ParameterStrings(string methodParameters, string parameterList, string typeList, string nameList);

public record ParameterInfo(string Type, string Name, string OutString, string Function);

internal class UnsupportedAccessibilityException(Accessibility accessibility) : Exception($"Unsupported accessibility type '{accessibility}'")
{
    public Accessibility Accessibility => accessibility;
}