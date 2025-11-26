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
    // Integration-level tests that exercise StarshipsController + EF Core + SQLite in-memory.
    // No WebApplicationFactory / HTTP needed — but still "real" DB + controller.
    public class StarshipsControllerIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _db;
        private readonly StarshipsController _controller;

        public StarshipsControllerIntegrationTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new AppDbContext(options);
            _db.Database.EnsureCreated();

            ISwapiFilmService fakeFilmService = new FakeSwapiFilmService();

            _controller = new StarshipsController(_db, fakeFilmService);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }

        private Starship SeedStarship(string name = "X-Wing")
        {
            var ship = new Starship
            {
                Name = name,
                Model = "T-65",
                Manufacturer = "Incom Corporation",
                CostInCredits = "100000",
                Length = "12.5",
                MaxAtmospheringSpeed = "1050",
                Crew = "1",
                Passengers = "0",
                CargoCapacity = "110",
                Consumables = "1 week",
                HyperdriveRating = "1.0",
                MGLT = "100",
                StarshipClass = "Starfighter",
                Pilots = JsonSerializer.Serialize(Array.Empty<string>()),
                Films = JsonSerializer.Serialize(Array.Empty<string>()),
                Url = "https://swapi.dev/api/starships/1/"
            };

            _db.Starships.Add(ship);
            _db.SaveChanges();
            return ship;
        }

        [Fact]
        public async Task GetAll_ReturnsSeededStarships()
        {
            SeedStarship("X-Wing");
            SeedStarship("Millennium Falcon");

            var result = await _controller.GetAll(
                name: null,
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

            var totalProp = type.GetProperty("total");
            var itemsProp = type.GetProperty("items");

            Assert.NotNull(totalProp);
            Assert.NotNull(itemsProp);

            var total = (int)(totalProp!.GetValue(payload) ?? 0);
            var items = (System.Collections.IEnumerable?)itemsProp!.GetValue(payload);

            Assert.Equal(2, total);
            Assert.NotNull(items);
        }

        [Fact]
        public async Task Create_PersistsStarship_ToDatabase()
        {
            var dto = new StarshipCreateDto
            {
                Name = "A-Wing",
                Model = "RZ-1",
                Manufacturer = "Alliance Underground Engineering",
                CostInCredits = "175000",
                Length = "9.6",
                MaxAtmospheringSpeed = "1300",
                Crew = "1",
                Passengers = "0",
                CargoCapacity = "40",
                Consumables = "1 week",
                HyperdriveRating = "1.0",
                MGLT = "120",
                StarshipClass = "Interceptor",
                Pilots = Array.Empty<string>(),
                Films = Array.Empty<string>()
            };

            var result = await _controller.Create(dto) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(nameof(StarshipsController.Get), result!.ActionName);

            var readDto = Assert.IsType<StarshipReadDto>(result.Value);
            Assert.Equal("A-Wing", readDto.Name);

            var inDb = await _db.Starships.SingleAsync(s => s.Name == "A-Wing");
            Assert.Equal("RZ-1", inDb.Model);
        }

        [Fact]
        public async Task Update_ChangesExistingStarship()
        {
            var ship = SeedStarship("Old Name");

            var dto = new StarshipUpdateDto
            {
                Name = "New Name",
                Model = "Updated Model",
                Manufacturer = "Updated Manufacturer",
                CostInCredits = "999999",
                Length = "15.0",
                MaxAtmospheringSpeed = "1500",
                Crew = "2",
                Passengers = "4",
                CargoCapacity = "200",
                Consumables = "2 months",
                HyperdriveRating = "0.5",
                MGLT = "150",
                StarshipClass = "Heavy Fighter",
                Pilots = Array.Empty<string>(),
                Films = Array.Empty<string>()
            };

            var response = await _controller.Update(ship.Id, dto);

            Assert.IsType<NoContentResult>(response);

            var updated = await _db.Starships.FindAsync(ship.Id);
            Assert.NotNull(updated);
            Assert.Equal("New Name", updated!.Name);
            Assert.Equal("Updated Model", updated.Model);
        }

        [Fact]
        public async Task Delete_RemovesStarship()
        {
            var ship = SeedStarship("To Delete");

            var response = await _controller.Delete(ship.Id);

            Assert.IsType<NoContentResult>(response);
            Assert.False(await _db.Starships.AnyAsync());
        }

        [Fact]
        public async Task Get_ReturnsNotFound_ForMissingId()
        {
            var response = await _controller.Get(999);

            Assert.IsType<NotFoundResult>(response);
        }

        // Minimal fake ISwapiFilmService so controller can be constructed.
        // We don't care about SWAPI behavior in these controller + DB tests.
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
