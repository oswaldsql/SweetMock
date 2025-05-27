# Mocking Guidelines

Mocking is a crucial aspect of unit testing, allowing developers to isolate and test individual components of their applications. 
This document provides __opinionated guidelines__ on how to effectively use mocks to mock various targets, ensuring comprehensive and 
reliable tests.

Just as important as knowing what to mock is knowing what not to mock. This document also outlines objects that should not be mocked.

## Good Candidates for Mocking

When writing unit tests, it's important to mock certain components to isolate the unit of work being tested. Here are some good candidates for mocking:

### External Dependencies

External dependencies include databases, file systems, web services, and any other external systems that your code interacts with. 
Mocking these dependencies allows you to test your code without relying on the actual external systems. 
This ensures that your tests are not affected by the availability or state of these external systems, leading to more reliable and faster tests.

Remember to test the actual interactions with external dependencies in integration or end-to-end tests to ensure that your code works correctly with the real systems.

### Planned Components

Components that are planned but not yet implemented, such as interfaces or classes that are part of the design but not yet developed, can be mocked to test how your code interacts with them.
By mocking these components, you can simulate their behavior and test how your code integrates with them before they are fully implemented. 
This helps in identifying potential issues early in the development process.

Remember to update the mocks when the actual components are implemented to ensure that the tests remain relevant and accurate.

### Time-consuming Operations

Operations that are time-consuming, such as network requests, complex calculations or database queries, can be mocked to speed up test execution. 
By mocking these operations, you can simulate their behavior without actually performing the time-consuming tasks, which helps in running tests quickly and efficiently.

Remember to test the actual time-consuming operations in integration or end-to-end tests to ensure that they work correctly.
You can use the mock to test timeout scenarios or error handling without relying on the actual time-consuming operations.

### Volatile Components

Components that return volatile or unpredictable results, such as random number generators and guid generators should be mocked to ensure that your tests are deterministic and reproducible.
By mocking these components, you can control the output and simulate different scenarios to test how your code behaves under various conditions.

Remember to test the actual volatile components in integration or end-to-end tests to ensure that your code works correctly with the real components.
If the components are volatile in production scenarios, remember to test the edge cases in your code.

### Error-prone Components

Components that are error-prone, such as third-party libraries, legacy code, or complex algorithms, should be mocked to test how your code behaves under different error conditions.
By mocking these components, you can simulate error scenarios and ensure that your code handles errors correctly without relying on the actual error-prone components.

Remember to test the actual error-prone components in integration or end-to-end tests to ensure that your code works correctly with the real components.
If the components are error-prone in production scenarios remember to test the error handling logic in your code.

## Specific Examples

### Services

Any service classes that your code depends on, such as authentication services, payment gateways, or email services, should be mocked to isolate the unit of work. 
This allows you to test the logic of your code without relying on the actual implementation of these services, ensuring that your tests are focused and reliable.

Consider isolating the service dependencies behind a facade or adapter to isolate our code from the actual service implementation.

### Repositories

Data access layers or repositories that interact with the database should be mocked to test the business logic without hitting the database. 
This helps in isolating the business logic from the data access logic, making your tests more focused and faster.

Using an in-memory database can be an alternative to mocking repositories, but makes is harder to run tests in parallel and can be slower than using mocks.

### Caching Layers

Mocking caching mechanisms like `IMemoryCache` or `IDistributedCache` allows you to test how your application behaves with different cache states.
This helps in ensuring that your application handles caching correctly without relying on the actual cache implementation.

For scenarios where the cache is not the focus of the test cases, consider using a simple in-memory cache instead of mocking the cache. 

### Message Queues

Mocking message queue clients like `IQueueClient` or `IMessagePublisher` allows you to simulate message sending and receiving without relying on the actual message broker. 
This helps in testing the messaging logic of your application in isolation.

### User Context

Mocking user context providers like `IHttpContextAccessor` allows you to simulate different user scenarios and authentication states. 
This helps in testing how your application behaves under different user contexts without relying on the actual user context implementation.


By mocking these targets, you can create isolated and reliable unit tests that focus on the specific behavior of the code under test.

## Do Not Mock

When writing unit tests, certain components should generally not be mocked:

### Objects with Existing Test Classes

Objects like `ILogger`, `TimeProvider`, and `HttpClient` already have well-defined test classes provided by the framework or libraries. 
These test classes are designed to facilitate testing without the need for mocking. 
Using these existing test classes ensures that your tests are more reliable and maintainable.

### Mapper Classes

Mapper classes are responsible for converting data between different layers of your application. 
Instead of mocking these classes, use the real mapper classes and test the mapping logic. 
This ensures that the mappings are correct and that any changes to the mapping logic are properly tested.

### Validation Logic

Validation logic, such as Fluent Validation, data annotations or custom validation attributes is part of the behavior of your application and should not be mocked.
Instead of mocking the validation logic, test the validation rules directly by passing valid and invalid data to the validation logic and asserting the results.

### Value Objects

Value objects are simple data structures like `DateTime`, `TimeSpan`, or custom value objects. 
These objects should not be mocked because they are simple and have no behavior that needs to be isolated. 
Use real instances of these objects in your tests to ensure that they are used correctly.

### Third-Party Libraries

Avoid mocking third-party libraries directly. Instead, create an abstraction layer around the third-party library and mock that layer. 
This approach ensures that your tests are not tightly coupled to the third-party library and that you can easily replace the library if needed.

### Configuration Settings

Use real configuration settings in your tests to ensure that they are correctly applied. 
Mocking configuration settings can lead to tests that do not accurately reflect the real behavior of your application. 
By using real configuration settings, you can ensure that your tests are more reliable and maintainable.

Mocking these objects can lead to brittle tests that are tightly coupled to the implementation details of the code under test.

## Unsupported Mocking Scenarios

The following scenarios are generally not recommended for mocking and not supported by the SweetMock framework:

### Static Methods and classes

SweetMock uses inheritance-based mocking, which does not support mocking static methods or classes.
Mocking static methods can lead to brittle tests and should be avoided. 
Static methods are tightly coupled to their class and cannot be easily replaced or overridden. 
Instead, refactor the code to use dependency injection, allowing you to inject dependencies that can be mocked.

### Private Methods

Private methods should be tested indirectly through the public methods that call them. 
Testing private methods directly can lead to tests that are tightly coupled to the implementation details, making them brittle and harder to maintain.

### Extension Methods

Extension methods should be tested through the classes they extend rather than mocked directly. This ensures that the extension methods are tested in the context of their usage.

### Sealed Classes

SweetMock uses inheritance-based mocking, which does not support mocking sealed classes.
Consider using interfaces or abstractions to enable mocking. This allows you to create mock implementations for testing purposes.
