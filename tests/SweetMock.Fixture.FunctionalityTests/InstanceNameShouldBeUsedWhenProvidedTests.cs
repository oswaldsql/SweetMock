namespace SweetMock.FixtureGenerator.FunctionalityTests;

[Mock<IInstanceName>]
public class InstanceNameShouldBeUsedWhenProvidedTests
{
    [Fact]
    public void WhenInstanceNameIsNotSetClassNameShouldBeUsed()
    {
        // Arrange
        var sut = Mock.IInstanceName();

        // ACT
        var actualMethodException = Assert.Throws<NotExplicitlyMockedException>(() => sut.Method());
        var actualPropertyException = Assert.Throws<NotExplicitlyMockedException>(() => sut.Property = "");
        var actualIndexerException = Assert.Throws<NotExplicitlyMockedException>(() => sut[12] = "fds");

        // Assert 
        Assert.Equal("IInstanceName", actualMethodException.InstanceName);
        Assert.Equal("IInstanceName", actualPropertyException.InstanceName);
        Assert.Equal("IInstanceName", actualIndexerException.InstanceName);
    }
    
    [Fact]
    public void WhenInstanceNameIsSetItIsUsed()
    {
        // Arrange
        var sut = Mock.IInstanceName(_ => {}, new(instanceName:"instance"));

        // ACT
        var actualMethodException = Assert.Throws<NotExplicitlyMockedException>(() => sut.Method());
        var actualPropertyException = Assert.Throws<NotExplicitlyMockedException>(() => sut.Property = "");
        var actualIndexerException = Assert.Throws<NotExplicitlyMockedException>(() => sut[12] = "fds");

        // Assert 
        Assert.Equal("instance", actualMethodException.InstanceName);
        Assert.Equal("instance", actualPropertyException.InstanceName);
        Assert.Equal("instance", actualIndexerException.InstanceName);
    }
    
    public interface IInstanceName
    {
        public string Property { get; set; }
        public string Method();
        public string this[int i] { get; set; }
    }
}