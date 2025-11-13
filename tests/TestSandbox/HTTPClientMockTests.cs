namespace TestSandbox;

using System.Net;
using SweetMock;
using Xunit.Abstractions;

[Fixture<TestFixture>]
public class HttpClientMockTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task ValidateThatHttpClientCanBeMocked()
    {
        var fixture = Fixture.TestFixture(config =>
        {
//            config.client.Send(message => message.ReplyMovedPermanently(new Uri("http://localhost/redir")));
            config.client.Send(request => request switch
            {
                { RequestUri.AbsolutePath: "/tester", Method: { Method: "GET" } } => request.ReplyOk().WithJsonContent((string[])["t1", "t2"]),
                { RequestUri.AbsolutePath: var p } when p.Contains("test2") => request.ReplyBadRequest(),
                { RequestUri.Query: "test3" } => request.ReplyInternalServerError().WithHtmlContent("<ups/>"),
                { Version.Major: 1, Version.Minor: 0 } => new HttpResponseMessage(HttpStatusCode.ExpectationFailed),

                _ => new HttpResponseMessage(HttpStatusCode.NotFound)
            });
//            config.client.Send(new HttpResponseMessage(HttpStatusCode.OK));
            config.client.Send([new (HttpStatusCode.OK), new(HttpStatusCode.NotModified)]);
        });

        var sut = fixture.CreateTestFixture();
        testOutputHelper.WriteLine(await sut.GetResult());
        
        //await sut.Client.GetAsync("http://localhost/");
        //await sut.Client.GetAsync("http://localhost/");
        //
        //var result = await sut.Client.GetAsync("http://localhost/");
        //Console.WriteLine(result);

        foreach (var callLogItem in fixture.Log)
        {
            testOutputHelper.WriteLine(callLogItem.ToString());
        }

        Assert.Single(fixture.Log.HttpClient().SendAsync(t => t.request.Method == HttpMethod.Get));
    }
    
    protected HttpResponseMessage Send2(HttpRequestMessage request) =>
        request switch
        {
            { RequestUri.AbsolutePath: "/tester", Method: { Method: "GET" } } => request.ReplyOk().WithJsonContent((string[])["t1", "t2"]),
            { RequestUri.AbsolutePath: var p } when p.Contains("test2") => request.ReplyBadRequest(),
            { RequestUri.Query: "test3" } => request.ReplyInternalServerError().WithHtmlContent("<ups/>"),
            { Version.Major: 1, Version.Minor: 0 } => new HttpResponseMessage(HttpStatusCode.ExpectationFailed),

            _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        };
}

public class TestFixture(HttpClient client)
{
    public async Task<string> GetResult()
    {
        return await client.GetStringAsync("http://localhost/tester");
    }
}