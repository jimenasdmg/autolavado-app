using System.Net;
using System.Net.Http.Headers;
using System.Text;
using AutoLavadoApp.Services.Core;
using Xunit;

namespace AutoLavadoApp.Tests;

public class FirestoreBaseServiceTests
{
    [Fact]
    public async Task CrearDocumentoAsync_ConflictConDocumentId_HaceFallbackAPatch()
    {
        var handler = new SequenceHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = new StringContent("{\"error\":{\"code\":409,\"message\":\"Document already exists\"}}", Encoding.UTF8, "application/json")
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });

        using var httpClient = new HttpClient(handler);
        var sut = new TestFirestoreService(httpClient);

        var ok = await sut.CrearAsync("servicios", new { fields = new { nombre = new { stringValue = "Lavado" } } }, "doc-1", "token");

        Assert.True(ok);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
        Assert.Equal(HttpMethod.Patch, handler.Requests[1].Method);
        Assert.Contains("documents/servicios?documentId=doc-1", handler.Requests[0].RequestUri!.ToString());
        Assert.Contains("documents/servicios/doc-1", handler.Requests[1].RequestUri!.ToString());
        Assert.Equal("Bearer", handler.Requests[1].Headers.Authorization?.Scheme);
        Assert.Equal("token", handler.Requests[1].Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task CrearDocumentoAsync_SinToken_NoHaceLlamadasYRegresaFalse()
    {
        var handler = new SequenceHttpMessageHandler();
        using var httpClient = new HttpClient(handler);
        var sut = new TestFirestoreService(httpClient);

        var ok = await sut.CrearAsync("servicios", new { fields = new { } }, "doc-1", null);

        Assert.False(ok);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task CrearDocumentoAsync_ConflictSinDocumentId_LanzaFirebaseServiceException()
    {
        var handler = new SequenceHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = new StringContent("{\"error\":{\"code\":409,\"message\":\"Document already exists\"}}", Encoding.UTF8, "application/json")
            });

        using var httpClient = new HttpClient(handler);
        var sut = new TestFirestoreService(httpClient);

        await Assert.ThrowsAsync<AutoLavadoApp.Services.FirebaseServiceException>(() =>
            sut.CrearAsync("servicios", new { fields = new { } }, null, "token"));

        Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
    }

    private sealed class TestFirestoreService(HttpClient httpClient) : FirestoreBaseService(httpClient)
    {
        public Task<bool> CrearAsync(string collection, object body, string? documentId, string? idToken)
            => CrearDocumentoAsync(collection, body, documentId, idToken);
    }

    private sealed class SequenceHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses;

        public SequenceHttpMessageHandler(params HttpResponseMessage[] responses)
        {
            _responses = new Queue<HttpResponseMessage>(responses);
        }

        public List<HttpRequestMessage> Requests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(CloneRequest(request));

            if (_responses.Count == 0)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                });
            }

            return Task.FromResult(_responses.Dequeue());
        }

        private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);
            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (request.Content is not null)
            {
                var body = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                clone.Content = new StringContent(body, Encoding.UTF8, request.Content.Headers.ContentType?.MediaType ?? "application/json");
                if (request.Content.Headers.ContentType is MediaTypeHeaderValue contentType)
                {
                    clone.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType.MediaType)
                    {
                        CharSet = contentType.CharSet
                    };
                }
            }

            return clone;
        }
    }
}
