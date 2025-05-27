# ![Icon](Icon.png) Sweet Mock

A source generator for creating mocks for testing. SweetMock automatically generates mock implementations of interfaces or classes to help in unit testing by providing a way to simulate the behavior of complex dependencies.

``` csharp
[Mock<IBookRepository>] // Use the Mock attribute to indicate the interface or class to mock.
public class BookRepositoryTests
{
  [Fact]
  public void TheGuideShouldAlwaysBeAvailable() {
    var sut = Mock.IBookRepository(config => config // Get a new instance of the mock 
      .IsAvailable(returns: true)); // Configure what should happen when specific actions are performed

    var actual = sut.IsAvailable("isbn 0-434-00348-4"); 

    Assert.True(actual); // Use your chosen way of assertion 
  }
}
```

## Installation

Install the package via NuGet:

shell dotnet add package SweetMock

## Features

- Source generator-based mocking
- Support for interface mocking
- Support for a subset of class mocking
- No runtime dependencies
- Lightweight and fast
- Native C# code generation

## Usage

Simple example of how to use SweetMock:

## Requirements

- .NET Standard 2.0+
- C# 13.0 or higher

## License

This project is licensed under [LICENSE] - see the LICENSE file for details.

## Project Links

- [GitHub Repository](https://github.com/oswaldsql/SweetMock)
- [NuGet Package](https://www.nuget.org/packages/SweetMock)
