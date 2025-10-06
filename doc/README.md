# ![Icon](Icon.png) Sweet Mock

SweetMock offers a **minimalistic** approach to mocking in .NET with a focus on simplicity and ease of use. 

```csharp
[Fixture<ShoppingBasket>]
[Fact]
public async Task TheGuideShouldAlwaysBeAvailable()
{
    // Arrange
    var fixture = Fixture.ShoppingBasket(config =>
    {
        config.bookRepo.GetByISBN(new Book("isbn 0-434-00348-4", "The Hitch Hiker's Guide to the Galaxy", "Douglas Adams"));
        config.messageBroker.SendMessage();
    });
    
    var sut = fixture.CreateShoppingBasket();
    
    // Act
    await sut.AddBookToBasket("isbn 0-434-00348-4", CancellationToken.None);
    
    // Assert
    var sendMessage = Assert.Single(fixture.Log.IMessageBroker().SendMessage());
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

Try it out or continue with [Getting started](guide/getting-started.md) to learn more or read the [Mocking guidelines](guide/mocking-guidelines.md) to get a better understanding of when, why and how to mock and when not to.

For more details on specific aspects you can read about [Construction](guide/construction.md), [Methods](guide/methods.md), [Properties](guide/properties.md), [Events](guide/events.md) or 
[Indexers](guide/indexers.md).

If you are more into the ins and outs of SweetMock you can read the [ADR](ADR/README.md).
