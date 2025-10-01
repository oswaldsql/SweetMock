namespace TestSandbox;

using System.Net;
using MassTransit;
using MassTransit.TestFramework;
using NUnit.Framework;
using SweetMock;

[Fixture<TestFixture>]
[Mock<ConsumeContext<Payload>, MockOfConsumeContext<Payload>>]
[Mock<HttpClient>]
[Mock<HttpMessageHandler>]
public class HttpClientMockTests
{
    [Test]
    public async Task ValidateThatHttpClientCanBeMocked()
    {
        var fixture = Fixture.TestFixture(config =>
        {
            config.context.Value = new TestConsumeContext<string>("test");
            config.client.Send((message) => new HttpResponseMessage(HttpStatusCode.Ambiguous));
        });

        var sut = fixture.CreateTestFixture();

        var result = await sut.Client.GetAsync("http://localhost/");
        Console.WriteLine(result);
    }
}

public class TestFixture(ConsumeContext<string> context, HttpClient client)
{
    public ConsumeContext<string> Context { get; } = context;
    public HttpClient Client { get; } = client;
}

public class Payload
{
}

internal class MockOfConsumeContext<T> : MockBase<ConsumeContext<T>> where T : class;