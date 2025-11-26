using System.Net;
using System.Net.Http.Json;
using StarshipApi.Models;
using Xunit;

namespace StarshipApi.IntegrationTests
{
    public class PeopleLookupTests : IClassFixture<TestApiFactory>
    {
        private readonly HttpClient _client;

        public PeopleLookupTests(TestApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PeopleLookup_ReturnsListOfPeople()
        {
            var response = await _client.GetAsync("/api/people");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var people = await response.Content.ReadFromJsonAsync<IEnumerable<PersonLookupDto>>();
            Assert.NotNull(people);

            var list = people!.ToList();
            Assert.NotEmpty(list);
            Assert.Contains(list, f => f.Name == "Luke Skywalker");
        }
    }
}