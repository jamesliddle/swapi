using System.Collections.Generic;
using System.Threading.Tasks;
using StarshipApi.Models;

namespace StarshipApi.Services
{
    public interface ISwapiFilmService
    {
        Task<string[]> GetFilmTitlesAsync(string filmsJson);
        Task<IEnumerable<FilmLookupDto>> GetFilmLookupAsync();
        Task<IEnumerable<PersonLookupDto>> GetPeopleLookupAsync();
    }
}
