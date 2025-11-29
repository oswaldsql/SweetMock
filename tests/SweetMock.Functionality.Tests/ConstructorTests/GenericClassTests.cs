namespace Test.ConstructorTests;

[Mock<Repo<GenericClassTests>>]
public class GenericClassTests
{
    [Fact]
    public void CanCreateGenericMock()
    {
        Action<Guid> trigger = null!;
        // Arrange & Act

        Repo_Logs<Guid> callLogs = null!;
        var sut = Mock.Repo<Guid>(config => config
            .SomeMethod(Guid.NewGuid())
            .SomeMethod([Guid.NewGuid(), Guid.NewGuid()])
            .SomeMethod(new ArgumentException())
            .SomeMethod(guid => guid)
            .SomeProperty(Guid.NewGuid())
            .SomeProperty(Guid.NewGuid, _ => { })
            .Indexer(new())
            .Indexer(_ => Guid.NewGuid(), (s, _) => Guid.Parse(s))
            .SomeEvent(Guid.NewGuid())
            .SomeEvent(out trigger)
            .OutMethod(throws: new ArgumentException())
            .OutMethod((out Guid output) => output = Guid.NewGuid())
            .SomeList(() => [Guid.NewGuid()])
            .SomeList(returns: [Guid.NewGuid()])
            .ActionMethod()
            .SomeMethodAsync(returns: Task.FromResult(Guid.NewGuid()))
            .GetCallLogs(out callLogs)
        );

        sut.OutMethod(out _);
        sut.SomeMethod(Guid.NewGuid());
        
        trigger(Guid.NewGuid());

        Assert.NotNull(sut);

        Assert.Single(callLogs.SomeMethod(args => args.input is Guid));
    }

    public class Repo<T> where T : new()
    {
        public virtual T? SomeProperty { get; set; }

        public virtual T this[string key] { get => new(); set { } }

        public virtual T SomeMethod(T input) { return input; }

        public virtual void OutMethod(out T output)
        {
            output = new T();
        }

        public virtual IEnumerable<T> SomeList() => [];

        public virtual void ActionMethod(Action<T> action) => action(default!);

        public virtual Task<T> SomeMethodAsync() => Task.FromResult(new T());
        
        public virtual event EventHandler<T>? SomeEvent;

        protected virtual void OnSomeEvent(T e)
        {
            SomeEvent?.Invoke(this, e);
        }
    }
}