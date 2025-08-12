namespace Test.MethodTests;

[Mock<IMultipleReturnValues>]
public class MultipleReturnValuesTests
{
    [Fact]
    public void ItShouldBePossibleToSpecifyMultipleReturnValues()
    {
        // Arrange
        var sut = Mock.IMultipleReturnValues(config => config.Method(["1", "2", "3"]));

        // Act
        var first = sut.Method();
        var second = sut.Method();
        var third = sut.Method();
        var shouldFail = Assert.Throws<SweetMock.NotExplicitlyMockedException>(() => sut.Method());

        // Assert
        Assert.Equal("1", first);
        Assert.Equal("2", second);
        Assert.Equal("3", third);
        Assert.NotNull(shouldFail);
    }

    [Fact]
    public async Task ItShouldBePossibleToSpecifyMultipleReturnValuesForAsyncMethods()
    {
        // Arrange
        var sut = Mock.IMultipleReturnValues(config =>
            config.MethodAsync([Task.FromResult("1"), Task.FromResult("2"), Task.FromResult("3")]));

        // Act
        var token = CancellationToken.None;
        var firstAsync = await sut.MethodAsync(token);
        var secondAsync = await sut.MethodAsync(token);
        var thirdAsync = await sut.MethodAsync(token);
        var shouldFailAsync = await Assert.ThrowsAsync<SweetMock.NotExplicitlyMockedException>(() => sut.MethodAsync(token));

        // Assert
        Assert.Equal("1", firstAsync);
        Assert.Equal("2", secondAsync);
        Assert.Equal("3", thirdAsync);
        Assert.NotNull(shouldFailAsync);
    }

    [Fact]
    public void ItShouldBePossibleToSpecifyMultipleReturnValuesForMethodsWithParameters()
    {
        // Arrange
        var sut = Mock.IMultipleReturnValues(config => config.Method(["1", "2", "3"]));

        // Act
        var callToMethodWithParameter = sut.Method("test");

        // Assert
        Assert.Equal("1", callToMethodWithParameter);
    }

    [Fact]
    public void EmptyValuesAreAllowedAndDefaultsToThrowException()
    {
        // Arrange
        var enumerable = new string[] { };
        var sut = Mock.IMultipleReturnValues(config =>
        {
            config.Method(enumerable);
        });

        // Act
        var shouldFail = Assert.Throws<SweetMock.NotExplicitlyMockedException>(() => sut.Method());

        // Assert
        Assert.NotNull(shouldFail);
    }

    [Fact]
    public void MultipleValuesAreSetIndividuallyOnEachMethod()
    {
        // Arrange
        var sut = Mock.IMultipleReturnValues(config => config.Method(["1", "2", "3"]));

        // ACT
        var callToMethodWithoutParameter = sut.Method();
        var callToMethodWithParameter = sut.Method("test");

        // Assert
        Assert.Equal("1", callToMethodWithoutParameter);
        Assert.Equal("1", callToMethodWithParameter);
    }

    [Fact]
    public async Task ItShouldBePossibleToSpecifyMultipleReturnValueFromAsyncMethods()
    {
        // Arrange
//        var sut = Mock.IMultipleReturnValues(config => config.MethodAsync(returnValues: ["1", "2", "3"]));
        var sut = Mock.IMultipleReturnValues(config => config.MethodAsync(returnValues: [Task.FromResult("1"), Task.FromResult("2"), Task.FromResult("3")]));

        // ACT
        var first = await sut.MethodAsync(CancellationToken.None);
        var second = await sut.MethodAsync(CancellationToken.None);
        var third = await sut.MethodAsync(CancellationToken.None);
        var shouldFail = Assert.ThrowsAsync<InvalidOperationException>(() => sut.MethodAsync(CancellationToken.None));

        // Assert
        Assert.Equal("1", first);
        Assert.Equal("2", second);
        Assert.Equal("3", third);
        Assert.NotNull(shouldFail);
    }

    public interface IMultipleReturnValues
    {
        public string Method();
        public string Method(string value);
        public Task<string> MethodAsync(CancellationToken token);
    }
}
