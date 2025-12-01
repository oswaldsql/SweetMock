// ReSharper disable RedundantNullableDirective
// ReSharper disable ArrangeNamespaceBody
// ReSharper disable RedundantNameQualifier
// ReSharper disable RedundantBaseQualifier
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
#nullable enable

using global::SweetMock;
using global::System.Text.Json;
using global::System.Threading;
using global::System.Threading.Tasks;

/*

namespace System.Net.Http{
    /// <summary>
    ///    Mock implementation of <see cref="global::System.Net.Http.HttpClient">HttpClient</see>.
    ///    Should only be used for testing purposes.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("SweetMock", "{{SweetMockVersion}}")]
    internal class MockOf_HttpClient : HttpClient
    {
        private const string _containerName = "global::System.Net.Http.HttpClient";
        private void _log(global::SweetMock.ArgumentBase argument) {this.Options.Logger.Add(argument);}

        public MockOf_HttpClient()
        {
            var handler = new MockHandler(this);
            this.Config = new MockConfig(handler);
            this.Value = new HttpClient(handler);
        }

        public global::SweetMock.MockOptions Options { get; set; } = new(instanceName: "HttpClient" );

        public MockConfig Config { get; init; }
        internal HttpClient Value { get; init; }

        public class MockConfig(MockHandler handler)
        {
            public void SendAsync(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> callBack) =>
                handler.CallBack = callBack;

            public void Send(Func<HttpRequestMessage, HttpResponseMessage> callBack) =>
                this.SendAsync((message, _) => Task.FromResult(callBack(message)));

            public void Send(HttpResponseMessage returns) =>
                this.SendAsync((_, _) => Task.FromResult(returns));

            public void Send(HttpResponseMessage[] returnValues)
            {
                var enumerator = returnValues.GetEnumerator();
                this.SendAsync((_, _) =>
                {
                    enumerator.MoveNext();
                    return Task.FromResult((HttpResponseMessage)enumerator.Current!);
                });
            }
        }

        public record SendAsync_Arguments(
            global::System.String? InstanceName,
            global::System.String MethodSignature,
            global::System.Net.Http.HttpRequestMessage? request = null, global::System.Threading.CancellationToken? cancellationToken = null
        ) : ArgumentBase(_containerName, "SendAsync", MethodSignature, InstanceName);

        internal class MockHandler(MockOf_HttpClient mockOfHttpClient) : HttpMessageHandler
        {
            private MockOptions Options => mockOfHttpClient.Options;

            internal Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> CallBack { get; set; } = (message, _) => Task.FromResult(message.ReplyNotFound());

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                mockOfHttpClient._log(new SendAsync_Arguments(mockOfHttpClient._sweetMockInstanceName, "SendAsync(System.Net.Http.HttpRequestMessage, System.Threading.CancellationToken)", request : request, cancellationToken : cancellationToken));
                Console.WriteLine(this.Options.Logger == null ? "null" : "not null");
                if (this.Options.Logger != null)
                {
                    this.Options.Logger.Add(new SendAsync_Arguments() "global::System.Net.Http.HttpClient.SendAsync(HttpRequestMessage, CancellationToken)", Arguments.With("request", request).And("cancellationToken", cancellationToken));
                }

                return this.CallBack.Invoke(request, cancellationToken);
            }
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


}
*/
