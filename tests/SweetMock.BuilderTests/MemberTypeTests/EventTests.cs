// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace SweetMock.BuilderTests.MemberTypeTests;

public class EventTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void EventRepositoryTests()
    {
        var source = Build.TestClass<IEventRepository>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetErrors());
    }

    [Fact]
    public void NotifyPropertyChangedTests()
    {
        var source = Build.TestClass<TestNotifyPropertyChanged>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetErrors());
    }

    public interface IEventRepository
    {
        delegate void SampleEventHandler(object sender, string pe);

        event EventHandler SimpleEvent;
        event EventHandler<string> EventWithArgs;
        event SampleEventHandler CustomEvent;
    }

    public class TestNotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}