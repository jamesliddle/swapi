using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using StarshipApi.Models;
using StarshipApi.Services;
using System.Net;
using System.Text;

public class SwapiFilmServiceTests
{
    private IMemoryCache CreateMemoryCache()
    {
        return new MemoryCache(new MemoryCacheOptions());
    }

    private IHttpClientFactory CreateFactory(HttpClient client)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
               .Returns(client);

        return factory.Object;
    }

    private HttpClient CreateMockHttp(string json)
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        return new HttpClient(handler.Object);
    }

    private HttpClient CreateMockHttpException()
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("boom"));

        return new HttpClient(handler.Object);
    }

    [Fact]
    public async Task GetFilmLookupAsync_ReturnsFilmList()
    {
        string json = """
        {
            "results": [
                { "url": "u1", "title": "A New Hope" },
                { "url": "u2", "title": "Empire Strikes Back" }
            ],
            "next": null
        }
        """;

        var client = CreateMockHttp(json);
        var svc = new SwapiFilmService(CreateFactory(client), CreateMemoryCache());

        var result = await svc.GetFilmLookupAsync();

        Assert.Equal(2, result.Count());
        Assert.Contains(result, f => f.Url == "u1" && f.Title == "A New Hope");
        Assert.Contains(result, f => f.Url == "u2" && f.Title == "Empire Strikes Back");
    }

    [Fact]
    public async Task GetFilmLookupAsync_CachesResults()
    {
        string json = """
        {
            "results": [
                { "url": "u1", "title": "A New Hope" }
            ],
            "next": null
        }
        """;

        var client = CreateMockHttp(json);
        var cache = CreateMemoryCache();
        var svc = new SwapiFilmService(CreateFactory(client), cache);

        var first = await svc.GetFilmLookupAsync();
        var second = await svc.GetFilmLookupAsync(); // should hit cache; no HTTP call

        Assert.Single(first);
        Assert.Single(second);
        Assert.Equal("A New Hope", second.First().Title);
    }

    [Fact]
    public async Task GetFilmLookupAsync_WhenHttpFails_ReturnsEmpty()
    {
        var client = CreateMockHttpException();
        var svc = new SwapiFilmService(CreateFactory(client), CreateMemoryCache());

        var result = await svc.GetFilmLookupAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPeopleLookupAsync_ReturnsPeopleList()
    {
        string json = """
        {
            "results": [
                { "url": "p1", "name": "Luke Skywalker" },
                { "url": "p2", "name": "Han Solo" }
            ],
            "next": null
        }
        """;

        var client = CreateMockHttp(json);
        var svc = new SwapiFilmService(CreateFactory(client), CreateMemoryCache());

        var result = await svc.GetPeopleLookupAsync();

        Assert.Equal(2, result.Count());
        Assert.Contains(result, p => p.Url == "p1" && p.Name == "Luke Skywalker");
    }

    [Fact]
    public async Task GetPeopleLookupAsync_WhenHttpFails_ReturnsEmpty()
    {
        var client = CreateMockHttpException();
        var svc = new SwapiFilmService(CreateFactory(client), CreateMemoryCache());

        var result = await svc.GetPeopleLookupAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFilmTitlesAsync_ReturnsTitleList()
    {
        string json = """ { "title": "Return of the Jedi" } """;

        var client = CreateMockHttp(json);
        var svc = new SwapiFilmService(CreateFactory(client), CreateMemoryCache());

        string urlsJson = """ ["http://x/api/films/3/"] """;

        var result = await svc.GetFilmTitlesAsync(urlsJson);

        Assert.Single(result);
        Assert.Equal("Return of the Jedi", result[0]);
    }

    [Fact]
    public async Task GetFilmTitlesAsync_ReturnsUnavailableOnHttpError()
    {
        var client = CreateMockHttpException();
        var svc = new SwapiFilmService(CreateFactory(client), CreateMemoryCache());

        string urlsJson = """ ["http://x/api/films/99/"] """;

        var result = await svc.GetFilmTitlesAsync(urlsJson);

        Assert.Single(result);
        Assert.Equal("(unavailable)", result[0]);
    }

    [Fact]
    public async Task GetFilmTitlesAsync_UsesCache()
    {
        string json = """ { "title": "Phantom Menace" } """;

        var client = CreateMockHttp(json);
        var cache = CreateMemoryCache();
        var svc = new SwapiFilmService(CreateFactory(client), cache);

        string urlsJson = """ ["http://x/api/films/4/"] """;

        var first = await svc.GetFilmTitlesAsync(urlsJson);
        var second = await svc.GetFilmTitlesAsync(urlsJson); // Should use cache

        Assert.Equal("Phantom Menace", second[0]);
    }

    [Fact]
    public async Task GetFilmTitlesAsync_ReturnsEmpty_WhenJsonInvalid()
    {
        var svc = new SwapiFilmService(CreateFactory(CreateMockHttp("{}")), CreateMemoryCache());

        var result = await svc.GetFilmTitlesAsync("INVALID JSON");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFilmTitlesAsync_ReturnsEmpty_WhenEmptyArray()
    {
        var svc = new SwapiFilmService(CreateFactory(CreateMockHttp("{}")), CreateMemoryCache());

        var result = await svc.GetFilmTitlesAsync("[]");

        Assert.Empty(result);
    }
}
