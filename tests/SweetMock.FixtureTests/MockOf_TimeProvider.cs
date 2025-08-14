namespace SweetMock.FixtureTests.CustomMocks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

internal class MockOf_TimeProvider(Action<MockOf_TimeProvider.Config>? action = null, object dummy = null) : SweetMock.WrapperMock<TimeProvider>(action);

internal class MockOf_ILogger<TCategoryName>(Action<MockOf_ILogger<TCategoryName>.Config>? action = null, object dummy = null)
    : SweetMock.WrapperMock<ILogger<TCategoryName>>(action)
{
    protected override ILogger<TCategoryName>? value { get; set; } = NullLogger<TCategoryName>.Instance;
}

internal class MockOf_String(Action<MockOf_String.Config>? action = null, object dummy = null) : SweetMock.WrapperMock<string>(action);