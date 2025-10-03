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

namespace System.Net.Http{
    /// <summary>
    ///    Mock implementation of <see cref="global::System.Net.Http.HttpClient">HttpClient</see>.
    ///    Should only be used for testing purposes.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("SweetMock", "{{SweetMockVersion}}")]
    internal class MockOf_HttpClient
    {
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
                    return Task.FromResult((HttpResponseMessage)enumerator.Current);
                });
            }
        }

        internal class MockHandler(MockOf_HttpClient mockOfHttpClient) : HttpMessageHandler
        {
            private MockOptions Options => mockOfHttpClient.Options;

            internal Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> CallBack { get; set; } = (message, _) => Task.FromResult(message.ReplyNotFound());

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Console.WriteLine(this.Options.Logger == null ? "null" : "not null");
                if (this.Options.Logger != null)
                {
                    this.Options.Logger.Add("global::System.Net.Http.HttpClient.SendAsync(HttpRequestMessage, CancellationToken)", Arguments.With("request", request).And("cancellationToken", cancellationToken));
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

    [System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SweetMockVersion}}")]
    internal static class MockOf_HttpMessageHandler_LogExtensions{
        public static HttpClient_Filter HttpClient(this global::System.Collections.Generic.IEnumerable<CallLogItem> source) => new(source);
        public class HttpClient_Filter(global::System.Collections.Generic.IEnumerable<CallLogItem> source) : CallLog_Filter(source){
            protected override string SignatureStart => "global::System.Net.Http.HttpClient.";
        }
#region Constructors
        public class HttpClient_Args : SweetMock.TypedArguments { }

        /// <summary>
        ///    Identifies when the mock object for <see cref="global::System.Net.Http.HttpClient.HttpClient()">HttpClient()</see> <see cref="global::System.Net.Http.HttpClient">HttpMessageHandler.HttpMessageHandler()</see> is created.
        /// </summary>
        public static global::System.Collections.Generic.IEnumerable<HttpClient_Args> HttpClient(this HttpClient_Filter log, Func<HttpClient_Args, bool>? predicate = null) =>
             ((ICallLog_Filter)log).Filter().HttpClient(predicate);

        /// <summary>
        ///    Identifies when the mock object for <see cref="global::System.Net.Http.HttpClient.HttpClient()">HttpClient()</see> <see cref="global::System.Net.Http.HttpClient">HttpMessageHandler.HttpMessageHandler()</see> is created.
        /// </summary>
        public static global::System.Collections.Generic.IEnumerable<HttpClient_Args> HttpClient(this global::System.Collections.Generic.IEnumerable<CallLogItem> log, Func<HttpClient_Args, bool>? HttpClient_HttpClient_Predicate = null) =>
            log.Matching<HttpClient_Args>("global::System.Net.Http.HttpClient.HttpClient()", HttpClient_HttpClient_Predicate);

#endregion

#region Method : SendAsync
        public class SendAsync_Args : SweetMock.TypedArguments{
            /// <summary>
            ///    Enables filtering on the request argument.
            /// </summary>
            public System.Net.Http.HttpRequestMessage request => (System.Net.Http.HttpRequestMessage)base.Arguments["request"]!;

            /// <summary>
            ///    Enables filtering on the cancellationToken argument.
            /// </summary>
            public System.Threading.CancellationToken cancellationToken => (System.Threading.CancellationToken)base.Arguments["cancellationToken"]!;

        }
        /// <summary>
        ///     Identifying calls to the method <see cref="global::System.Net.Http.HttpClient.SendAsync(System.Net.Http.HttpRequestMessage, System.Threading.CancellationToken)">HttpMessageHandler.SendAsync(HttpRequestMessage, CancellationToken)</see>.
        /// </summary>
        public static global::System.Collections.Generic.IEnumerable<SendAsync_Args> SendAsync(this HttpClient_Filter log, Func<SendAsync_Args, bool>? predicate = null) =>
             ((ICallLog_Filter)log).Filter().SendAsync(predicate);

        /// <summary>
        ///    Identifying calls to the method <see cref="global::System.Net.Http.HttpClient.SendAsync(System.Net.Http.HttpRequestMessage, System.Threading.CancellationToken)">HttpMessageHandler.SendAsync(HttpRequestMessage, CancellationToken)</see>.
        /// </summary>
        public static global::System.Collections.Generic.IEnumerable<SendAsync_Args> SendAsync(this global::System.Collections.Generic.IEnumerable<CallLogItem> log, Func<SendAsync_Args, bool>? HttpMessageHandler_SendAsync_Predicate = null) =>
            log.Matching<SendAsync_Args>("global::System.Net.Http.HttpClient.SendAsync(HttpRequestMessage, CancellationToken)", HttpMessageHandler_SendAsync_Predicate);

#endregion

    }

}
