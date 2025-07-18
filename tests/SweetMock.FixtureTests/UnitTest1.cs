namespace SweetMock.FixtureTests;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SweetMock;


[Fixture<ShoppingBasket>]
//[Mock<IG<string>>]
[Mock<IBasketRepo>]
[Mock<IStockHandler>]
[Mock<IUser>]
[Mock<ILogger<ShoppingBasket>>]
public class Tests
{
    [SetUp]
    public void Setup()
    {
        var logger = NullLogger<ShoppingBasket>.Instance;
        
        var fixture = Fixture.ShoppingBasket(config =>
        {
            config.user.GetUserName("fds");
            config.logger.Log(call: (level, id, state, exception, formatter, tState) => logger.Log<object>(level, id, state, exception, (Func<object, object?, string>)formatter));
        });
        var sut = fixture.CreateSut();
        
        foreach (var callLogItem in fixture.CallLog.GetLogs())
        {
            Console.WriteLine(callLogItem);
        }
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
}

public interface IG<T>
{
//    void Do<TU>(TU u);
    void Do2<TU>(Func<TU> u);
}

public class ShoppingBasket(IUser user, IStockHandler stockHandler, IBasketRepo repo, ILogger<ShoppingBasket> logger) { }

public interface IBasketRepo { }

public interface IStockHandler { }

public interface IUser
{
    public string GetUserName();
}

