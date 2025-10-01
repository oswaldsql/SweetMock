namespace SweetMock.BuildInMocks;

using System;
using Microsoft.Extensions.Options;

public class MockOf_IOptionsMonitor<TOptions> : IOptionsMonitor<TOptions>
{
    public TOptions Get(string? name) => throw new NotImplementedException();

    public IDisposable? OnChange(Action<TOptions, string?> listener) => throw new NotImplementedException();

    public TOptions CurrentValue { get; }
}
