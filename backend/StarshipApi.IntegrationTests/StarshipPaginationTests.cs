using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StarshipApi.Controllers;
using StarshipApi.Data;
using StarshipApi.Models;
using StarshipApi.Services;
using Xunit;

namespace StarshipApi.IntegrationTests
{
    public class StarshipPaginationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _db;
        private readonly StarshipsController _controller;

        public StarshipPaginationTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new AppDbContext(options);
            _db.Database.EnsureCreated();

            _controller = new StarshipsController(_db, new FakeSwapiFilmService());
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }

        private void SeedMany(int count)
        {
            for (int i = 1; i <= count; i++)
            {
                _db.Starships.Add(new Starship
                {
                    Name = $"Ship {i}",
                    Model = $"Model {i}",
                    Manufacturer = "Corellian",
                    CostInCredits = (1000 + i).ToString(),
                    Length = "10",
                    MaxAtmospheringSpeed = "1000",
                    Crew = "1",
                    Passengers = "0",
                    CargoCapacity = "100",
                    Consumables = "1 week",
                    HyperdriveRating = "1.0",
                    MGLT = "100",
                    StarshipClass = "Starfighter",
                    Pilots = JsonSerializer.Serialize(Array.Empty<string>()),
                    Films = JsonSerializer.Serialize(Array.Empty<string>()),
                    Url = $"https://swapi.dev/api/starships/{i}/"
                });
            }

            _db.SaveChanges();
        }

        [Fact]
        public async Task Pagination_Works_AsExpected()
        {
            SeedMany(30);

            var result = await _controller.GetAll(
                name: null,
                model: null,
                manufacturer: null,
                cost: null,
                sortBy: "name",
                sortDir: "asc",
                page: 2,
                pageSize: 10
            ) as OkObjectResult;

            Assert.NotNull(result);

            var payload = result!.Value!;
            var type = payload.GetType();

            var total = (int)type.GetProperty("total")!.GetValue(payload)!;
            var items = (System.Collections.IEnumerable)type.GetProperty("items")!.GetValue(payload)!;

            Assert.Equal(30, total);

            var list = items.Cast<object>().ToList();
            Assert.Equal(10, list.Count);

            var nameProp = list[0].GetType().GetProperty("Name");
            Assert.NotNull(nameProp);

            // SQLite sorts strings alphabetically, so page 2 begins at "Ship 19"
            var expectedNames = new[]
            {
                "Ship 19", "Ship 2", "Ship 20", "Ship 21", "Ship 22",
                "Ship 23", "Ship 24", "Ship 25", "Ship 26", "Ship 27"
            };

            for (int i = 0; i < expectedNames.Length; i++)
            {
                string actual = nameProp!.GetValue(list[i]) as string ?? "";
                Assert.Equal(expectedNames[i], actual);
            }
        }

        [Fact]
        public async Task Filtering_ByName_Works()
        {
            SeedMany(10);

            _db.Starships.Add(new Starship
            {
                Name = "Falcon Heavy",
                Model = "FH-1",
                Manufacturer = "SpaceX",
                CostInCredits = "999999",
                Length = "70",
                MaxAtmospheringSpeed = "50000",
                Crew = "4",
                Passengers = "10",
                CargoCapacity = "1000",
                Consumables = "2 months",
                HyperdriveRating = "0.5",
                MGLT = "200",
                StarshipClass = "Heavy",
                Pilots = "[]",
                Films = "[]",
                Url = "https://swapi.dev/api/starships/999/"
            });

            _db.SaveChanges();

            var result = await _controller.GetAll(
                name: "Falcon",
                model: null,
                manufacturer: null,
                cost: null,
                sortBy: "name",
                sortDir: "asc",
                page: 1,
                pageSize: 50
            ) as OkObjectResult;

            Assert.NotNull(result);

            var payload = result!.Value!;
            var type = payload.GetType();

            var total = (int)type.GetProperty("total")!.GetValue(payload)!;
            var items = (System.Collections.IEnumerable)type.GetProperty("items")!.GetValue(payload)!;

            Assert.Equal(1, total);

            var list = items.Cast<object>().ToList();
            Assert.Single(list);

            var ship = list[0];
            var nameProp = ship.GetType().GetProperty("Name");

            Assert.Equal("Falcon Heavy", nameProp!.GetValue(ship));
        }

        private sealed class FakeSwapiFilmService : ISwapiFilmService
        {
            public Task<string[]> GetFilmTitlesAsync(string filmsJson) =>
                Task.FromResult(Array.Empty<string>());

            public Task<System.Collections.Generic.IEnumerable<FilmLookupDto>> GetFilmLookupAsync() =>
                Task.FromResult<System.Collections.Generic.IEnumerable<FilmLookupDto>>(
                    Array.Empty<FilmLookupDto>());

            public Task<System.Collections.Generic.IEnumerable<PersonLookupDto>> GetPeopleLookupAsync() =>
                Task.FromResult<System.Collections.Generic.IEnumerable<PersonLookupDto>>(
                    Array.Empty<PersonLookupDto>());
        }
    }
}
