using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StarshipApi.Data;
using StarshipApi.Models;
using StarshipApi.Services;
using System.Net;
using System.Text;
using System.Text.Json;

public class StarshipSeedServiceTests
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private HttpClient CreateHttp(string jsonResponse)
    {
        var handler = new MockHttpHandler(jsonResponse);
        return new HttpClient(handler);
    }

    private void InjectHttp(StarshipSeedService svc, HttpClient client)
    {
        var field = typeof(StarshipSeedService)
            .GetField("_http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        field!.SetValue(svc, client);
    }

    [Fact]
    public void MapJsonToStarship_ParsesFieldsCorrectly()
    {
        string json = """
        {
            "name":"Falcon",
            "model":"YT-1300",
            "manufacturer":"CEC",
            "cost_in_credits":"100",
            "length":"30",
            "pilots":["p1","p2"],
            "films":["f1"],
            "url":"https://swapi.dev/api/starships/10/"
        }
        """;

        var el = JsonDocument.Parse(json).RootElement;

        var svc = new StarshipSeedService(
            new ConfigurationBuilder().Build(),
            CreateDb()
        );

        var method = typeof(StarshipSeedService)
            .GetMethod("MapJsonToStarship",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

        var s = (Starship)method!.Invoke(svc, new object[] { el });

        Assert.Equal("Falcon", s.Name);
        Assert.Equal("YT-1300", s.Model);
        Assert.Equal("CEC", s.Manufacturer);
        Assert.Equal("100", s.CostInCredits);
        Assert.Equal("30", s.Length);
        Assert.Equal("[\"p1\",\"p2\"]", s.Pilots);
        Assert.Equal("[\"f1\"]", s.Films);
        Assert.Equal("https://swapi.dev/api/starships/10/", s.Url);
    }

    [Fact]
    public async Task SeedFromSwapiAsync_InsertsStarships()
    {
        string json = """
        {
            "results":[
                {
                    "name":"A-Wing",
                    "model":"RZ-1",
                    "manufacturer":"Kuat",
                    "pilots":[],
                    "films":[],
                    "url":"https://swapi.dev/api/starships/20/"
                }
            ],
            "next": null
        }
        """;

        var db = CreateDb();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Swapi:Primary"] = "https://example.com/api/starships/"
            })
            .Build();

        var svc = new StarshipSeedService(config, db);

        InjectHttp(svc, CreateHttp(json));

        await svc.SeedFromSwapiAsync();

        var all = db.Starships.ToList();
        Assert.Single(all);
        Assert.Equal("A-Wing", all[0].Name);
    }

    [Fact]
    public async Task SeedFromSwapiAsync_HandlesPagination()
    {
        string page1 = """
        {
            "results":[
                { "name":"Ship1", "url":"https://swapi.dev/api/starships/1/", "pilots":[], "films":[] }
            ],
            "next": "https://example.com/api/starships/?page=2"
        }
        """;

        string page2 = """
        {
            "results":[
                { "name":"Ship2", "url":"https://swapi.dev/api/starships/2/", "pilots":[], "films":[] }
            ],
            "next": null
        }
        """;

        var handler = new MockSequenceHttpHandler(
            new Dictionary<string, string>
            {
                ["https://example.com/api/starships/"] = page1,
                ["https://example.com/api/starships/?page=2"] = page2
            });

        var db = CreateDb();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Swapi:Primary"] = "https://example.com/api/starships/"
            })
            .Build();

        var svc = new StarshipSeedService(config, db);

        InjectHttp(svc, new HttpClient(handler));

        await svc.SeedFromSwapiAsync();

        Assert.Equal(2, db.Starships.Count());
        Assert.Contains(db.Starships, s => s.Name == "Ship1");
        Assert.Contains(db.Starships, s => s.Name == "Ship2");
    }

    [Fact]
    public async Task SeedFromSwapiAsync_HandlesHttpFailure()
    {
        var handler = new AlwaysFailHttpHandler(); // always throws

        var db = CreateDb();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Swapi:Primary"] = "https://example.com/api/starships/"
            })
            .Build();

        var svc = new StarshipSeedService(config, db);

        InjectHttp(svc, new HttpClient(handler));

        await svc.SeedFromSwapiAsync(); // should NOT throw

        Assert.Empty(db.Starships); // nothing added
    }
}

public class MockHttpHandler : HttpMessageHandler
{
    private readonly string _json;

    public MockHttpHandler(string json) => _json = json;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(_json, Encoding.UTF8, "application/json")
        });
    }
}

public class MockSequenceHttpHandler : HttpMessageHandler
{
    private readonly Dictionary<string, string> _map;

    public MockSequenceHttpHandler(Dictionary<string, string> map)
    {
        _map = map;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
    {
        string url = request.RequestUri!.ToString();

        if (_map.TryGetValue(url, out var json))
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}

public class AlwaysFailHttpHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken t)
    {
        throw new HttpRequestException("Simulated failure");
    }
}
