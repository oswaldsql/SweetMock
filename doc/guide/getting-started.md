# Getting started

## Installation and First Use

Reference the NuGet package in your test project:

```sh
dotnet add package SweetMock
```

## Create a test fixture

- Specify the system under test (sut) class to test by using the `[Fixture<MyService>]`.
- Create the test fixture by calling `Fixture.MyService()`.
- Use `Fixture.MyService(config => {<mock dependency>})` to configure how the different dependency mocks should behave.
- Create an instance of the service with mock dependencies by calling `var sut = Fixture.CreateMyService()`.
- Perform the test actions of your system under test.
- Use the CallLog for assertions by using `fixture.Log`

```csharp
[Fixture<ShoppingBasket>] // Use the Fixture attribute to specify the system under test (sut) class
[Fact]
public async Task TheGuideShouldAlwaysBeAvailable()
{
    // Arrange
    var fixture = Fixture.ShoppingBasket(config => // Create the test fixture
    {
        config.bookRepo.GetByISBN(new Book("isbn 0-434-00348-4", "The Hitch Hiker's Guide to the Galaxy", "Douglas Adams")); // configure the dependency mocks
        config.messageBroker.SendMessage();
    });
    
    var sut = fixture.CreateShoppingBasket(); // Create an instance of the sut
    
    // Act
    await sut.AddBookToBasket("isbn 0-434-00348-4", CancellationToken.None); // Execute the test
    
    // Assert
    var sendMessage = Assert.Single(fixture.Log.IMessageBroker().SendMessage()); // Assert using the CallLog
    Assert.Equal("The book The Hitch Hiker's Guide to the Galaxy by Douglas Adams was added to your basket", sendMessage.message);
}
```

### Create mocks yourself

- Specify the interface or class to create a mock for by using `[Mock<IBookRepository>]`
- Create an instance of the mock object by calling `Mock.IBookRepository()`
- Configure the mock `Mock.IBookRepository(config => config.<mock configuation>);`
- Inject the mock into your sut as if it was a standard dependency.

```csharp
[Mock<IBookRepository>] // Use the Mock attribute to indicate the interface or class to mock.
[Fact]
public void TheGuideShouldAlwaysBeAvailable() {
    // Arrange
    var mockRepo = Mock.IBookRepository(config => config // Get a new instance of the mock 
        .IsAvailable(returns: true)); // Configure what should happen when specific actions are performed
    
    // Act
    var sut = new ShoppingBasket(MockRepo);
    var actual = sut.IsAvailable("isbn 0-434-00348-4"); 
    
    // Assert
    Assert.True(actual); // Use your chosen way of assertion 
}
```

For more details on specific aspects you can read about [Construction](construction.md), [Methods](methods.md), [Properties](properties.md), [Events](events.md) or
[Indexers](indexers.md).
