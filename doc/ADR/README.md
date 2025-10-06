# Architectural Decision Record (ADR)

## Framework-wide Decision on Mocking Scope and Implementation in SweetMock

The **SweetMock** framework is being developed to provide a consistent, usable, and comprehensive mocking solution for .NET applications. To achieve these goals, it is essential to define a clear and consistent approach to mocking various constructs in the C# language. This decision encompasses mock creation, supported access levels, and features such as classes, interfaces, events, properties, methods, constructors, indexers, and delegates.

The overall goal is to balance flexibility and functionality while maintaining a manageable level of complexity and ensuring that the framework aligns with standard C# patterns and best practices.

---

### Support for Fixture

- **Bulk creation** : Creation of mocks for use as dependencies in **system under test** classes
- **Creation of SUT** : Build in factory for creating System under tests classes
- **Mock overwrite** : For greater control user which objects to use.
- **Shared CallLog** : For tracking the flow through the calls.

### Supported Targets for Mocking

- **Classes and Interfaces**:
    - **Interfaces** : Mocking **interfaces** is fully supported, due to their frequent use in dependency injection and common design patterns.
    - **Classes** : Mocking of **classes** is fully supported, ensuring the framework is versatile for a wide variety of use cases.
    - **Records** : Mocking of **records** will not currently be supported.
    - **Structs** : Mocking of **struct** will not be supported due to their nature of being sealed.
- **Supported Accessibility**:
    - **Virtual** and **Abstract** members.
    - **Public** and **Protected** members.
    - **Internal** members (if explicitly specified) members.
- **Unsupported Accessibility**:
    - **Sealed** and **non-overridable** members are explicitly not supported.
    - **Private** members are out of scope, with no reflection-based access provided.

### Mock Creation

- **Centralized Creation**: All mocks will be created using a **Mock Factory**. The factory acts as a single, centralized interface to create and configure mocks consistently.
- **Direct Access to Constructors**: Direct use of constructors in mocked objects remains accessible but is discouraged for general use to ensure uniformity.

### Logging and assertion

- **Logging** : All method calls can be logged and used for validation later. 
- **Assertions** : SweetMock will not contain an assertion framework (there are more than enough of them around)

---

## Members

- **Constructors**:
  - All constructors with **Public** or **Protected** access levels are supported.
  - If no constructors are present, a **parameterless constructor** will be automatically generated.
  - **Parameterized constructors**, **constructor overloads**, and dependency injection scenarios will be fully supported.
  - Classes with only **Internal** or **Private** constructors will not be generated for mocking. A warning will be issued to users in such cases.

- **Methods**:
  - **Types of Supported Methods**:
    - **Synchronous** and **Asynchronous** methods (e.g., `Task<>`, `Task`, CancellationToken).
    - **Overloaded** methods.
    - **Generic** methods, including support for `where` constraints.
    - Methods with `out` and `ref` parameters.
  - **Mocking Behavior**:
    - `Call`: Executes a delegate matching the method signature to define custom behavior.
    - `Throw`: Specifies an exception to be thrown when the method is called.
    - `Return`: Specifies a specific return value to return for all calls.
    - `ReturnValues`: Specifies a set of values to return.
    - **Default behavior**: If no configuration is provided, an `InvalidOperationException` will be thrown, clearly identifying the unmocked method and class.
  - **Unsupported**:
    - **Ref return**: Returning `ref` result from methods is not currently supported.  

- **Properties**:
  - **Read-only**, **Set-only** and **Read-write properties** are mockable.
  - Mocking Configurations:
    - `Get/Set`: Delegates defining getter and setter behavior.
    - `Value`: A preset return value for the property.
  - **Default behavior**: If no explicit configuration is provided, getting or setting the property will raise an `InvalidOperationException` with a meaningful error message.

- **Indexers**:
  - Supports both **Read-only**, **Write-only** and **Read-write** indexers.
  - Mocking Configurations:
    - `Get/Set`: Define behavior for the getter and setter delegates.
    - `Values`: Use an internal dictionary as the indexer's backing source.
  - **Default behavior**: Accessing an unmocked indexer will result in an `InvalidOperationException`, clearly stating the missing configuration.

- **Events**:
  - All types of events (standard, custom, or with varying delegate types) are supported.
  - Mocking Configurations:
    - `Raise`: Triggers the event and invokes all subscribed handlers.
    - `Trigger`: Exposes a delegate for programmatic event invocation.
    - `Add/Remove`: Simulates the behavior of `add` and `remove` accessors for event handlers.

---

## Benefits

- **Consistency**:
  - Centralized mock creation ensures uniform use across projects, reducing fragmentation or misuse.
- **Flexibility**:
  - Broad support for critical C# constructs makes the framework suitable for testing diverse scenarios.
- **Usability**:
  - Explicit error handling ensures developers are aware of missing configurations, facilitating easier debugging.
  - Helper methods and centralized mocking logic reduce boilerplate and improve workflow efficiency.
- **Predictability**:
  - Clear accessibility rules prevent unexpected behavior by avoiding reliance on reflection.
- **Versatility**:
  - Features like handling asynchronous methods, generic methods, constructor injection, and event-triggering improve integration with modern design patterns (e.g., Dependency Injection, Publish-Subscribe).

---

## Drawbacks
- **Complexity**:
  - Supporting a wide range of constructs such as methods, constructors, and events increases the complexity of the framework, requiring additional development and maintenance effort.
- **Maintenance Overhead**:
  - Compatibility with evolving C# language features (e.g., new modifiers or constructs) requires continuous updates to remain relevant.
- **Limitations for Advanced Scenarios**:
  - Framework deliberately avoids supporting reflection-based access to private/internal members. This may hinder certain niche use cases for advanced testing.
