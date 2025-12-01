// ReSharper disable RedundantNullableDirective
// ReSharper disable ArrangeNamespaceBody
// ReSharper disable RedundantNameQualifier
// ReSharper disable RedundantBaseQualifier
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
#nullable enable

namespace Microsoft.Extensions.Logging{
//    using SweetMock;
//
//    [System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SweetMockVersion}}")]
//    internal class MockOf_ILogger<TCategoryName> : global::SweetMock.MockBase<ILogger<TCategoryName>>{
//        public override ILogger<TCategoryName> Value => new MockLogger<TCategoryName>(this.Options);
//
//        private class MockLogger<TMCategoryName> : ILogger<TMCategoryName>{
//            private readonly global::SweetMock.MockOptions options;
//            public MockLogger(global::SweetMock.MockOptions options){
//                this.options = options;
//                //options.Logger?.Add($"Microsoft.Extensions.Logging.ILogger<{typeof(TMCategoryName).Name}>()");
//            }
//
//            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, global::System.Exception? exception, global::System.Func<TState, global::System.Exception?, string> formatter){
//                var message = formatter(state, exception);
//                var arguments = global::SweetMock.Arguments.With("message", message).And("logLevel", logLevel).And("eventId", eventId).And("exception", exception).And("state", state);
//                //this.options.Logger?.Add("Microsoft.Extensions.Logging.ILogger.Log<TState>(Microsoft.Extensions.Logging.LogLevel, Microsoft.Extensions.Logging.EventId, TState, System.Exception?, System.Func<TState, System.Exception?, string>)", arguments);
//            }
//
//            public bool IsEnabled(LogLevel logLevel){
//                //this.options.Logger?.Add("Microsoft.Extensions.Logging.ILogger.IsEnabled(LogLevel)", global::SweetMock.Arguments.With("logLevel", logLevel));
//                return true;
//            }
//
//            public global::System.IDisposable BeginScope<TState>(TState state) where TState : notnull{
//                //this.options.Logger?.Add("Microsoft.Extensions.Logging.ILogger.BeginScope<TState>(TState)", global::SweetMock.Arguments.With("state", state));
//                return new IDisposeWrapper(() =>
//                {
//                    //this.options.Logger?.Add("Microsoft.Extensions.Logging.ILogger.EndScope<TState>(TState)", global::SweetMock.Arguments.With("state", state));
//                });
//            }
//        }
//
//        public class IDisposeWrapper(global::System.Action action) : global::System.IDisposable{
//            public void Dispose() => action();
//        }
//    }
}
