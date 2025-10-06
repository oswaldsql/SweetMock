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
            .IsAvailable(true)
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

[Fixture<ShoppingBasket>]
public class BookRepositoryTests2
{
    [Fixture<ShoppingBasket>]
    [Fact]
    public async Task TheGuideShouldAlwaysBeAvailable()
    {
        var userGuid = Guid.NewGuid();

        var fixture = Fixture.ShoppingBasket(config =>
        {
            config.user.Id(userGuid);
            var basket1 = Mock.IBasket(c => c.Add());
            config.basketRepo
                .TryGetUserBasket(Task.FromResult(true), basket1)
                .Save();
            config.bookRepo
                .IsAvailable(true)
                .InStock(42)
                .GetByISBN(new Book("isbn 0-434-00348-4", "The Hitch Hiker's Guide to the Galaxy", "Douglas Adams"));
            config.messageBroker.SendMessage();
        });

        var sut = fixture.CreateShoppingBasket();

        await sut.AddBookToBasket("isbn 0-434-00348-4", CancellationToken.None);

        foreach (var item in fixture.Log)
        {
            Console.WriteLine(item);
        }

        var sendMessage = Assert.Single(fixture.Log.IMessageBroker().SendMessage());
        Assert.Equal("The book The Hitch Hiker's Guide to the Galaxy by Douglas Adams was added to your basket", sendMessage.message);
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
    public void SendMessage(Guid userId, string message);
}

public interface IBasketRepo
{
    public Task<bool> TryGetUserBasket(Guid userId, out IBasket basket, CancellationToken token);
    public Task<IBasket> CreateUserBasket(Guid userId, CancellationToken token);
    public Task Save(IBasket basket, CancellationToken token);
}

public interface IBasket
{
    public void Add(string isbn);
}

public interface IUser
{
    public Guid Id { get; set; }
}

public interface IBookRepository
{
    public Task<Book> GetByISBN(string isbn, CancellationToken token);
    public Task<bool> IsAvailable(string isbn, CancellationToken token);
    public ValueTask<int> InStock(string isbn, CancellationToken token);
}

public record Book(
    string ISBN, // ISBN for tracking and uniqueness
    string Title, // The title of the book
    string Author // The author of the book
);
