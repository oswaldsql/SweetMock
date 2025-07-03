namespace Test.ConstructorTests;

public class ParameterLessTests
{
    [Fact]
    [Mock<ParameterLessClass>]
    public void ParameterlessConstructorCanBeUsed()
    {
        // Arrange

        // ACT
        var sut = Mock.ParameterLessClass();

        // Assert
        Assert.IsAssignableFrom<ParameterLessClass>(sut);
        Assert.True(sut.CtorIsCalled);
        Assert.True(sut.ImplicitCtorIsCalled);
    }

    [Fact]
    [Mock<IInterfaceWithoutCtor>]
    public void InterfaceWithoutCtorCanBeUsed()
    {
        // Arrange

        // ACT
        var sut = Mock.IInterfaceWithoutCtor();

        // Assert
        Assert.IsAssignableFrom<IInterfaceWithoutCtor>(sut);
    }

    public class ParameterLessClass
    {
        public bool CtorIsCalled { get; set; } = true;

        public bool ImplicitCtorIsCalled { get; set; } = true;
    }

    public interface IInterfaceWithoutCtor
    {
    }
}