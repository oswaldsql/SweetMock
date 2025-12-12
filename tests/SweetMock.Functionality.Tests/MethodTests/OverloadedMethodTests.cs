// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace Test.MethodTests;

[Mock<IOverloadedMethods>]
public class OverloadedMethodTests(ITestOutputHelper output)
{
    [Fact]
    public void OverloadedMethod_WhenNotInitialized_AllOverloadsShouldThrowException()
    {
        // Arrange
        var logger = new CallLog();
        var sut = Mock.IOverloadedMethods(_ => {}, new(logger));

        // Act

        // Assert
        Assert.Throws<NotExplicitlyMockedException>(() => sut.OverloadedMethod());
        Assert.Throws<NotExplicitlyMockedException>(() => sut.OverloadedMethod("name"));
        Assert.Throws<NotExplicitlyMockedException>(() => sut.OverloadedMethod("name", 10));
        Assert.Throws<NotExplicitlyMockedException>(() => sut.OverloadedMethod(10, "name"));

        var logs = new OverloadedMethodTests_Logs(logger);
        foreach (var overloadedMethodArguments in logs.OverloadedMethod(arguments => arguments.name == "name"))
        {
            output.WriteLine(overloadedMethodArguments.ToString());
        } 
    }

    internal class OverloadedMethodTests_Logs(CallLog log)
    {
        public IEnumerable<MockOf_IOverloadedMethods.OverloadedMethod_Arguments> OverloadedMethod(Func<MockOf_IOverloadedMethods.OverloadedMethod_Arguments, bool>? filter = null) => 
            log.Calls.OfType<MockOf_IOverloadedMethods.OverloadedMethod_Arguments>().Where(filter ?? (_ => true));
    }
    
    [Fact]
    public void OverloadedMethod_WhenMockNotInitialized_ShouldThrowException()
    {
        // Arrange
        var sut = Mock.IOverloadedMethods();

        // Act
        var actual = Assert.Throws<NotExplicitlyMockedException>(() => sut.OverloadedMethod());

        // Assert
        Assert.NotNull(actual);
        output.WriteLine(actual.Message);
        Assert.Contains("OverloadedMethod", actual.Message);
    }

    [Fact]
    public void OverloadedMethod_WhenMockInitializedWithNoParameters_ShouldCallMethod()
    {
        // Arrange
        var isCalled = false;
        var sut = Mock.IOverloadedMethods(mock => mock.OverloadedMethod(() =>
        {
            isCalled = true;
            return 10;
        }));

        // Act
        var actual = sut.OverloadedMethod();

        // Assert
        Assert.True(isCalled, "Should be true when the mock is called");
        Assert.Equal(10, actual);
    }

    [Fact]
    public void OverloadedMethod_WhenMockInitializedWithOneParameter_ShouldCallMethod()
    {
        // Arrange
        var actual = "";
        var sut = Mock.IOverloadedMethods(mock => mock.OverloadedMethod(value => actual = value));

        // Act
        sut.OverloadedMethod("Whats in a name");

        // Assert
        Assert.Equal("Whats in a name", actual);
    }

    [Fact]
    public void OverloadedMethod_WhenMockInitializedWithTwoParameters_ShouldCallMethod()
    {
        // Arrange
        var sut = Mock.IOverloadedMethods(mock => mock.OverloadedMethod((string name, int value) => $"{name} {value}"));

        // Act
        var actual = sut.OverloadedMethod("Whats in a name", 10);

        // Assert
        Assert.Equal("Whats in a name 10", actual);
    }

    public interface IOverloadedMethods
    {
        int OverloadedMethod();
        string OverloadedMethod(string name);
        string OverloadedMethod(string name, int value);
        string OverloadedMethod(string name, int? value, DateTime? date);
        string OverloadedMethod(int value, string name);
        string OverloadedMethod<T>(T generic);
    }
}