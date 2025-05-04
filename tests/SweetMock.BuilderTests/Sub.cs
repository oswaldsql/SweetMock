namespace SweetMock.BuilderTests;

using Util;

public class Sub
{
    [Fact]
    public void METHOD()
    {
        // Arrange
        var namedTypeSymbol = SymbolHelper.GetClassSymbol("public class Name {}", "Name");

        // ACT
        Console.WriteLine(namedTypeSymbol);
        
        // Assert 
    }
}