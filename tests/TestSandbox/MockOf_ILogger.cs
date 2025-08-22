namespace TestSandbox;

using global::System;
using global::Microsoft.Extensions.Logging;
using global::SweetMock;

internal class MockOf_ILogger<TCategoryName>() : MockBase<ILogger<TCategoryName>>()
{
    internal override ILogger<TCategoryName> Value => new MockLogger<TCategoryName>(this.Options);

    private class MockLogger<TMCategoryName>(MockOptions options) : ILogger<TMCategoryName>{
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter){
            var message = formatter(state, exception);
            var arguments = Arguments.With("message", message).And("logLevel", logLevel).And("eventId", eventId).And("Exception", exception).And("State", state);
            options.Logger?.Add($"Log : {logLevel} : {message}", arguments);
        }
        public bool IsEnabled(LogLevel logLevel){
            return true;
        }
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull{
            options.Logger?.Add("BeginScope : " + state, Arguments.With("state", state));
            return new IDisposeWrapper(() => options.Logger?.Add("EndScope : " + state, Arguments.With("state", state)));
        }
    }
    public class IDisposeWrapper(Action action) : IDisposable{
        public void Dispose() => action();
    }
}