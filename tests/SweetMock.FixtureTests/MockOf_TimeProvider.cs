// ReSharper disable RedundantNameQualifier
namespace SweetMock.FixtureTests;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

    [System.CodeDom.Compiler.GeneratedCode("SweetMock","0.9.25.0")]
    internal class MockOf_ILogger<TCategoryName> : MockBase<ILogger<TCategoryName>>{
        public override ILogger<TCategoryName> Value => new MockLogger<TCategoryName>(this.Options);

        private class MockLogger<TMCategoryName> : ILogger<TMCategoryName>{
            private readonly MockOptions options;

            public MockLogger(MockOptions options)
            {
                this.options = options;
                options.Logger?.Add($"Microsoft.Extensions.Logging.ILogger<{options.InstanceName}>");
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter){
                var message = formatter(state, exception);
                var arguments = Arguments.With("message", message).And("logLevel", logLevel).And("eventId", eventId).And("Exception", exception).And("State", state);
                options.Logger?.Add($"Log : {logLevel} : {message}", arguments);
            }

            public bool IsEnabled(LogLevel logLevel){
                return true;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull{
                options.Logger?.Add("BeginScope : " + state, global::SweetMock.Arguments.With("state", state));
                return new IDisposeWrapper(() => options.Logger?.Add("EndScope : " + state, Arguments.With("state", state)));
            }
        }

        public class IDisposeWrapper(Action action) : IDisposable{
            public void Dispose() => action();
        }
    }

// IServiceProvider
// IMemoryCache
// IOptionsMonitor
// EF?


internal class MockOf_IMemoryCache : MockBase<IMemoryCache>
{
    TimeProvider _provider = TimeProvider.System;
    
    public MockOf_IMemoryCache() : base()
    {
        ISystemClock? clock = new MockSystemClock(_provider);
        var options = new MemoryCacheOptions() { Clock = clock};
        this.Config.Value = new MemoryCache(options, new NullLoggerFactory());
    }

    internal class MockSystemClock(TimeProvider timeProvider) : ISystemClock
    {
        public DateTimeOffset UtcNow => 
            timeProvider.GetUtcNow();
    }
}