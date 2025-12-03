namespace TestSandbox;

using System.Net;
using System.Text.Json;
using SweetMock;
using Xunit.Abstractions;

[Fixture<TestFixture>]
[Mock<HttpMessageHandler>]
public class HttpClientMockTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task ValidateThatHttpClientCanBeMocked()
    {
        var fixture = Fixture.TestFixture(config =>
        {
//            config.client.Send(message => message.ReplyMovedPermanently(new Uri("http://localhost/redir")));
            config.client.Send((request, token) => request switch
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

        foreach (var callLogItem in fixture.Logs.client.All())
        {
            testOutputHelper.WriteLine(callLogItem.ToString());
        }

        //Assert.Single(fixture.Logs.SendAsync(t => t.request.Method == HttpMethod.Get));
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

    public static class MockOf_HttpClientConfig_Extensions
    {
        public static HttpResponseMessage Reply(this HttpRequestMessage request, System.Net.HttpStatusCode statusCode) => new(statusCode) { RequestMessage = request, Version = request.Version};

        public static HttpResponseMessage ReplyOk(this HttpRequestMessage request) => request.Reply(HttpStatusCode.OK);
        public static HttpResponseMessage ReplyCreated(this HttpRequestMessage request) => request.Reply(HttpStatusCode.Created);
        public static HttpResponseMessage ReplyNoContent(this HttpRequestMessage request) => request.Reply(HttpStatusCode.NoContent);
        public static HttpResponseMessage ReplyMovedPermanently(this HttpRequestMessage request, string uri)
        {
            var result = request.Reply(HttpStatusCode.MovedPermanently);
            result.Headers.Location = new Uri(uri);
            return result;
        }
        public static HttpResponseMessage ReplyMovedPermanently(this HttpRequestMessage request, Uri uri)
        {
            var result = request.Reply(HttpStatusCode.MovedPermanently);
            result.Headers.Location = uri;
            return result;
        }

        public static HttpResponseMessage ReplyFound(this HttpRequestMessage request, string uri)
        {
            var result = request.Reply(System.Net.HttpStatusCode.Found);
            result.Headers.Location = new Uri(uri);
            return result;
        }
        public static HttpResponseMessage ReplyFound(this HttpRequestMessage request, Uri uri)
        {
            var result = request.Reply(HttpStatusCode.Found);
            result.Headers.Location = uri;
            return result;
        }


        public static HttpResponseMessage ReplyNotModified(this HttpRequestMessage request) => request.Reply(HttpStatusCode.NotModified);
        public static HttpResponseMessage ReplyBadRequest(this HttpRequestMessage request) => request.Reply(HttpStatusCode.BadRequest);

        /// <summary>
        /// For information about what parameters to use refere to <a href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/WWW-Authenticate">Mozilla documentation</a>
        /// </summary>
        /// <param name="request">The request to use as a source of the response.</param>
        /// <param name="scheme">The authentication schema.</param>
        /// <param name="parameter">The authentication parameters</param>
        /// <returns>A response message.</returns>
        public static HttpResponseMessage ReplyUnauthorized(this HttpRequestMessage request, string scheme, string? parameter = null)
        {
            var result = request.Reply(HttpStatusCode.Unauthorized);
            result.Headers.WwwAuthenticate.Add(new(scheme, parameter));
            return result;
        }

        public static HttpResponseMessage ReplyForbidden(this HttpRequestMessage request) => request.Reply(HttpStatusCode.Forbidden);
        public static HttpResponseMessage ReplyNotFound(this HttpRequestMessage request) => request.Reply(HttpStatusCode.NotFound);
        public static HttpResponseMessage ReplyMethodNotAllowed(this HttpRequestMessage request) => request.Reply(HttpStatusCode.MethodNotAllowed);
        public static HttpResponseMessage ReplyConflict(this HttpRequestMessage request) => request.Reply(HttpStatusCode.Conflict);
        public static HttpResponseMessage ReplyUnsupportedMediaType(this HttpRequestMessage request) => request.Reply(HttpStatusCode.UnsupportedMediaType);
        public static HttpResponseMessage ReplyTooManyRequests(this HttpRequestMessage request) => request.Reply(HttpStatusCode.TooManyRequests);
        public static HttpResponseMessage ReplyInternalServerError(this HttpRequestMessage request) => request.Reply(HttpStatusCode.InternalServerError);
        public static HttpResponseMessage ReplyBadGateway(this HttpRequestMessage request) => request.Reply(HttpStatusCode.BadGateway);
        public static HttpResponseMessage ReplyServiceUnavailable(this HttpRequestMessage request) => request.Reply(HttpStatusCode.ServiceUnavailable);
        public static HttpResponseMessage ReplyGatewayTimeout(this HttpRequestMessage request) => request.Reply(HttpStatusCode.GatewayTimeout);

        public static HttpResponseMessage WithContent(this HttpResponseMessage response, HttpContent content, string contentType)
        {
            response.Content = content;
            response.Content.Headers.ContentType = new(contentType);
            return response;
        }

        public static HttpResponseMessage WithHtmlContent(this HttpResponseMessage response, string html)
            => response.WithContent(new StringContent(html), "text/html");

        public static HttpResponseMessage WithJsonContent(this HttpResponseMessage response, string json)
            => response.WithContent(new StringContent(json), "application/json");

        public static HttpResponseMessage WithJsonContent(this HttpResponseMessage response, object jsonSource)
        {
            var json = JsonSerializer.Serialize(jsonSource, JsonSerializerOptions.Web);
            return response.WithContent(new StringContent(json), "application/json");
        }

        public static HttpResponseMessage WithHeader(this HttpResponseMessage response, string headerName, string headerValue)
        {
            response.Headers.Add(headerName, headerValue);
            return response;
        }
    }


internal class FixtureFor_TestFixture_2{
    internal class FixtureConfig{
        /// <summary>
        ///    Configuration object for the fixture
        /// </summary>
        /// <param name="client">Configuring the client (<see cref="global::System.Net.Http.HttpClient">HttpClient</see>) mock for the fixture <see cref="global::TestSandbox.TestFixture">TestFixture</see>.</param>
        internal FixtureConfig(global::System.Net.Http.MockOf_HttpClient.MockConfig client){
            this.client = client;
        }

        /// <summary>
        ///    Gets the configuration for client used within the fixture.
        /// </summary>
        internal global::System.Net.Http.MockOf_HttpClient.MockConfig client {get;private set;}
    }

    /// <summary>
    ///    Gets or sets the configuration object for the fixture used in the test setup process.
    ///    This property enabled configuration and management of the mocked dependencies.
    /// </summary>
    internal FixtureConfig Config{get; private set;}

    private readonly global::System.Net.Http.HttpClient _client;

    /// <summary>
    ///    Gets the call log used to record method invocations and interactions within the mocked dependencies during the test execution process.
    ///    This property facilitates the tracking and validation of method calls made on the mocks in the scope of the unit tests.
    /// </summary>
    private global::SweetMock.CallLog _log;
    public FixtureLogger Logs {get; private set;}
    internal class FixtureLogger(global::SweetMock.CallLog callLog) : global::SweetMock.FixtureLog_Base(callLog){
        public global::System.Net.Http.MockOf_HttpClient.HttpClient_Logs client = new(callLog, "client");

    }

    /// <summary>
    ///    Provides a fixture for the <see cref="global::TestSandbox.TestFixture">TestFixture</see> object, setting up mocks and a call log for testing purposes.
    /// </summary>
    /// <param name="config">Optional configuration of the mocked dependencies.</param>
    public FixtureFor_TestFixture_2(System.Action<FixtureConfig>? config = null){
        _log = new SweetMock.CallLog();
        Logs = new FixtureLogger(_log);

//        global::System.Net.Http.MockOf_HttpClient.MockConfig temp_client = null!;
//        _client = new System.Net.Http.MockOf_HttpClient(config => temp_client = config, new global::SweetMock.MockOptions(_log, "client"));

        MockOf_HttpClient.MockConfig temp_client = null!;
        _client = Mock.HttpClient(out temp_client, new global::SweetMock.MockOptions(_log, "client"));
        
        Config = new FixtureConfig(temp_client);
        config?.Invoke(Config);
    }

    /// <summary>
    ///    Creates an instance of the <see cref="global::TestSandbox.TestFixture">TestFixture</see> object using the initialized mock dependencies.
    /// </summary>
    /// <param name="client">Explicitly sets the value for client bypassing the values created by the fixture.</param>
    /// <returns>A <see cref="global::TestSandbox.TestFixture">TestFixture</see> instance configured with mocked dependencies.</returns>
    public global::TestSandbox.TestFixture CreateTestFixture(System.Net.Http.HttpClient? client = null){
        var argument_client = client ?? _client;
        return new global::TestSandbox.TestFixture(argument_client);
    }
}