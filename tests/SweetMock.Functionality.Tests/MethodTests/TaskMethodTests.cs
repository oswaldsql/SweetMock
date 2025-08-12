// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace Test.MethodTests;

[Mock<IAsyncTaskMethods>]
public class TaskMethodTests
{
    [Fact]
    public async Task SimpleTask_WhenMockNotInitialized_ShouldThrowException()
    {
        // Arrange
        var sut = Mock.IAsyncTaskMethods();

        // Act
        var actual = await Assert.ThrowsAsync<SweetMock.NotExplicitlyMockedException>(() => sut.SimpleTask("Whats in a name"));

        // Assert
        Assert.NotNull(actual);
        Assert.Contains("SimpleTask", actual.Message);
    }

    [Fact]
    public async Task SimpleTask_WhenMockInitializedWithException_ShouldThrowException()
    {
        // Arrange
        var sut = Mock.IAsyncTaskMethods(mock => mock.SimpleTask(_ => throw new ArgumentException("Test Exception")));

        // Act and Assert
        var actual = await Assert.ThrowsAsync<ArgumentException>(() => sut.SimpleTask("Whats in a name"));

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("Test Exception", actual.Message);
    }

    [Fact]
    public async Task SimpleTask_WhenMockInitializedWithAction_ShouldCallAction()
    {
        // Arrange
        var actual = "";
        var sut = Mock.IAsyncTaskMethods(mock => mock.SimpleTask(name =>
        {
            actual = name;
            return Task.CompletedTask;
        }));

        // Act and Assert
        await sut.SimpleTask("Whats in a name");

        // Assert
        Assert.Equal("Whats in a name", actual);
    }

    [Fact]
    public async Task SimpleTask_WhenMockInitializedWithTaskFunction_ShouldCallAction()
    {
        // Arrange
        var actual = "";
        var sut = Mock.IAsyncTaskMethods(mock => mock.SimpleTask(value =>
        {
            actual = value;
            return Task.CompletedTask;
        }));

        // Act and Assert
        await sut.SimpleTask("Whats in a name");

        // Assert
        Assert.Equal("Whats in a name", actual);
    }

    [Fact]
    public async Task SimpleTask_WhenMockInitializedWithTaskCompleted_ShouldNotFail()
    {
        // Arrange
        var sut = Mock.IAsyncTaskMethods(mock => mock.SimpleTask(_ => Task.CompletedTask));

        // Act and Assert
        await sut.SimpleTask("Whats in a name");

        // Assert
    }

    [Fact]
    public async Task TaskWithResult_WhenMockNotInitialized_ShouldThrowException()
    {
        // Arrange
        var sut = Mock.IAsyncTaskMethods();

        // Act
        var actual = await Assert.ThrowsAsync<SweetMock.NotExplicitlyMockedException>(() => sut.TaskWithResult("Whats in a name"));

        // Assert
        Assert.NotNull(actual);
        Assert.Contains("TaskWithResult", actual.Message);
    }

    [Fact]
    public async Task TaskWithResult_WhenMockInitializedWithException_ShouldThrowException()
    {
        // Arrange
        var sut = Mock.IAsyncTaskMethods(mock => mock.TaskWithResult(_ => throw new ArgumentException("Test Exception")));

        // Act and Assert
        var actual = await Assert.ThrowsAsync<ArgumentException>(() => sut.TaskWithResult("Whats in a name"));

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("Test Exception", actual.Message);
    }

    [Fact]
    public async Task TaskWithResult_WhenMockInitializedWithAction_ShouldCallAction()
    {
        // Arrange
        var actual = "";
        var sut = Mock.IAsyncTaskMethods(mock => mock.TaskWithResult(value =>
        {
            actual = value;
            return Task.FromResult(value);
        }));

        // Act and Assert
        await sut.TaskWithResult("Whats in a name");

        // Assert
        Assert.Equal("Whats in a name", actual);
    }

    [Fact]
    public async Task TaskWithResult_WhenMockInitializedWithTaskFunction_ShouldCallAction()
    {
        // Arrange
        var actual = "";
        var sut = Mock.IAsyncTaskMethods(mock => mock.TaskWithResult(value =>
        {
            actual = value;
            return Task.FromResult("Result");
        }));

        // Act and Assert
        var actualReturnValue = await sut.TaskWithResult("Whats in a name");

        // Assert
        Assert.Equal("Whats in a name", actual);
        Assert.Equal("Result", actualReturnValue);
    }

    [Fact]
    public async Task TaskWithResult_WhenMockInitializedWithTaskCompleted_ShouldNotFail()
    {
        // Arrange
        var sut = Mock.IAsyncTaskMethods(mock => mock.TaskWithResult(_ => Task.FromResult("Return")));

        // Act
        var actual = await sut.TaskWithResult("Whats in a name");

        // Assert
        Assert.Equal("Return", actual);
    }

    public interface IAsyncTaskMethods
    {
        Task SimpleTask(string name);

        Task<string> TaskWithResult(string name);
    }
}
