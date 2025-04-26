// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace Test.MethodTests;

[Mock<IBasicMethods>]
public class BasicMethodTests
{
    [Fact]
    public void VoidWithoutParameters_WhenMockNotInitialized_ShouldThrowException()
    {
        // Arrange
        var sut = Mock.IBasicMethods();

        // Act
        var actual = Assert.Throws<InvalidOperationException>(() => sut.VoidWithoutParameters());

        // Assert
        Assert.NotNull(actual);
        Assert.Contains("VoidWithoutParameters", actual.Message);
        Assert.Contains("VoidWithoutParameters", actual.Source);
    }

    [Fact]
    public void VoidWithoutParameters_WhenMockInitializedWithException_ShouldThrowException()
    {
        // Arrange
        var sut = Mock.IBasicMethods(mock => mock.VoidWithoutParameters(() => throw new ArgumentException("Test exception")));

        // Act
        var actual = Assert.Throws<ArgumentException>(() => sut.VoidWithoutParameters());

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("Test exception", actual.Message);
    }

    [Fact]
    public void VoidWithoutParameters_WhenMockNotInitialized_ShouldThrowException3()
    {
        // Arrange
        var isCalled = false;
        var sut = Mock.IBasicMethods(mock => mock.VoidWithoutParameters(() => isCalled = true));

        // Act
        sut.VoidWithoutParameters();

        // Assert
        Assert.True(isCalled, "Should be true when the mock is called");
    }

    [Fact]
    public void VoidWithParameters_WhenMockNotInitialized_ShouldThrowException4()
    {
        // Arrange
        var actual = "";
        var sut = Mock.IBasicMethods(mock => mock.VoidWithParameters(value => actual = value));

        // Act
        sut.VoidWithParameters("Whats in a name");

        // Assert
        Assert.Equal("Whats in a name", actual);
    }

    [Fact]
    public void ReturnWithoutParameters_WhenMockInitializedWithValue_ShouldReturnValue()
    {
        // Arrange
        var sut = Mock.IBasicMethods(mock => mock.ReturnWithoutParameters(() => "Value"));

        // Act
        var actual = sut.ReturnWithoutParameters();

        // Assert
        Assert.Equal("Value", actual);
    }

    [Fact]
    public void ReturnWithParameters_WhenMockInitializedWithValue_ShouldReturnValue()
    {
        // Arrange
        var sut = Mock.IBasicMethods(mock => mock.ReturnWithParameters(name => "Value"));

        // Act
        var actual = sut.ReturnWithParameters("Whats in a name");

        // Assert
        Assert.Equal("Value", actual);
    }

    [Fact]
    public void ReturnWithParameters_WhenMockInitializedWithFunction_ShouldReturnValue()
    {
        // Arrange
        var sut = Mock.IBasicMethods(mock => mock.ReturnWithParameters(value => value));

        // Act
        var actual = sut.ReturnWithParameters("Whats in a name");

        // Assert
        Assert.Equal("Whats in a name", actual);
    }

    [Fact]
    public void CallsShouldBeLoggedToTheLogger()
    {
        // Arrange
        var logger = new CallLog();
        var sut = Mock.IBasicMethods(c => c.ReturnWithParameters(name =>  "test").LogCallsTo(logger));

        // ACT
        sut.ReturnWithParameters("dfa1");
        sut.ReturnWithParameters("dfa2");
        sut.ReturnWithParameters("dfa3");
        sut.ReturnWithParameters("dfa4");
        sut.ReturnWithParameters(null);
    }

    public interface IBasicMethods
    {
        void VoidWithoutParameters();
        void VoidWithParameters(string name);
        string ReturnWithoutParameters();
        string ReturnWithParameters(string name);
    }
}
