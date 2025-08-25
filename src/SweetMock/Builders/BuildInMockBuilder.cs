namespace SweetMock.Builders;

using SweetMock.Generation;
using Utils;

public static class BuildInMockBuilder
{
    private static Dictionary<string, Func<string>> buildInMocks = new()
    {
        {"Microsoft.Extensions.Logging.ILogger<TCategoryName>", ILogger},
        {"System.TimeProvider", TimeProvider},
        {"Microsoft.Extensions.Options.IOptions<TOptions>", IOptions},
        //{"Microsoft.Extensions.Options.IOptionsMonitor<TOptions>", IOptionsMonitor},
    };
    internal static IEnumerable<MockInfo> CreateBuildinMocks(List<MockTypeWithLocation> collectedMocks, SourceProductionContext spc)
    {
        var candidates = collectedMocks.Where(t => t.Type != null).ToLookup(t => t.Type, SymbolEqualityComparer.Default);

        foreach (var candidate in candidates)
        {
            if (candidate.Key is INamedTypeSymbol symbol)
            {
                var displayString = symbol.ToDisplayString();
                if (buildInMocks.TryGetValue(displayString, out var func))
                {
                    var source = func();
                    spc.AddSource(symbol.ToCRef() + ".g.cs", source);

                    yield return new MockInfo(symbol, symbol.ContainingNamespace + ".MockOf_" + symbol.Name, MockKind.BuildIn, "MockConfig");
                    collectedMocks.RemoveAll(t => SymbolEqualityComparer.Default.Equals(t.Type, symbol));
                }
            }
        }
    }

    private static string ILogger()
    {
        CodeBuilder result = new();

        result
            .Nullable()
            .Usings("global::System", "global::Microsoft.Extensions.Logging", "global::SweetMock")
            .Scope("namespace Microsoft.Extensions.Logging", namespaceScope =>
            {
                namespaceScope
                    .AddGeneratedCodeAttrib()
                    .Scope("internal class MockOf_ILogger<TCategoryName> : MockBase<ILogger<TCategoryName>>", classScope =>
                    {
                        classScope
                            .Add("internal override ILogger<TCategoryName> Value => new MockLogger<TCategoryName>(this.Options);")
                            .AddLineBreak()
                            .Scope("private class MockLogger<TMCategoryName> : ILogger<TMCategoryName>", logscope => logscope
                                .Add("private readonly MockOptions options;")
                                .Scope("public MockLogger(MockOptions options)", t => t
                                    .Add("this.options = options;")
                                    .Add("options.Logger?.Add($\"Microsoft.Extensions.Logging.ILogger<{typeof(TMCategoryName).Name}>()\");")
                                ).AddLineBreak()
                                .Scope("public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)", t => t
                                    .Add("var message = formatter(state, exception);")
                                    .Add("var arguments = Arguments.With(\"message\", message).And(\"logLevel\", logLevel).And(\"eventId\", eventId).And(\"exception\", exception).And(\"state\", state);")
                                    .Add("options.Logger?.Add(\"Microsoft.Extensions.Logging.ILogger.Log<TState>(Microsoft.Extensions.Logging.LogLevel, Microsoft.Extensions.Logging.EventId, TState, System.Exception?, System.Func<TState, System.Exception?, string>)\", arguments);")
                                ).AddLineBreak()
                                .Scope("public bool IsEnabled(LogLevel logLevel)", t => t
                                    .Add("options.Logger?.Add(\"Microsoft.Extensions.Logging.ILogger.IsEnabled(LogLevel)\", Arguments.With(\"logLevel\", logLevel));")
                                    .Add("return true;")
                                ).AddLineBreak()
                                .Scope("public IDisposable? BeginScope<TState>(TState state) where TState : notnull", t => t
                                    .Add("options.Logger?.Add(\"Microsoft.Extensions.Logging.ILogger.BeginScope<TState>(TState)\", Arguments.With(\"state\", state));")
                                    .Add("return new IDisposeWrapper(() => options.Logger?.Add(\"Microsoft.Extensions.Logging.ILogger.EndScope<TState>(TState)\", Arguments.With(\"state\", state)));")
                                )).AddLineBreak()
                            .Scope("public class IDisposeWrapper(Action action) : IDisposable", t => t
                                .Add("public void Dispose() => action();"));
                    }).AddLineBreak();

                namespaceScope
                    .AddGeneratedCodeAttrib()
                    .Scope("internal static class MockOf_ILogger_LogExtensions", classScope =>
                    {
                        classScope
                            .Region("Log", regionScope =>
                            {
                                regionScope.AddFilterClass("Log", filterScope => filterScope
                                    .AddArgumentFilter("logLevel", "Microsoft.Extensions.Logging.LogLevel")
                                    .AddArgumentFilter("eventId", "Microsoft.Extensions.Logging.EventId")
                                    .AddGenericArgumentFilter("state")
                                    .AddArgumentFilter("exception", "System.Exception?")
                                    .AddArgumentFilter("message", "System.String"));

                                regionScope
                                    .Documentation("Identifying calls to the method <see cref=\"global::Microsoft.Extensions.Logging.ILogger.Log{TState}(Microsoft.Extensions.Logging.LogLevel, Microsoft.Extensions.Logging.EventId, TState, System.Exception, System.Func{TState, System.Exception, System.String})\">ILogger.Log&lt;TState&gt;(LogLevel, EventId, TState, Exception, Func&lt;TState, Exception, string&gt;)</see>.")
                                    .Add("public static System.Collections.Generic.IEnumerable<Log_Args> Log(this SweetMock.CallLog log, Func<Log_Args, bool>? ILogger_Log_Predicate = null) =>").Indent()
                                    .Add("log.Matching<Log_Args>(\"Microsoft.Extensions.Logging.ILogger.Log<TState>(Microsoft.Extensions.Logging.LogLevel, Microsoft.Extensions.Logging.EventId, TState, System.Exception?, System.Func<TState, System.Exception?, string>)\", ILogger_Log_Predicate);").Unindent();
                            });

                        classScope.Region("IsEnabled", regionScope =>
                        {
                            regionScope.AddFilterClass("IsEnabled", filterScope => filterScope
                                .AddArgumentFilter("logLevel", "Microsoft.Extensions.Logging.LogLevel"));

                            regionScope.Documentation("Identifying calls to the method <see cref=\"global::Microsoft.Extensions.Logging.ILogger.IsEnabled(Microsoft.Extensions.Logging.LogLevel)\">ILogger.IsEnabled(LogLevel)</see>.")
                                .Add("public static System.Collections.Generic.IEnumerable<IsEnabled_Args> IsEnabled(this SweetMock.CallLog log, Func<IsEnabled_Args, bool>? ILogger_IsEnabled_Predicate = null) =>")
                                .Indent()
                                .Add("log.Matching<IsEnabled_Args>(\"Microsoft.Extensions.Logging.ILogger.IsEnabled(Microsoft.Extensions.Logging.LogLevel)\", ILogger_IsEnabled_Predicate);")
                                .Unindent();
                        });

                        classScope.Region("BeginScope", regionScope =>
                        {
                            regionScope.AddFilterClass("BeginScope", filterScope => filterScope
                                .AddGenericArgumentFilter("state"));

                            regionScope.Documentation("Identifying calls to the method <see cref=\"global::Microsoft.Extensions.Logging.ILogger.BeginScope{TState}(TState)\">ILogger.BeginScope&lt;TState&gt;(TState)</see>.")
                                .Add("public static System.Collections.Generic.IEnumerable<BeginScope_Args> BeginScope(this SweetMock.CallLog log, Func<BeginScope_Args, bool>? ILogger_BeginScope_Predicate = null) =>")
                                .Indent()
                                .Add("log.Matching<BeginScope_Args>(\"Microsoft.Extensions.Logging.ILogger.BeginScope<TState>(TState)\", ILogger_BeginScope_Predicate);")
                                .Unindent();
                        });

                        classScope.Region("EndScope", regionScope =>
                        {
                            regionScope.AddFilterClass("EndScope", filterScope => filterScope
                                .AddGenericArgumentFilter("state"));

                            regionScope.Documentation("Identifying when a scope initialized with <see cref=\"global::Microsoft.Extensions.Logging.ILogger.BeginScope{TState}(TState)\">ILogger.BeginScope&lt;TState&gt;(TState)</see> is ended.")
                                .Add("public static System.Collections.Generic.IEnumerable<EndScope_Args> EndScope(this SweetMock.CallLog log, Func<EndScope_Args, bool>? ILogger_EndScope_Predicate = null) =>")
                                .Indent()
                                .Add("log.Matching<EndScope_Args>(\"Microsoft.Extensions.Logging.ILogger.EndScope<TState>(TState)\", ILogger_EndScope_Predicate);")
                                .Unindent();
                        });
                    });
            });

        return result.ToString();
    }

    private static CodeBuilder AddFilterClass(this CodeBuilder scope, string name, Action<CodeBuilder> filterScope) =>
        scope.Scope($"public class {name}_Args : SweetMock.TypedArguments", filterScope).AddLineBreak();

    private static CodeBuilder AddArgumentFilter(this CodeBuilder methodScope, string name, string type) =>
        methodScope
            .Documentation($"The {name} argument used.")
            .Add($"public {type} {name} => ({type})base.Arguments[\"{name}\"]!;")
            .AddLineBreak();

    private static CodeBuilder AddGenericArgumentFilter(this CodeBuilder methodScope, string name) =>
        methodScope
            .Documentation($"The {name} argument used.", "The argument is a generic type. (TState)")
            .Add($"public object? {name} => base.Arguments[\"{name}\"]!;")
            .AddLineBreak();


    private static string TimeProvider()
    {
        CodeBuilder result = new();

        result
            .Nullable()
            .Usings("global::System", "global::SweetMock")
            .Scope("namespace System", namespaceScope => namespaceScope
                .AddGeneratedCodeAttrib()
                .Add("internal class MockOf_TimeProvider(): MockBase<TimeProvider>(TimeProvider.System);"));

        return result.ToString();
    }

    private static string IOptions()
    {
        CodeBuilder result = new();

        result
            .Nullable()
            .Usings("global::System", "global::Microsoft.Extensions.Options", "global::SweetMock")
            .Scope("namespace Microsoft.Extensions.Options", namespaceScope => namespaceScope
                .AddGeneratedCodeAttrib()
                .Scope("internal class MockOf_IOptions<TOptions>() where TOptions : class", classScope =>
                {
                    classScope
                        .Add("public MockOptions Options { get; set; } = new MockOptions() {InstanceName = typeof(TOptions).Name};")
                        .Add("public MockConfig Config { get; } = new MockConfig();")
                        .Scope("internal IOptions<TOptions> Value", optionsScope => optionsScope
                            .Add("get => Config.Value ?? throw new ArgumentNullException(Options.InstanceName, $\"'{Options.InstanceName}' must have a value before being used.\");"))
                        .Scope("public class MockConfig", classScope => classScope
                            .Add("public IOptions<TOptions>? Value { get; private set; }")
                            .Scope("public MockConfig()", ctorScope => ctorScope
                                .Scope("try", tryScope => tryScope
                                    .Add("var options = Activator.CreateInstance<TOptions>();")
                                    .Add("this.Value = new OptionsWrapper<TOptions>((TOptions)options);"))
                                .Scope("catch", catchScope => catchScope.Add("// ignored")))
                            .Scope("public MockConfig Set(TOptions options)", setScope => setScope
                                .Add("this.Value = new OptionsWrapper<TOptions>(options);")
                                .Add("return this;")
                            )
                        );
                }));

        return result.ToString();
    }

    public static string IOptionsMonitor()
    {
        CodeBuilder result = new();

        result
            .Nullable()
            .Usings("global::System", "global::Microsoft.Extensions.Options", "global::SweetMock")
            .Scope("namespace Microsoft.Extensions.Options", namespaceScope => namespaceScope
                .AddGeneratedCodeAttrib()
                .Scope("internal class MockOf_IOptionsMonitor<TOptions>() where TOptions : class", classScope =>
                {
                    classScope
                        .Add("public MockOptions Options { get; set; } = new MockOptions() {InstanceName = typeof(TOptions).Name};")
                        .Add("public MockConfig Config { get; } = new MockConfig();")
                        .Add("internal IOptionsMonitor<TOptions> Value => Config.Value;")
                        .Scope("public class MockConfig", classScope => classScope
                            .Add("public OptionsMonitor<TOptions> Value { get; private set; }")
                            .Add("private readonly OptionsCache<TOptions> cache = new();")
                            .Scope("public MockConfig()", ctorScope => ctorScope
                                .Add("var factory = new OptionsFactory<TOptions>([], []);")
                                .Add("Value = new OptionsMonitor<TOptions>(factory, [], cache);")
                                .Scope("try", tryScope => tryScope
                                    .Add("var options = Activator.CreateInstance<TOptions>();")
                                    .Add("this.cache.TryAdd(null, options);"))
                                .Scope("catch", catchScope => catchScope.Add("// ignored")))
                            .Scope("public MockConfig Set(TOptions options)", setScope => setScope
                                .Add("this.cache.TryAdd(null, options);")
                                .Add("return this;"))
                            .Scope("public MockConfig Set(string name, TOptions options)", setScope => setScope
                                .Add("this.cache.TryAdd(name, options);")
                                .Add("return this;"))
                        );
                }));

        return result.ToString();



        /*
         * internal class MockOf_OptionsMonitor<TOptions>() where TOptions : class
{
    public MockOptions Options { get; set; } = new MockOptions() {InstanceName = typeof(TOptions).Name};

    public MockConfig Config { get; } = new MockConfig();

    internal IOptionsMonitor<TOptions> Value => Config.Value;

    public class MockConfig
    {
        public OptionsMonitor<TOptions> Value { get; private set; }

        private readonly OptionsCache<TOptions> cache = new();
        public MockConfig()
        {
            var factory = new OptionsFactory<TOptions>([], []);
            Value = new OptionsMonitor<TOptions>(factory, [], cache);

            try
            {
                var options = Activator.CreateInstance<TOptions>();
                this.cache.TryAdd(null, options);
            }
            catch
            {
                // ignored
            }
        }

        public MockConfig Set(TOptions options)
        {
            this.cache.TryAdd(null, options);
            return this;
        }

        public MockConfig Set(string name, TOptions options)
        {
            this.cache.TryAdd(name, options);
            return this;
        }

    }
}
         */

    }
}
