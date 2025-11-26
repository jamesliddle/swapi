using System.Net;
using System.Net.Http.Json;
using StarshipApi.Models;
using Xunit;

namespace StarshipApi.IntegrationTests
{
    public class FilmLookupTests : IClassFixture<TestApiFactory>
    {
        private readonly HttpClient _client;

        public FilmLookupTests(TestApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task FilmLookup_ReturnsListOfFilms()
        {
            var response = await _client.GetAsync("/api/films");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var films = await response.Content.ReadFromJsonAsync<IEnumerable<FilmLookupDto>>();
            Assert.NotNull(films);

            var list = films!.ToList();
            Assert.NotEmpty(list);
            Assert.Contains(list, f => f.Title == "A New Hope");
        }
    }
}
