namespace SweetMock.Demo;

[Mock<IBookRepository>]
public class BookRepositoryTests
{
    [Fact]
    public void TheGuideShouldAlwaysBeAvailable()
    {
        var logger = new CallLog();

        var sut = Mock.IBookRepository(config => config
            .GetByISBN(new Book("isbn 0-434-00348-4", "The Hitch Hiker's Guide to the Galaxy", "Douglas Adams"))
            .IsAvailable(returns: true)
            .InStock(isbn => isbn.Contains("0-434-00348-4") ? 42 : 0),
            new(logger)
        );

        var sut2 = Mock.IBookRepository(out var configIBookRepository, new(logger));
        configIBookRepository.IsAvailable(returns: true);

        var actual = sut.IsAvailable("isbn 0-434-00348-4");

        Assert.True(actual);

        var request = Assert.Single(logger.IsAvailable(args => args.isbn == "isbn 0-434-00348-4"));
    }
}

public interface IBookRepository
{
    Book GetByISBN(string isbn);
    bool IsAvailable(string isbn);
    int InStock(string isbn);
}

public record Book(
    string ISBN,            // ISBN for tracking and uniqueness
    string Title,          // The title of the book
    string Author         // The author of the book
);
