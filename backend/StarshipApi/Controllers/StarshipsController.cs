using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarshipApi.Data;
using StarshipApi.Models;
using StarshipApi.Services;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarshipApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StarshipsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ISwapiFilmService _filmService;

    public StarshipsController(AppDbContext db, ISwapiFilmService filmService)
    {
        _db = db;
        _filmService = filmService;
    }

    private static string[] SafeArray(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<string>();

        try
        {
            return JsonSerializer.Deserialize<string[]>(json!) ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private StarshipReadDto ToDto(Starship s)
    {
        return new StarshipReadDto
        {
            Id = s.Id,
            Name = s.Name,
            Model = s.Model,
            Manufacturer = s.Manufacturer,
            CostInCredits = s.CostInCredits,
            Length = s.Length,
            MaxAtmospheringSpeed = s.MaxAtmospheringSpeed,
            Crew = s.Crew,
            Passengers = s.Passengers,
            CargoCapacity = s.CargoCapacity,
            Consumables = s.Consumables,
            HyperdriveRating = s.HyperdriveRating,
            MGLT = s.MGLT,
            StarshipClass = s.StarshipClass,

            Pilots = SafeArray(s.Pilots),
            Films = SafeArray(s.Films),

            Url = s.Url,
            Created = s.Created?.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
            Edited = s.Edited?.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")
        };
    }

    [HttpGet("/api/films")]
    public async Task<IActionResult> GetAllFilms() =>
        Ok(await _filmService.GetFilmLookupAsync());

    [HttpGet("/api/people")]
    public async Task<IActionResult> GetAllPeople() =>
        Ok(await _filmService.GetPeopleLookupAsync());

    // GET: api/starships
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? name,
        [FromQuery] string? model,
        [FromQuery] string? manufacturer,
        [FromQuery] string? cost,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var q = _db.Starships.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            q = q.Where(s => s.Name.Contains(name));
        if (!string.IsNullOrWhiteSpace(model))
            q = q.Where(s => s.Model.Contains(model));
        if (!string.IsNullOrWhiteSpace(manufacturer))
            q = q.Where(s => s.Manufacturer.Contains(manufacturer));
        if (!string.IsNullOrWhiteSpace(cost))
            q = q.Where(s => s.CostInCredits.Contains(cost));

        bool desc = sortDir?.ToLower() == "desc";
        sortBy = sortBy?.ToLower();

        q = sortBy switch
        {
            "name" => desc ? q.OrderByDescending(s => s.Name) : q.OrderBy(s => s.Name),
            "model" => desc ? q.OrderByDescending(s => s.Model) : q.OrderBy(s => s.Model),
            "manufacturer" => desc ? q.OrderByDescending(s => s.Manufacturer) : q.OrderBy(s => s.Manufacturer),
            "cost" => desc ? q.OrderByDescending(s => s.CostInCredits) : q.OrderBy(s => s.CostInCredits),
            _ => desc ? q.OrderByDescending(s => s.Id) : q.OrderBy(s => s.Id)
        };

        var total = await q.CountAsync();

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StarshipListItemDto
            {
                Id = s.Id,
                Name = s.Name,
                Model = s.Model,
                Manufacturer = s.Manufacturer,
                CostInCredits = s.CostInCredits
            })
            .ToListAsync();

        return Ok(new
        {
            total,
            page,
            pageSize,
            items
        });
    }

    // GET: api/starships/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var s = await _db.Starships.FindAsync(id);
        if (s == null)
            return NotFound();

        return Ok(ToDto(s));
    }

    // POST: api/starships
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StarshipCreateDto dto)
    {
        var entity = new Starship
        {
            Name = dto.Name,
            Model = dto.Model,
            Manufacturer = dto.Manufacturer,
            CostInCredits = dto.CostInCredits,
            Length = dto.Length,
            MaxAtmospheringSpeed = dto.MaxAtmospheringSpeed,
            Crew = dto.Crew,
            Passengers = dto.Passengers,
            CargoCapacity = dto.CargoCapacity,
            Consumables = dto.Consumables,
            HyperdriveRating = dto.HyperdriveRating,
            MGLT = dto.MGLT,
            StarshipClass = dto.StarshipClass,
            Pilots = JsonSerializer.Serialize(dto.Pilots ?? Array.Empty<string>()),
            Films = JsonSerializer.Serialize(dto.Films ?? Array.Empty<string>()),
            Created = DateTime.UtcNow,
            Edited = DateTime.UtcNow
        };

        _db.Starships.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = entity.Id }, ToDto(entity));
    }

    // PUT: api/starships/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] StarshipUpdateDto dto)
    {
        var s = await _db.Starships.FindAsync(id);
        if (s == null)
            return NotFound();

        s.Name = dto.Name;
        s.Model = dto.Model;
        s.Manufacturer = dto.Manufacturer;
        s.CostInCredits = dto.CostInCredits;
        s.Length = dto.Length;
        s.MaxAtmospheringSpeed = dto.MaxAtmospheringSpeed;
        s.Crew = dto.Crew;
        s.Passengers = dto.Passengers;
        s.CargoCapacity = dto.CargoCapacity;
        s.Consumables = dto.Consumables;
        s.HyperdriveRating = dto.HyperdriveRating;
        s.MGLT = dto.MGLT;
        s.StarshipClass = dto.StarshipClass;
        s.Pilots = JsonSerializer.Serialize(dto.Pilots ?? Array.Empty<string>());
        s.Films = JsonSerializer.Serialize(dto.Films ?? Array.Empty<string>());
        s.Edited = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/starships/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.Starships.FindAsync(id);
        if (s == null)
            return NotFound();

        _db.Starships.Remove(s);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
