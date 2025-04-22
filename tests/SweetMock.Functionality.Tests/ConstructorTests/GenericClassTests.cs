namespace Test.ConstructorTests;

[Mock<Repo<GenericClassTests>>]
public class GenericClassTests
{
    [Fact]
    public void CanCreateGenericMock()
    {
        // Arrange & Act
        var actual = Mock.Repo<GenericClassTests>();

        // Assert
        Assert.NotNull(actual);
    }
    
    public class Repo<T> where T : new()
    {
        public virtual T SomeMethod(T input) => input;
        public virtual T SomeProperty { get; set; }
        public virtual T this[string key] { get { return new T();} set{} }
        public virtual event EventHandler<Version> NewVersionAdded;
    }
}