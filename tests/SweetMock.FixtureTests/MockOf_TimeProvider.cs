namespace SweetMock.FixtureTests.CustomMocks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

internal partial class MockOf_TimeProvider() : MockBase<TimeProvider>(TimeProvider.System);

[Mock<ILogger<string>, MockOf_ILogger<string>>]
internal class MockOf_ILogger<TCategoryName>() : MockBase<ILogger<TCategoryName>>(NullLogger<TCategoryName>.Instance);

//internal class MockOf_ILogger<TCategoryName>(Action<MockOf_ILogger<TCategoryName>.MockConfig>? action = null, MockOptions options = null)
//    : SweetMock.WrapperMock<ILogger<TCategoryName>>(action,options)
//{
//    protected override ILogger<TCategoryName>? value { get; set; } = NullLogger<TCategoryName>.Instance;
//}

public static class Mock2
{
    #region System.TimeProvider
    /// <summary>
    ///    Creates a mock object for <see cref="global::System.TimeProvider">TimeProvider</see>.
    /// </summary>
    /// <param name="config">Optional configuration for the mock object.</param>
    /// <param name="options">Options for the mock object.</param>
    /// <returns>The mock object for <see cref="global::System.TimeProvider">TimeProvider</see>.</returns>
    internal static SweetMock.FixtureTests.CustomMocks.MockOf_TimeProvider2 TimeProvider
        (System.Action<SweetMock.FixtureTests.CustomMocks.MockOf_TimeProvider2.MockConfig>? config = null, MockOptions? options = null)
    {
        var result = new SweetMock.FixtureTests.CustomMocks.MockOf_TimeProvider2();
        config?.Invoke(result.Config);
        result.MockInitialize(config, options);
        return result;
    }

    /// <summary>
    ///    Creates a mock object for <see cref="global::System.TimeProvider">TimeProvider</see>.
    /// </summary>
    /// <param name="configTimeProvider">Outputs configuration for the mock object.</param>
    /// <param name="options">Options for the mock object.</param>
    /// <returns>The mock object for <see cref="global::System.TimeProvider">TimeProvider</see>.</returns>
    internal static SweetMock.FixtureTests.CustomMocks.MockOf_TimeProvider2 TimeProvider
        (out SweetMock.FixtureTests.CustomMocks.MockOf_TimeProvider2.MockConfig configTimeProvider, MockOptions? options = null){
        var result = new SweetMock.FixtureTests.CustomMocks.MockOf_TimeProvider2();
        result.MockInitialize(_ => {}, options);
        configTimeProvider = result.Config;
        return result;
    }
    #endregion
}

public class MockOf_TimeProvider2() : MockBase<TimeProvider>(TimeProvider.System);