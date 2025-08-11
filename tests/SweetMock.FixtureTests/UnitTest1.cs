namespace SweetMock.FixtureTests;

using CustomMocks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SweetMock;

[Fixture<ShoppingBasket>]
[Mock<TimeProvider, MockOf_TimeProvider>]
[Mock<String, MockOf_String>]
[Mock<ILogger<string>, MockOf_ILogger<string>>]
[Mock<Tests>]
[Mock<ShoppingBasket>]
public class Tests
{
    [SetUp]
    public void Setup()
    {
        var fixture = Fixture.ShoppingBasket(config =>
        {
            config.name.Value = "Tester32";
            config.user.GetUserName("fds");
            config.time.Value = TimeProvider.System;
            config.logger.Value = NullLogger<ShoppingBasket>.Instance;
        });
        var sut = fixture.CreateSut();
        
        Console.WriteLine(sut.Name);
        
        foreach (var callLogItem in fixture.Log.GetLogs())
        {
            Console.WriteLine(callLogItem);
        }
    }
}

public interface IG<T>
{
//    void Do<TU>(TU u);
    void Do2<TU>(Func<TU> u);
}

public class ShoppingBasket(string name, TimeProvider time, IUser user, IStockHandler stockHandler, IBasketRepo repo, ILogger<ShoppingBasket> logger)
{
    public string Name => name;
}

public interface IBasketRepo { }

public interface IStockHandler { }

public interface IUser
{
    public string GetUserName();
}

public class User : IUser
{
    public string GetUserName() => "";
}