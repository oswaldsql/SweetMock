namespace SweetMock.FixtureGenerator.FunctionalityTests.BuildInMocks;

using System.Net;
using Microsoft.Extensions.Options;

[Fixture<OptionsTarget>]
[Fixture<OptionsTarget2>]
public class IOptionsTest
{
    [Fact]
    public void OptionsWithAEmptyConstructorIsAutomaticallyCreated()
    {
        // Arrange
        var fixture = Fixture.OptionsTarget();
        var sut = fixture.CreateOptionsTarget();

        // ACT
        var actual = sut.ReturnOptionValue();

        // Assert 
        Assert.Equal("Initial value", actual);
    }

    [Fact]
    public void OptionsWithASetValueIsReturned()
    {
        // Arrange
        var fixture = Fixture.OptionsTarget(config => 
            config.options.Set(new() { SomeProperty = "Set value" }));
        var sut = fixture.CreateOptionsTarget();

        // ACT
        var actual = sut.ReturnOptionValue();

        // Assert 
        Assert.Equal("Set value", actual);
    }
    
    [Fact]
    public void OptionsWithACtorShouldThrowExceptionIsNotSet()
    {
        // Arrange
        var fixture = Fixture.OptionsTarget2();
        
        // ACT
        var actual = Record.Exception(() => fixture.CreateOptionsTarget2());

        // Assert
        var actualException = Assert.IsType<ArgumentNullException>(actual);
        Assert.StartsWith("'options' must have a value before being used.", actualException.Message);
        Assert.Equal("options", actualException.ParamName);
    }
        
    [Fact]
    public void OptionsWithACtorShouldWorkIfSet()
    {
        // Arrange
        var fixture = Fixture.OptionsTarget2(config => 
            config.options.Set(new("Ctor value")));
        var sut = fixture.CreateOptionsTarget2();

        // ACT
        var actual = sut.ReturnOptionValue();

        // Assert 
        Assert.Equal("Ctor value", actual);
    }
    
    public class OptionsTarget(IOptions<TargetOptions> options)
    {
        public string ReturnOptionValue() => options.Value.SomeProperty;
    }
    
    public class TargetOptions
    {
        public string SomeProperty { get; set; } = "Initial value";
    }
    
    public class OptionsTarget2(IOptions<TargetOptionsWithCtor> options)
    {
        public string ReturnOptionValue() => options.Value.SomeProperty;
    }
    
    public class TargetOptionsWithCtor(string inputValue)
    {
        public string SomeProperty { get; set; } = inputValue;
    }
}
