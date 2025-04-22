namespace Test.ConstructorTests;

public class WithParametersTests
{
    [Fact]
    [Mock<ParameterClass>]
    public void CanCreateMocksWithBaseConstructorParameters()
    {
        // Arrange

        // ACT
        var actual = Mock.ParameterClass("oswald", 25);
        var sut = Mock.ParameterClass("name", 10, DateTimeOffset.Now, config => config.IsAgeValid(() => false));

        // Assert
        Assert.Equal(25, actual.Age);
        Assert.Equal("oswald", actual.Name);

        Assert.False(sut.IsAgeValid());
    }

    internal class ParameterClass
    {
        public ParameterClass(string name, int age)
        {
            this.Name = name;
            this.Age = age;
        }

        public ParameterClass(string name, int age, DateTimeOffset birth)
        {
            this.Name = name;
            this.Age = age;
            this.Birth = birth;
        }

        public string Name { get; }
        public int Age { get; }
        public DateTimeOffset Birth { get; } = DateTimeOffset.MinValue;

        public virtual bool IsAgeValid()
        {
            var calculateAge = CalculateAge(this.Birth);
            return this.Age == calculateAge;
        }

        public static int CalculateAge(DateTimeOffset birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}