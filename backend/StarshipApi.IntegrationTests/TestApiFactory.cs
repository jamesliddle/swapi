using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StarshipApi.Data;
using StarshipApi.Models;
using StarshipApi.Services;

namespace StarshipApi.IntegrationTests
{
    // Isolated test host:
    // - Uses in-memory EF instead of SQL Server
    // - Uses a fake ISwapiFilmService (no real HTTP calls)
    // - Never touches your dev / prod database
    public class TestApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("IntegrationTests");

            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services
                    .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                var dbName = $"StarshipTests_{Guid.NewGuid()}";

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));

                var swapiDescriptor = services
                    .SingleOrDefault(d => d.ServiceType == typeof(ISwapiFilmService));

                if (swapiDescriptor != null)
                {
                    services.Remove(swapiDescriptor);
                }

                services.AddScoped<ISwapiFilmService, FakeSwapiFilmService>();

                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                }
            });
        }
    }

    // Fake SWAPI service so integration tests don't hit the network.
    // You can tweak the data to match what your tests expect.
    internal class FakeSwapiFilmService : ISwapiFilmService
    {
        public Task<string[]> GetFilmTitlesAsync(string filmsJson)
        {
            return Task.FromResult(Array.Empty<string>());
        }

        public Task<IEnumerable<FilmLookupDto>> GetFilmLookupAsync()
        {
            var films = new[]
            {
                new FilmLookupDto
                {
                    Url = "https://swapi.dev/api/films/1/",
                    Title = "A New Hope"
                },
                new FilmLookupDto
                {
                    Url = "https://swapi.dev/api/films/2/",
                    Title = "The Empire Strikes Back"
                }
            };

            return Task.FromResult<IEnumerable<FilmLookupDto>>(films);
        }

        public Task<IEnumerable<PersonLookupDto>> GetPeopleLookupAsync()
        {
            var people = new[]
            {
                new PersonLookupDto
                {
                    Url = "https://swapi.dev/api/people/1/",
                    Name = "Luke Skywalker"
                },
                new PersonLookupDto
                {
                    Url = "https://swapi.dev/api/people/2/",
                    Name = "C-3PO"
                }
            };

            return Task.FromResult<IEnumerable<PersonLookupDto>>(people);
        }
    }
}
