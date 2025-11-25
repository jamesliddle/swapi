namespace StarshipApi.Models
{
    public record FilmLookupDto
    {
        public string Url { get; init; } = "";
        public string Title { get; init; } = "";
    }
}
