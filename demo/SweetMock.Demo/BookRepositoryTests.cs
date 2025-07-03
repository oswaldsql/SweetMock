namespace SweetMock.Demo;

[Mock<IUser>]
[Mock<IBasketRepo>]
[Mock<IBookRepository>]
[Mock<IMessageBroker>]
[Mock<IBasket>]
public class BookRepositoryTests
{
    [Fact]
    public async Task TheGuideShouldAlwaysBeAvailable()
    {
        var logger = new CallLog();

        var userGuid = Guid.NewGuid();

        Mock.IBasket();
        var mockUser = Mock.IUser(config => config.Id(userGuid));
        var basket1 = Mock.IBasket(c => c.Add());
        var basketRepo = Mock.IBasketRepo(config => config
            .TryGetUserBasket((Guid _, out IBasket basket, CancellationToken _) =>
            {
                basket = basket1;
                return Task.FromResult(true);
            })
            .Save(), new(logger)
        );
        var bookRepo = Mock.IBookRepository(config => config
            .IsAvailable(returns: true)
            .InStock(42)
            .GetByISBN(new Book("isbn 0-434-00348-4", "The Hitch Hiker's Guide to the Galaxy", "Douglas Adams"))
        );
        var messageBroker = Mock.IMessageBroker(config => config
            .SendMessage()
        );

        var sut = new ShoppingBasket(mockUser, basketRepo, bookRepo, messageBroker);

        await sut.AddBookToBasket("isbn 0-434-00348-4", CancellationToken.None);
//
//        var sut = Mock.IBookRepository(config => config
//            .GetByISBN(new Book("isbn 0-434-00348-4", "The Hitch Hiker's Guide to the Galaxy", "Douglas Adams"))
//            .IsAvailable(returns: true)
//            .InStock(isbn => isbn.Contains("0-434-00348-4") ? 42 : 0),
//            new(logger)
//        );
////
//        var sut2 = Mock.IBookRepository(out var configIBookRepository, new(logger));
//        configIBookRepository.IsAvailable(returns: true);
////
//        var actual = sut.IsAvailable("isbn 0-434-00348-4");
////
//        Assert.True(actual);
////
//        var request = Assert.Single(logger.IsAvailable(args => args.isbn == "isbn 0-434-00348-4"));
    }
}

public class ShoppingBasket(IUser user, IBasketRepo basketRepo, IBookRepository bookRepo, IMessageBroker messageBroker)
{
    public async Task AddBookToBasket(string isbn, CancellationToken token)
    {
        if (!await basketRepo.TryGetUserBasket(user.Id, out var basket, token))
        {
            basket = await basketRepo.CreateUserBasket(user.Id, token);
        }

        if (await bookRepo.IsAvailable(isbn, token) && await bookRepo.InStock(isbn, token) > 1)
        {
            basket.Add(isbn);
            await basketRepo.Save(basket, token);

            var book = await bookRepo.GetByISBN(isbn, token);
            messageBroker.SendMessage(user.Id, $"The book {book.Title} by {book.Author} was added to your basket");
        }
    }
}

public interface IMessageBroker
{
    void SendMessage(Guid userId, string message);
}

public interface IBasketRepo
{
    Task<bool> TryGetUserBasket(Guid userId, out IBasket basket, CancellationToken token);
    Task<IBasket> CreateUserBasket(Guid userId, CancellationToken token);
    Task Save(IBasket basket, CancellationToken token);
}

public interface IBasket
{
    void Add(string isbn);
}

public interface IUser
{
    Guid Id { get; set; }
}

public interface IBookRepository
{
    Task<Book> GetByISBN(string isbn, CancellationToken token);
    Task<bool> IsAvailable(string isbn, CancellationToken token);
    ValueTask<int> InStock(string isbn, CancellationToken token);
}

public record Book(
    string ISBN, // ISBN for tracking and uniqueness
    string Title, // The title of the book
    string Author // The author of the book
);
