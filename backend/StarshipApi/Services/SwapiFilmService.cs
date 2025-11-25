using Microsoft.Extensions.Caching.Memory;
using StarshipApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarshipApi.Services
{
    public class SwapiFilmService : ISwapiFilmService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;

        public SwapiFilmService(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        public async Task<string[]> GetFilmTitlesAsync(string filmsJson)
        {
            if (string.IsNullOrWhiteSpace(filmsJson))
                return Array.Empty<string>();

            List<string>? urls;
            try
            {
                urls = JsonSerializer.Deserialize<List<string>>(filmsJson);
            }
            catch
            {
                return Array.Empty<string>();
            }

            if (urls is null || urls.Count == 0)
                return Array.Empty<string>();

            var client = _httpClientFactory.CreateClient();

            var tasks = urls.Select(async url =>
            {
                if (_cache.TryGetValue(url, out string? cached) && cached is not null)
                    return cached;

                try
                {
                    var json = await client.GetStringAsync(url);
                    using var doc = JsonDocument.Parse(json);
                    var title = doc.RootElement.GetProperty("title").GetString() ?? "(unknown)";

                    _cache.Set(url, title, TimeSpan.FromHours(24));
                    return title;
                }
                catch
                {
                    return "(unavailable)";
                }
            });

            return await Task.WhenAll(tasks);
        }

        public async Task<IEnumerable<FilmLookupDto>> GetFilmLookupAsync()
        {
            const string cacheKey = "swapi-film-lookup";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<FilmLookupDto>? cached) && cached != null)
                return cached;

            var client = _httpClientFactory.CreateClient();
            var list = new List<FilmLookupDto>();
            string? next = "https://swapi.dev/api/films/";

            try
            {
                while (!string.IsNullOrEmpty(next))
                {
                    var json = await client.GetStringAsync(next);
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    foreach (var film in root.GetProperty("results").EnumerateArray())
                    {
                        var url = film.GetProperty("url").GetString() ?? "";
                        var title = film.GetProperty("title").GetString() ?? "(unknown)";

                        if (!string.IsNullOrEmpty(url))
                        {
                            list.Add(new FilmLookupDto
                            {
                                Url = url,
                                Title = title
                            });
                        }
                    }

                    next = root.TryGetProperty("next", out var nextProp)
                        ? nextProp.GetString()
                        : null;
                }

                _cache.Set(cacheKey, list, TimeSpan.FromHours(24));
            }
            catch
            {
                // If SWAPI fails, just return whatever we have so far
            }

            return list;
        }

        public async Task<IEnumerable<PersonLookupDto>> GetPeopleLookupAsync()
        {
            const string cacheKey = "swapi-people-lookup";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<PersonLookupDto>? cached) && cached != null)
                return cached;

            var client = _httpClientFactory.CreateClient();
            var list = new List<PersonLookupDto>();
            string? next = "https://swapi.dev/api/people/";

            try
            {
                while (!string.IsNullOrEmpty(next))
                {
                    var json = await client.GetStringAsync(next);
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    foreach (var p in root.GetProperty("results").EnumerateArray())
                    {
                        var url = p.GetProperty("url").GetString() ?? "";
                        var name = p.GetProperty("name").GetString() ?? "(unknown)";

                        if (!string.IsNullOrEmpty(url))
                        {
                            list.Add(new PersonLookupDto
                            {
                                Url = url,
                                Name = name
                            });
                        }
                    }

                    next = root.TryGetProperty("next", out var nextProp)
                        ? nextProp.GetString()
                        : null;
                }

                _cache.Set(cacheKey, list, TimeSpan.FromHours(24));
            }
            catch
            {
                // If SWAPI fails, return what we have (possibly empty)
            }

            return list;
        }
    }
}
