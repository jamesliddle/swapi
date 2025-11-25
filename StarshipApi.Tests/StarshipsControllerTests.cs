using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StarshipApi.Controllers;
using StarshipApi.Data;
using StarshipApi.Models;
using StarshipApi.Services;
using System.Text.Json;

public class StarshipsControllerTests
{
    private class ListResponse
    {
        public int total { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public List<StarshipListItemDto> items { get; set; } = new();
    }

    private AppDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(opts);
    }

    private Starship MakeShip(int id, string name, string model, string manufacturer, string cost = "1000")
    {
        return new Starship
        {
            Id = id,
            Name = name,
            Model = model,
            Manufacturer = manufacturer,
            CostInCredits = cost,
            Films = "[]",
            Pilots = "[]"
        };
    }

    private Mock<ISwapiFilmService> MakeFilmMock()
    {
        var mock = new Mock<ISwapiFilmService>();

        mock.Setup(m => m.GetFilmLookupAsync())
            .ReturnsAsync(new List<FilmLookupDto>());

        mock.Setup(m => m.GetPeopleLookupAsync())
            .ReturnsAsync(new List<PersonLookupDto>());

        mock.Setup(m => m.GetFilmTitlesAsync(It.IsAny<string>()))
            .ReturnsAsync(Array.Empty<string>());

        return mock;
    }

    [Fact]
    public async Task Get_ReturnsStarship()
    {
        var db = CreateDb();
        db.Starships.Add(MakeShip(1, "X-Wing", "T-65", "Incom"));
        await db.SaveChangesAsync();

        var controller = new StarshipsController(db, MakeFilmMock().Object);

        var result = await controller.Get(1);
        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<StarshipReadDto>(ok.Value);

        Assert.Equal("X-Wing", dto.Name);
    }

    [Fact]
    public async Task Get_ReturnsNotFound()
    {
        var db = CreateDb();
        var controller = new StarshipsController(db, MakeFilmMock().Object);

        var result = await controller.Get(999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetAll_FiltersByName()
    {
        var db = CreateDb();
        db.Starships.Add(MakeShip(1, "Falcon", "M1", "CEC"));
        db.Starships.Add(MakeShip(2, "X-Wing", "M2", "Incom"));
        await db.SaveChangesAsync();

        var controller = new StarshipsController(db, MakeFilmMock().Object);

        var result = await controller.GetAll("Falcon", null, null, null, null, null, 1, 50);
        var ok = Assert.IsType<OkObjectResult>(result);

        var json = JsonSerializer.Serialize(ok.Value);
        var data = JsonSerializer.Deserialize<ListResponse>(json)!;

        Assert.Equal(1, data.total);
        Assert.Single(data.items);
        Assert.Equal("Falcon", data.items[0].Name);
    }

    [Fact]
    public async Task GetAll_FiltersByModel()
    {
        var db = CreateDb();
        db.Starships.Add(MakeShip(1, "ShipA", "A1", "Corp"));
        db.Starships.Add(MakeShip(2, "ShipB", "B1", "Corp"));
        await db.SaveChangesAsync();

        var controller = new StarshipsController(db, MakeFilmMock().Object);

        var result = await controller.GetAll(null, "A1", null, null, null, null, 1, 50);
        var ok = Assert.IsType<OkObjectResult>(result);

        var json = JsonSerializer.Serialize(ok.Value);
        var data = JsonSerializer.Deserialize<ListResponse>(json)!;

        Assert.Equal(1, data.total);
        Assert.Single(data.items);
        Assert.Equal("ShipA", data.items[0].Name);
    }

    [Fact]
    public async Task GetAll_SortsByName_Asc()
    {
        var db = CreateDb();
        db.Starships.Add(MakeShip(1, "Zeta", "M", "Corp"));
        db.Starships.Add(MakeShip(2, "Alpha", "M", "Corp"));
        await db.SaveChangesAsync();

        var controller = new StarshipsController(db, MakeFilmMock().Object);

        var result = await controller.GetAll(null, null, null, null, "name", "asc", 1, 50);
        var ok = Assert.IsType<OkObjectResult>(result);

        var json = JsonSerializer.Serialize(ok.Value);
        var data = JsonSerializer.Deserialize<ListResponse>(json)!;

        Assert.Equal("Alpha", data.items[0].Name);
        Assert.Equal("Zeta", data.items[1].Name);
    }

    [Fact]
    public async Task GetAll_SortsByName_Desc()
    {
        var db = CreateDb();
        db.Starships.Add(MakeShip(1, "Zeta", "M", "Corp"));
        db.Starships.Add(MakeShip(2, "Alpha", "M", "Corp"));
        await db.SaveChangesAsync();

        var controller = new StarshipsController(db, MakeFilmMock().Object);

        var result = await controller.GetAll(null, null, null, null, "name", "desc", 1, 50);
        var ok = Assert.IsType<OkObjectResult>(result);

        var json = JsonSerializer.Serialize(ok.Value);
        var data = JsonSerializer.Deserialize<ListResponse>(json)!;

        Assert.Equal("Zeta", data.items[0].Name);
        Assert.Equal("Alpha", data.items[1].Name);
    }

    [Fact]
    public async Task GetAll_ReturnsPagedData()
    {
        var db = CreateDb();

        for (int i = 1; i <= 25; i++)
            db.Starships.Add(MakeShip(i, $"Ship{i}", "M", "Corp"));

        await db.SaveChangesAsync();

        var controller = new StarshipsController(db, MakeFilmMock().Object);

        var result = await controller.GetAll(null, null, null, null, "name", "asc", 2, 10);
        var ok = Assert.IsType<OkObjectResult>(result);

        var json = JsonSerializer.Serialize(ok.Value);
        var data = JsonSerializer.Deserialize<ListResponse>(json)!;

        Assert.Equal(25, data.total);
        Assert.Equal(10, data.items.Count);
    }

    [Fact]
    public async Task Create_AddsStarship()
    {
        var db = CreateDb();
        var controller = new StarshipsController(db, MakeFilmMock().Object);

        var dto = new StarshipCreateDto
        {
            Name = "NewShip",
            Model = "M",
            Manufacturer = "Corp"
        };

        var result = await controller.Create(dto);
        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(1, db.Starships.Count());
    }

    [Fact]
    public async Task Update_ChangesStarship()
    {
        var db = CreateDb();
        db.Add(MakeShip(1, "Old", "M", "Corp"));
        await db.SaveChangesAsync();

        var controller = new StarshipsController(db, MakeFilmMock().Object);

        var dto = new StarshipUpdateDto
        {
            Name = "Updated",
            Model = "M2",
            Manufacturer = "NewCorp"
        };

        var result = await controller.Update(1, dto);
        Assert.IsType<NoContentResult>(result);

        var updated = db.Starships.Find(1);
        Assert.Equal("Updated", updated.Name);
    }

    [Fact]
    public async Task Delete_RemovesStarship()
    {
        var db = CreateDb();
        db.Starships.Add(MakeShip(1, "DeleteMe", "M", "Corp"));
        await db.SaveChangesAsync();

        var controller = new StarshipsController(db, MakeFilmMock().Object);

        await controller.Delete(1);

        Assert.Equal(0, db.Starships.Count());
    }

    [Fact]
    public async Task Delete_ReturnsNotFound()
    {
        var db = CreateDb();
        var controller = new StarshipsController(db, MakeFilmMock().Object);

        var result = await controller.Delete(99);

        Assert.IsType<NotFoundResult>(result);
    }
}
