# SweetMock

SweetMock offers a **minimalistic** approach to mocking in .NET with a focus on simplicity and ease of use. 

```csharp
    public interface IBookRepository
    {
        Task<Guid> AddBook(Book book, CancellationToken token);
        int BookCount { get; set; }
        Book this[Guid index] { get; set; }
        event EventHandler<Book> NewBookAdded;
    }

    [Fact]
    [Mock<IBookRepository>]
    public async Task BookCanBeCreated()
    {
        Action<Book> trigger = _ => { };

        var mockRepo = Mock.IBookRepository(config => config
            .AddBook(returns: Guid.NewGuid())
            .BookCount(value: 10)
            .Indexer(values: new Dictionary<Guid, Book>())
            .NewBookAdded(trigger: out trigger));
        
        var sut = new BookModel(mockRepo);
        var actual = await sut.AddBook(new Book());
        
        Assert.Equal("We now have 10 books", actual);
    }
```

Try it out or continue with [Getting started](guide/getting-started.md) to learn more or read the [Mocking guidelines](guide/mocking-guidelines.md) to get a better understanding of when, why and how to mock and when not to.

For more details on specific aspects you can read about [Construction](guide/construction.md), [Methods](guide/methods.md), [Properties](guide/properties.md), [Events](guide/events.md) or 
[Indexers](guide/indexers.md).

If you are more into the ins and outs of SweetMock you can read the [ADR](ADR/README.md).
