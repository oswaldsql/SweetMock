namespace Test.EventTests;

using System.ComponentModel;

public class TriggerEventFromConfiguration
{
    [Fact]
    [Mock<INotifyDto>]
    public void ItShouldBePossibleToTriggerEventsFromConfiguration()
    {
        // arrange
        var actual = "";
        var sut = Mock.INotifyDto(config => config.Value(() => "test", _ =>
        {
            config.PropertyChanged(out var trigger);
            trigger(new("Value"));
        }));

        sut.PropertyChanged += (_, args) => actual = args.PropertyName;

        // act
        sut.Value = "dummy";

        // assert
        Assert.Equal("Value", actual);
    }

    [Fact]
    [Mock<IVersionLibrary>]
    public void ItShouldBePossibleToTriggerEventsFromAExposedConfiguration()
    {
        // Arrange
        Version? actual = null;
        MockOf_IVersionLibrary.Config? exposedConfig = null;

        var versionLibrary = Mock.IVersionLibrary(config => exposedConfig = config);

        versionLibrary.NewVersionAdded += (_, version) => actual = version;

        // ACT
        Action<Version> trigger = null!;
        exposedConfig?.NewVersionAdded(out trigger);
        trigger(new(2, 0, 0, 0));

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(new(2, 0, 0, 0), actual);
    }

    public interface INotifyDto : INotifyPropertyChanged
    {
        public string Value { get; set; }
    }
}