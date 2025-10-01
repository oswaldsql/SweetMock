namespace SweetMock.FixtureTests;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SweetMock;
using Xunit.Abstractions;

[Fixture<ShoppingBasket>]
[Mock<Tests>]
[Mock<ShoppingBasket>]
[Mock<ISendEndpoint>]
public class Tests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestIsATest()
    {
        var fixture = Fixture.ShoppingBasket(config =>
        {
            config.name = "Tester32";
            config.user.GetUserName("fds");
            config.time.Value = TimeProvider.System;
        });
        var sut = fixture.CreateShoppingBasket();
        
        testOutputHelper.WriteLine(sut.Name);
        
        foreach (var callLogItem in fixture.Log)
        {
            testOutputHelper.WriteLine(callLogItem.ToString());
        }
    }
}

public interface IG<T>
{
//    void Do<TU>(TU u);
    void Do2<TU>(Func<TU> u);
}

public class ShoppingBasket([ServiceKey] string name, TimeProvider time, IUser user, IStockHandler stockHandler, IBasketRepo repo, ILogger<ShoppingBasket> logger)
{
    public TimeProvider Time { get; } = time;
    public IUser User { get; } = user;
    public IStockHandler StockHandler { get; } = stockHandler;
    public IBasketRepo Repo { get; } = repo;
    public ILogger<ShoppingBasket> Logger { get; } = logger;
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

    public interface ISendEndpoint
    {
        Task Send<T>(T message, CancellationToken cancellationToken = default)
            where T : class;

        Task Send<T>(T message, IPipe<SendContext<T>> pipe, CancellationToken cancellationToken = default)
            where T : class;

        Task Send<T>(T message, IPipe<SendContext> pipe, CancellationToken cancellationToken = default)
            where T : class;

        Task Send(object message, CancellationToken cancellationToken = default);

        Task Send(object message, Type messageType, CancellationToken cancellationToken = default);

        Task Send(object message, IPipe<SendContext> pipe, CancellationToken cancellationToken = default);

        Task Send(object message, Type messageType, IPipe<SendContext> pipe, CancellationToken cancellationToken = default);

        Task Send<T>(object values, CancellationToken cancellationToken = default)
            where T : class;

        Task Send<T>(object values, IPipe<SendContext<T>> pipe, CancellationToken cancellationToken = default)
            where T : class;

        Task Send<T>(object values, IPipe<SendContext> pipe, CancellationToken cancellationToken = default)
            where T : class;
    }

    public class SendContext<T>
    {
        
    }
    public class SendContext
    {
    }

    public interface IPipe<T>
    {
    }