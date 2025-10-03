namespace SweetMock;

using System;
using System.Collections.Generic;
using System.Linq;

public static class LEX
{
    public static IEnumerable<T> Matching<T>(this IEnumerable<CallLogItem> source, string signature, Func<T, bool>? predicate = null) where T : TypedArguments, new() =>
        source.Where(t => t.MethodSignature == signature)
            .Select(t => new TypedCallLogItem<T>(t).TypedArguments)
            .Where(t => predicate == null || predicate(t));

    public static IEnumerable<T> Matching<T>(this IEnumerable<CallLogItem> source, HashSet<string> signatures, Func<T, bool>? predicate = null) where T : TypedArguments, new() =>
        source.Where(t => signatures.Contains(t.MethodSignature!))
            .Select(t => new TypedCallLogItem<T>(t).TypedArguments)
            .Where(t => predicate == null || predicate(t));
}