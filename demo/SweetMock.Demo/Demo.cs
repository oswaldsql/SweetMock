namespace SweetMock.Demo;

public class Demo
{
    internal interface IDemoDependency
    {
        public string StringProperty { get; set; }
    }

    internal class DemoImplementation : IDemoDependency
    {
        public string StringProperty { get; set; } = "Demo";
    }

    internal class DemoService(IDemoDependency demoDependency)
    {
        public string GetStringProperty() => demoDependency.StringProperty;
    }

    /// <summary>
    ///     Mock and Fixture attributes can be placed on Classes and is the indicator for SweetMock to create a Mock or Fixture for the specified targets.
    /// </summary>
    /// <remarks>
    ///     Mocks and fixtures can be added multiple times in different files but will only be created once.
    ///     Mocks and fixture attributes can also be added to assemblies, methods, properties and a lot of other targets. But generally adding them to test classes is recommended.
    /// </remarks>
    [Mock<IDemoDependency>]
    [Fixture<DemoService>]
    public class AttributeUsage
    {
        [Mock<IDemoDependency>]
        [Fixture<DemoService>]
        [Fact]
        public void AttributeOnMethod() { }
    }

    /// <summary>
    ///     Create a mock object by using the static Mock factory, using the method with the same name as the Mock you want to create.
    ///     The method returns an instance of an object matching the interface or base class.
    ///     Mocks are configured using the config parameter that takes a lambda exposing a fluent interface.
    /// </summary>
    /// <remarks>Mocks can also be created directly by calling new MockOf_{class name}(). This is not recommended.</remarks>
    [Fact]
    public void CreatingMocks()
    {
        // Arrange
        var mock = Mock.IDemoDependency(config =>
        {
            config.StringProperty("Initial value");
        });

        // ACT
        var actual = mock.StringProperty;
        mock.StringProperty = "New value";

        // Assert
        Assert.Equal("Initial value", actual);
        Assert.Equal("New value", mock.StringProperty);
    }

    /// <summary>
    ///     Fixtures simplify creating all the mocks required for a service.
    ///     For each dependency a configuration object is created allowing configuration of the mock.
    /// </summary>
    /// <remarks>
    ///     The configuration of the fixture can also be access by calling fixture.Config allowing for modifying the configuration after creating the sut.
    ///     It is recommended to keep this to a minimum since it makes the configuration harder to read.
    /// </remarks>
    [Fact]
    public void CreatingFixture()
    {
        // Arrange
        var fixture = Fixture.DemoService(config =>
        {
            config.demoDependency.StringProperty("Initial value");
        });
        var sut = fixture.CreateDemoService();

        // ACT
        var actual = sut.GetStringProperty();

        // Assert
        Assert.Equal("Initial value", actual);
    }

    /// <summary>
    ///     If a concrete implementation of a dependency must be used, it can be specified when constructing the sut.
    /// </summary>
    /// <remarks>
    ///     This technique will disable all configuration and logging for the dependency.
    /// </remarks>
    [Fact]
    public void SpecifyingSpecificDependencies()
    {
        // Arrange
        var fixture = Fixture.DemoService(config =>
        {
            config.demoDependency.StringProperty("Initial value"); // Will be ignored
        });

        var implementation = new DemoImplementation { StringProperty = "Implementation value" };
        var sut = fixture.CreateDemoService(
            implementation
        );

        // ACT
        var actual = sut.GetStringProperty();

        // Assert
        Assert.Equal("Implementation value", actual);
    }

    /// <summary>
    ///     When a required member of a dependency is not configured, a NotExplicitlyMockedException will be thrown with information about the reason for the exception.
    /// </summary>
    /// <remarks>
    ///     For Mocks the instance name is the name of the class unless otherwise specified.
    ///     Dependency methods not accessed by the service do not require configuration.
    /// </remarks>
    [Fact]
    public void UnConfiguredDependenciesThrowsException()
    {
        // Arrange
        var fixture = Fixture.DemoService(config =>
        {
            //No dependencies configured.
        });
        var sut = fixture.CreateDemoService();

        // ACT
        var actual = Record.Exception(() => sut.GetStringProperty());

        // Assert
        var actualException = Assert.IsType<NotExplicitlyMockedException>(actual);
        Assert.Equal("'StringProperty' in 'demoDependency' is not explicitly mocked.", actualException.Message);
        Assert.Equal("StringProperty", actualException.MemberName);
        Assert.Equal("demoDependency", actualException.InstanceName);
    }

    // Constructor
    // Methode
    // - Generic
    // - Basic usage
    // - Return
    // - Task ValueTask
    // - ReturnValues
    // - Out
    // - Throw
    // Properties
    // Events
    // Indexers
    // Logging
    // - Log filter
    // Specific mock implementation
    // - ILogger
    // - TimeProvider
    // - IOptions
    // - HttpClient

    // Advanced
    // - Extension methods
    //
}
