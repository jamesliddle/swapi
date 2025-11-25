using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StarshipApi.Data;
using StarshipApi.Models;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarshipApi.Services
{
    public class StarshipSeedService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;
        private readonly HttpClient _http;

        public StarshipSeedService(
            IConfiguration config,
            AppDbContext db)
        {
            _config = config;
            _db = db;
            _http = new HttpClient();
        }

        // Seed once from SWAPI (using a primary + fallback endpoint)
        public async Task SeedFromSwapiAsync()
        {
            if (await _db.Starships.AnyAsync())
                return; // Already seeded

            var primary = _config["Swapi:Primary"];
            var fallback = _config["Swapi:Fallback"];

            bool seeded = false;

            // Try primary
            if (!string.IsNullOrWhiteSpace(primary))
            {
                try
                {
                    seeded = await TrySeedFromSwapiEndpointAsync(primary);
                }
                catch
                {
                    seeded = false;
                }
            }

            // Try fallback if primary failed
            if (!seeded && !string.IsNullOrWhiteSpace(fallback))
            {
                try
                {
                    seeded = await TrySeedFromSwapiEndpointAsync(fallback);
                }
                catch
                {
                    seeded = false;
                }
            }
        }

        // Handles paginated or array SWAPI starships endpoint.
        private async Task<bool> TrySeedFromSwapiEndpointAsync(string baseUrl)
        {
            var next = baseUrl;
            var any = false;

            while (!string.IsNullOrWhiteSpace(next))
            {
                HttpResponseMessage resp;
                try
                {
                    resp = await _http.GetAsync(next);
                }
                catch
                {
                    return any; // stop on network error
                }

                if (!resp.IsSuccessStatusCode)
                    return any;

                var json = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                    doc.RootElement.TryGetProperty("results", out var results))
                {
                    // PAGED format: { "results": [...], "next": "..." }
                    ProcessStarshipResults(results);
                    await _db.SaveChangesAsync();

                    next = doc.RootElement.TryGetProperty("next", out var nextEl) &&
                           nextEl.ValueKind == JsonValueKind.String
                        ? nextEl.GetString()
                        : null;

                    any = true;
                    continue;
                }

                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    // ARRAY format: [ {...}, {...} ]
                    ProcessStarshipResults(doc.RootElement);
                    await _db.SaveChangesAsync();
                    any = true;
                    break;
                }

                // SINGLE OBJECT format
                var s = MapJsonToStarship(doc.RootElement);
                _db.Starships.Add(s);
                await _db.SaveChangesAsync();

                any = true;
                break;
            }

            return any;
        }

        private void ProcessStarshipResults(JsonElement results)
        {
            foreach (var el in results.EnumerateArray())
            {
                var starship = MapJsonToStarship(el);
                _db.Starships.Add(starship);
            }
        }

        /// Converts SWAPI JSON -> Starship entity.
        /// Pilots and Films remain JSON arrays from SWAPI.
        private Starship MapJsonToStarship(JsonElement el)
        {
            string? GetString(string name) =>
                el.TryGetProperty(name, out var p) && p.ValueKind != JsonValueKind.Null
                    ? p.ToString()
                    : null;

            var s = new Starship
            {
                Name = GetString("name") ?? "",
                Model = GetString("model") ?? "",
                Manufacturer = GetString("manufacturer") ?? "",
                CostInCredits = GetString("cost_in_credits") ?? "",
                Length = GetString("length") ?? "",
                MaxAtmospheringSpeed = GetString("max_atmosphering_speed") ?? "",
                Crew = GetString("crew") ?? "",
                Passengers = GetString("passengers") ?? "",
                CargoCapacity = GetString("cargo_capacity") ?? "",
                Consumables = GetString("consumables") ?? "",
                HyperdriveRating = GetString("hyperdrive_rating") ?? "",
                MGLT = GetString("MGLT") ?? "",
                StarshipClass = GetString("starship_class") ?? "",

                Pilots = el.TryGetProperty("pilots", out var pilots)
                    ? pilots.ToString()
                    : "[]",

                Films = el.TryGetProperty("films", out var films)
                    ? films.ToString()
                    : "[]",

                Url = GetString("url") ?? ""
            };

            if (DateTime.TryParse(GetString("created") ?? "", out var created))
                s.Created = created;

            if (DateTime.TryParse(GetString("edited") ?? "", out var edited))
                s.Edited = edited;

            return s;
        }
    }
}
