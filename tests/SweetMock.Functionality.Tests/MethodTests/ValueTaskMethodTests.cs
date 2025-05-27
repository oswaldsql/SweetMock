namespace Test.MethodTests;

[Mock<IValueTasks>]
public class ValueTaskMethodTests
{
    [Fact]
    public void SimpleValueTaskWithoutParametersCanBeCalledWithoutArguments()
    {
        // Arrange
        var sut = Mock.IValueTasks(t => t.SimpleTask(() => ValueTask.CompletedTask));

        // ACT
        var actual = sut.SimpleTask();

        // Assert
        Assert.Equal(ValueTask.CompletedTask, actual);
    }

    [Fact]
    public void SimpleValueTaskWithoutParametersCanBeCalledWithReturnsValue()
    {
        // Arrange
        var sut = Mock.IValueTasks(t => t.SimpleTask(() => ValueTask.CompletedTask));

        // ACT
        var actual = sut.SimpleTask();

        // Assert
        Assert.Equal(ValueTask.CompletedTask, actual);
    }

    [Fact]
    public void SimpleValueTaskWithoutParametersCanBeCalledWithCallArgument()
    {
        // Arrange
        var sut = Mock.IValueTasks(t => t.SimpleTask(() => ValueTask.CompletedTask));

        // ACT
        var actual = sut.SimpleTask();

        // Assert
        Assert.Equal(ValueTask.CompletedTask, actual);
    }

    [Fact]
    public async Task SimpleValueTaskWithoutParametersCanBeCalledWithoutReturnValues()
    {
        // Arrange
        var sut = Mock.IValueTasks(t => t.SimpleTask([ValueTask.CompletedTask, ValueTask.CompletedTask]));

        // ACT
        var actual = sut.SimpleTask();
        await sut.SimpleTask();

        Exception? actualException = null;
        try
        {
            await sut.SimpleTask();
        }
        catch (Exception e)
        {
            actualException = e;
        }

        // Assert
        Assert.NotNull(actualException);
        Assert.Equal(ValueTask.CompletedTask, actual);
        Assert.IsType<InvalidOperationException>(actualException);
    }

    [Fact]
    public void SimpleValueTaskWithoutParametersCanGiveAException()
    {
        // Arrange
        var sut = Mock.IValueTasks(t => t.SimpleTask(() => ValueTask.FromException(new Exception())));
 
        // ACT
        var actual = Record.ExceptionAsync(async () => await sut.SimpleTask());
 
        // Assert
        Assert.IsType<ValueTask<Exception>>(actual);
    }
 
    [Fact]
    public async Task SimpleValueTaskWithoutParametersCanBeCancled()
    {
        // Arrange
        var sut = Mock.IValueTasks(t => t.SimpleTask(() => ValueTask.FromCanceled(new CancellationToken(true))));
 
        // ACT
        var actual = await Record.ExceptionAsync(async () => await sut.SimpleTask());
 
        // Assert
        Assert.IsType<TaskCanceledException>(actual);
    }
 
    [Fact]
    public async Task SimpleValueTaskShouldHaveAllTheExpectedHelperMethods()
    {
        // Arrange
        var sut = Mock.IValueTasks(t => t
            .SimpleTask(() => ValueTask.CompletedTask)
            .SimpleTask(ValueTask.CompletedTask)
            .SimpleTask([ValueTask.CompletedTask, ValueTask.CompletedTask])
            .SimpleTask(new InvalidOperationException())
            .SimpleTask());

        // ACT
        await sut.SimpleTask();

        // Assert
        Assert.NotNull(sut);
    }

    [Fact]
    public async Task SimpleValueWithParametersTaskShouldHaveAllTheExpectedHelperMethods()
    {
        // Arrange
        var sut = Mock.IValueTasks(t => t
            .SimpleTaskWithArgs(_ => ValueTask.CompletedTask)
            .SimpleTaskWithArgs(ValueTask.CompletedTask)
            .SimpleTaskWithArgs([ValueTask.CompletedTask, ValueTask.CompletedTask])
            .SimpleTaskWithArgs(new InvalidOperationException())
            .SimpleTaskWithArgs());

        // ACT
        await sut.SimpleTaskWithArgs("name");

        // Assert
        Assert.NotNull(sut);
    }

    [Fact(Skip = "ReturnValues dos not yet work with automatically wrapping the task")]
    public async Task TaskWithResultTaskShouldHaveAllTheExpectedHelperMethods()
    {
        // Arrange
        var sut = Mock.IValueTasks(t => t
            .TaskWithResult(call: () => ValueTask.FromResult("name"))
            .TaskWithResult(ValueTask.FromResult("name"))
            .TaskWithResult([ValueTask.FromResult("name1"), ValueTask.FromResult("name2")])
            //.TaskWithResult(["name1", "name2"])
            //.TaskWithResult(() => "name")
            .TaskWithResult(new InvalidOperationException())
            //.TaskWithResult("name")
        );

        // ACT
        var actual = await sut.TaskWithResult();

        // Assert
        Assert.NotNull(sut);
        Assert.Equal("name", actual);
    }

    [Fact(Skip = "ReturnValues dos not yet work with automatically wrapping the task")]
    public async Task TaskWithResultWithArgshouldHaveAllTheExpectedHelperMethods()
    {
        // Arrange
        var sut = Mock.IValueTasks(t => t
            .TaskWithResultWithArgs((name, _) => ValueTask.FromResult(name))
            .TaskWithResultWithArgs(ValueTask.FromResult("name"))
            .TaskWithResultWithArgs([ValueTask.FromResult("name1"), ValueTask.FromResult("name2")])
            //.TaskWithResultWithArgs(["name1", "name2"])
            .TaskWithResultWithArgs(new InvalidOperationException())
            //.TaskWithResultWithArgs("name")
        );
        
        // ACT
        var result = await sut.TaskWithResultWithArgs("name", CancellationToken.None);

        // Assert
        Assert.NotNull(sut);
        Assert.Equal("name", result);
    }

    public interface IValueTasks
    {
        ValueTask SimpleTask();
        ValueTask SimpleTaskWithArgs(string name);

        ValueTask<string> TaskWithResult();
        ValueTask<string> TaskWithResultWithArgs(string name, CancellationToken ct = default);
    }
}
