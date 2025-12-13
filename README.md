# ![Icon](Icon.png) Sweet Mock

A source generator for creating mocks and fixtures for testing. SweetMock automatically generates fixture and mock implementations of interfaces or classes to help in unit testing by providing a way to simulate the behavior of complex dependencies.

```csharp
[Fixture<ShoppingBasket>]
[Fact]
public async Task TheGuideShouldAlwaysBeAvailable()
{
    // Arrange
    var userGuid = Guid.NewGuid();

    var fixture = Fixture.ShoppingBasket(config =>
    {
        config.user.Id(userGuid);
        var basket1 = Mock.IBasket(c => c.Add());
        config.basketRepo
            .TryGetUserBasket(Task.FromResult(true), basket1)
            .Save();
        config.bookRepo
            .IsAvailable(true)
            .InStock(42)
            .GetByISBN(new Book("isbn 0-434-00348-4", "The Hitch Hiker's Guide to the Galaxy", "Douglas Adams"));
        config.messageBroker.SendMessage();
    });

    var sut = fixture.CreateShoppingBasket();

    // Act
    await sut.AddBookToBasket("isbn 0-434-00348-4", CancellationToken.None);

    // Assert
    var sendMessage = Assert.Single(fixture.Calls.messageBroker.SendMessage(arguments => arguments.userId == userGuid));
    Assert.Equal("The book The Hitch Hiker's Guide to the Galaxy by Douglas Adams was added to your basket", sendMessage.message);
}
```

## Installation

Install the package via NuGet:

```sh
dotnet add package SweetMock
```

## Features

__SweetMock__ offers a comprehensive set of features designed to streamline unit testing in __C#__ projects. 
Its __source generator code__ simplifies security scanning and ensures that mock implementations are both __fast, reliable and maintainable__.

The __fixture__ system enables bulk creation of mock dependencies, making it easy to set up complex test scenarios while direct access to creating individual __mock__ object allows for more advanced scenarios.

SweetMock supports both __interface__ and __class__ mocking, providing flexibility for a wide range of dependency types. __Built-in mocks__ cover common scenarios, reducing the need for custom implementations.

Extensive __logging__ features allow for detailed assertions, helping you verify interactions and outcomes with precision.

Additionally, SweetMock is test framework __agnostic__, working seamlessly with __XUnit__, __NUnit__, __TUnit__, and other popular frameworks.

## Requirements

- C# 12.0 (.net 8 and above)

## License

This project is licensed under [MIT License](LICENSE)

## Project Links

- [GitHub Repository](https://github.com/oswaldsql/SweetMock)
- [NuGet Package](https://www.nuget.org/packages/SweetMock)
