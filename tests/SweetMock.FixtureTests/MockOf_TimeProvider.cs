namespace SweetMock.FixtureTests.CustomMocks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

internal class MockOf_TimeProvider(Action<MockOf_TimeProvider.MockConfig>? action = null, MockOptions options = null) : SweetMock.WrapperMock<TimeProvider>(action, options);

[Mock<ILogger<string>,MockOf_ILogger<string>>]
internal class MockOf_ILogger<TCategoryName>(Action<MockOf_ILogger<TCategoryName>.MockConfig>? action = null, MockOptions options = null)
    : SweetMock.WrapperMock<ILogger<TCategoryName>>(action,options)
{
    protected override ILogger<TCategoryName>? value { get; set; } = NullLogger<TCategoryName>.Instance;
}

internal class MockOf_String(Action<MockOf_String.MockConfig>? action = null, MockOptions options = null) : SweetMock.WrapperMock<string>(action, options);