namespace SweetMock.FixtureGenerator.FunctionalityTests.BuildInMocks;

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
        var sut = fixture.CreateSut();

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
        var sut = fixture.CreateSut();

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
        var actual = Record.Exception(() => fixture.CreateSut());

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
        var sut = fixture.CreateSut();

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