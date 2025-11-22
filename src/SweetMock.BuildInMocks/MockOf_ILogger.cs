// ReSharper disable RedundantNullableDirective
// ReSharper disable ArrangeNamespaceBody
// ReSharper disable RedundantNameQualifier
// ReSharper disable RedundantBaseQualifier
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
#nullable enable

namespace Microsoft.Extensions.Logging{
    using SweetMock;

    [System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SweetMockVersion}}")]
    internal class MockOf_ILogger<TCategoryName> : global::SweetMock.MockBase<ILogger<TCategoryName>>{
        public override ILogger<TCategoryName> Value => new MockLogger<TCategoryName>(this.Options);

        private class MockLogger<TMCategoryName> : ILogger<TMCategoryName>{
            private readonly global::SweetMock.MockOptions options;
            public MockLogger(global::SweetMock.MockOptions options){
                this.options = options;
                options.Logger?.Add($"Microsoft.Extensions.Logging.ILogger<{typeof(TMCategoryName).Name}>()");
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, global::System.Exception? exception, global::System.Func<TState, global::System.Exception?, string> formatter){
                var message = formatter(state, exception);
                var arguments = global::SweetMock.Arguments.With("message", message).And("logLevel", logLevel).And("eventId", eventId).And("exception", exception).And("state", state);
                this.options.Logger?.Add("Microsoft.Extensions.Logging.ILogger.Log<TState>(Microsoft.Extensions.Logging.LogLevel, Microsoft.Extensions.Logging.EventId, TState, System.Exception?, System.Func<TState, System.Exception?, string>)", arguments);
            }

            public bool IsEnabled(LogLevel logLevel){
                this.options.Logger?.Add("Microsoft.Extensions.Logging.ILogger.IsEnabled(LogLevel)", global::SweetMock.Arguments.With("logLevel", logLevel));
                return true;
            }

            public global::System.IDisposable BeginScope<TState>(TState state) where TState : notnull{
                this.options.Logger?.Add("Microsoft.Extensions.Logging.ILogger.BeginScope<TState>(TState)", global::SweetMock.Arguments.With("state", state));
                return new IDisposeWrapper(() => this.options.Logger?.Add("Microsoft.Extensions.Logging.ILogger.EndScope<TState>(TState)", global::SweetMock.Arguments.With("state", state)));
            }
        }

        public class IDisposeWrapper(global::System.Action action) : global::System.IDisposable{
            public void Dispose() => action();
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SweetMockVersion}}")]
    internal static class MockOf_ILogger_LogExtensions{
        public static ILogger_Filter ILogger(this global::SweetMock.CallLog source) => new(source);

        public class ILogger_Filter(global::SweetMock.CallLog source) : CallLogFilter(source, "Microsoft.Extensions.Logging.ILogger.");

#region Log
        public class Log_Args : SweetMock.TypedArguments{
            /// <summary>
            ///    The logLevel argument used.
            /// </summary>
            public Microsoft.Extensions.Logging.LogLevel logLevel => (Microsoft.Extensions.Logging.LogLevel)base.Arguments["logLevel"]!;

            /// <summary>
            ///    The eventId argument used.
            /// </summary>
            public Microsoft.Extensions.Logging.EventId eventId => (Microsoft.Extensions.Logging.EventId)base.Arguments["eventId"]!;

            /// <summary>
            ///    The state argument used.
            ///    The argument is a generic type. (TState)
            /// </summary>
            public object state => base.Arguments["state"]!;

            /// <summary>
            ///    The exception argument used.
            /// </summary>
            public System.Exception exception => (System.Exception?)base.Arguments["exception"]!;

            /// <summary>
            ///    The message argument used.
            /// </summary>
            public string message => (string)base.Arguments["message"]!;

        }

        /// <summary>
        ///    Identifying calls to the method <see cref="global::Microsoft.Extensions.Logging.ILogger.Log{TState}(Microsoft.Extensions.Logging.LogLevel, Microsoft.Extensions.Logging.EventId, TState, System.Exception, System.Func{TState, System.Exception, System.String})">ILogger.Log&lt;TState&gt;(LogLevel, EventId, TState, Exception, Func&lt;TState, Exception, string&gt;)</see>.
        /// </summary>
        public static System.Collections.Generic.IEnumerable<Log_Args> Log(this SweetMock.CallLog log, global::System.Func<Log_Args, bool>? ILogger_Log_Predicate = null) =>
            log.Matching("Microsoft.Extensions.Logging.ILogger.Log<TState>(Microsoft.Extensions.Logging.LogLevel, Microsoft.Extensions.Logging.EventId, TState, System.Exception?, System.Func<TState, System.Exception?, string>)", ILogger_Log_Predicate);
#endregion

#region IsEnabled
        public class IsEnabled_Args : SweetMock.TypedArguments{
            /// <summary>
            ///    The logLevel argument used.
            /// </summary>
            public Microsoft.Extensions.Logging.LogLevel logLevel => (Microsoft.Extensions.Logging.LogLevel)base.Arguments["logLevel"]!;

        }

        /// <summary>
        ///    Identifying calls to the method <see cref="global::Microsoft.Extensions.Logging.ILogger.IsEnabled(Microsoft.Extensions.Logging.LogLevel)">ILogger.IsEnabled(LogLevel)</see>.
        /// </summary>
        public static System.Collections.Generic.IEnumerable<IsEnabled_Args> IsEnabled(this SweetMock.CallLog log, global::System.Func<IsEnabled_Args, bool>? ILogger_IsEnabled_Predicate = null) =>
            log.Matching("Microsoft.Extensions.Logging.ILogger.IsEnabled(Microsoft.Extensions.Logging.LogLevel)", ILogger_IsEnabled_Predicate);
#endregion

#region BeginScope
        public class BeginScope_Args : SweetMock.TypedArguments{
            /// <summary>
            ///    The state argument used.
            ///    The argument is a generic type. (TState)
            /// </summary>
            public object state => base.Arguments["state"]!;

        }

        /// <summary>
        ///    Identifying calls to the method <see cref="global::Microsoft.Extensions.Logging.ILogger.BeginScope{TState}(TState)">ILogger.BeginScope&lt;TState&gt;(TState)</see>.
        /// </summary>
        public static System.Collections.Generic.IEnumerable<BeginScope_Args> BeginScope(this SweetMock.CallLog log, global::System.Func<BeginScope_Args, bool>? ILogger_BeginScope_Predicate = null) =>
            log.Matching("Microsoft.Extensions.Logging.ILogger.BeginScope<TState>(TState)", ILogger_BeginScope_Predicate);
#endregion

#region EndScope
        public class EndScope_Args : SweetMock.TypedArguments{
            /// <summary>
            ///    The state argument used.
            ///    The argument is a generic type. (TState)
            /// </summary>
            public object state => base.Arguments["state"]!;

        }

        /// <summary>
        ///    Identifying when a scope initialized with <see cref="global::Microsoft.Extensions.Logging.ILogger.BeginScope{TState}(TState)">ILogger.BeginScope&lt;TState&gt;(TState)</see> is ended.
        /// </summary>
        public static System.Collections.Generic.IEnumerable<EndScope_Args> EndScope(this SweetMock.CallLog log, global::System.Func<EndScope_Args, bool>? ILogger_EndScope_Predicate = null) =>
            log.Matching("Microsoft.Extensions.Logging.ILogger.EndScope<TState>(TState)", ILogger_EndScope_Predicate);
#endregion

    }
}
