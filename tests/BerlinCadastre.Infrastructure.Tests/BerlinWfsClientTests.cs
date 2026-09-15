using System.Net;
using BerlinCadastre.Infrastructure.Wfs;

namespace BerlinCadastre.Infrastructure.Tests;

public sealed class BerlinWfsClientTests
{
    [Fact]
    public async Task GetFeaturesAsync_PaginatesUntilShortPage()
    {
        Queue<string> responses = new([
            Collection("a", "b"),
            Collection("c")
        ]);
        RecordingHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responses.Dequeue())
        });
        using HttpClient http = new(handler);
        BerlinWfsClient client = new(http, new WfsGeoJsonParser(), new WfsRequestOptions { PageSize = 2, MaxPages = 5, OutputFormat = "json" });

        WfsFetchResult result = await client.GetFeaturesAsync(new Uri("https://example.test/wfs"), "alkis:test", null, CancellationToken.None);

        Assert.Equal(3, result.Features.Count);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Contains("startIndex=0", handler.Requests[0].Query);
        Assert.Contains("startIndex=2", handler.Requests[1].Query);
        Assert.All(handler.Requests, uri => Assert.Contains("srsName=EPSG%3A25833", uri.Query));
    }

    private static string Collection(params string[] ids) => "{\"type\":\"FeatureCollection\",\"features\":[" + string.Join(',', ids.Select(id => $"{{\"type\":\"Feature\",\"id\":\"{id}\",\"properties\":{{}},\"geometry\":{{\"type\":\"Point\",\"coordinates\":[391000,5820000]}}}}")) + "]}";

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public List<Uri> Requests { get; } = [];
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!);
            return Task.FromResult(responseFactory(request));
        }
    }
}
