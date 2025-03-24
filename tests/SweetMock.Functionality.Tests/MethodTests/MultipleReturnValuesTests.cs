namespace Test.MethodTests;

[Mock<IMultipleReturnValues>]
public class MultipleReturnValuesTests
{
    [Fact(Skip = "Waiting for extension methods")]
    public void ItShouldBePossibleToSpecifyMultipleReturnValues()
    {
//        // Arrange
//        var sut = Mock.IMultipleReturnValues(config => config.Method(["1", "2", "3"]));
//
//        // Act
//        var first = sut.Method();
//        var second = sut.Method();
//        var third = sut.Method();
//        var shouldFail = Assert.Throws<InvalidOperationException>(() => sut.Method());
//
//        // Assert
//        Assert.Equal("1", first);
//        Assert.Equal("2", second);
//        Assert.Equal("3", third);
//        Assert.NotNull(shouldFail);
    }

    [Fact(Skip = "Waiting for extension methods")]
    public async Task ItShouldBePossibleToSpecifyMultipleReturnValuesForAsyncMethods()
    {
//        // Arrange
//        var sut = Mock.IMultipleReturnValues(config =>
//            config.MethodAsync([Task.FromResult("1"), Task.FromResult("2"), Task.FromResult("3")]));
//
//        // Act
//        var token = CancellationToken.None;
//        var firstAsync = await sut.MethodAsync(token);
//        var secondAsync = await sut.MethodAsync(token);
//        var thirdAsync = await sut.MethodAsync(token);
//        var shouldFailAsync = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.MethodAsync(token));
//
//        // Assert
//        Assert.Equal("1", firstAsync);
//        Assert.Equal("2", secondAsync);
//        Assert.Equal("3", thirdAsync);
//        Assert.NotNull(shouldFailAsync);
    }

    [Fact(Skip = "Waiting for extension methods")]
    public void ItShouldBePossibleToSpecifyMultipleReturnValuesForMethodsWithParameters()
    {
//        // Arrange
//        var sut = Mock.IMultipleReturnValues(config => config.Method(["1", "2", "3"]));
//
//        // Act
//        var callToMethodWithParameter = sut.Method("test");
//
//        // Assert
//        Assert.Equal("1", callToMethodWithParameter);
    }

    [Fact(Skip = "Waiting for extension methods")]
    public void EmptyValuesAreAllowedAndDefaultsToThrowException()
    {
//        // Arrange
//        var enumerable = new string[] { };
//        var sut = Mock.IMultipleReturnValues(config =>
//        {
//            config.Method(enumerable);
//        });
//
//        // Act
//        var shouldFail = Assert.Throws<InvalidOperationException>(() => sut.Method());
//
//        // Assert
//        Assert.NotNull(shouldFail);
    }

    [Fact(Skip = "Waiting for extension methods")]
    public void MultipleValuesAreSetIndividuallyOnEachMethod()
    {
//        // Arrange
//        var sut = Mock.IMultipleReturnValues(config => config.Method(["1", "2", "3"]));
//
//        // ACT
//        var first = sut.Method();
//        var CallToMethodWithParameter = sut.Method("test");
//
//        // Assert
//        Assert.Equal("1", first);
//        Assert.Equal("1", CallToMethodWithParameter);
    }

    [Fact(Skip = "Waiting for extension methods")]
    public void ItShouldBePossibleToSpecifyMultipleReturnValue()
    {
//        // Arrange
//        var sut = Mock.IMultipleReturnValues(config => config.Method(["1", "2", "3"]).MethodAsync(["1", "2", "3"]));
//
//        // ACT
//        var first = sut.Method();
//        var second = sut.Method();
//        var third = sut.Method();
//        var shouldFail = Assert.Throws<InvalidOperationException>(() => sut.Method());
//
//        var CallToMethodWithParameter = sut.Method("test");
//
//        // Assert
//        Assert.Equal("1", first);
//        Assert.Equal("2", second);
//        Assert.Equal("3", third);
//        Assert.NotNull(shouldFail);
//
//        Assert.Equal("1", CallToMethodWithParameter);
    }

    public interface IMultipleReturnValues
    {
        public string Method();
        public string Method(string value);
        public Task<string> MethodAsync(CancellationToken token);
    }
}
