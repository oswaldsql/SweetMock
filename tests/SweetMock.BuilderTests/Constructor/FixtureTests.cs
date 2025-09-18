namespace SweetMock.BuilderTests.Constructor;

public class FixtureTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void SpecifyingOnlyFixtureShouldAddDependencies()
    {
        // Arrange
        var source = """
                     namespace Demo;

                     using SweetMock.BuilderTests;
                     using SweetMock;
                     using System;

                     [Fixture<ShoppingBasket>]
                     public class ShoppingBasket(IUser user, IStockHandler stockHandler, IBasketRepo repo, ILogger<ShoppingBasket> logger) { }

                     public interface IBasketRepo { }

                     public interface IStockHandler { }

                     public interface IUser { }

                     public interface ILogger<T> {}
                     """;

        var generate = new SweetMockSourceGenerator().Generate(source);

        var code = generate.syntaxTrees.ToArray();

        testOutputHelper.DumpResult(code, generate.diagnostics);

        Assert.Empty(generate.GetErrors());
    }    
}