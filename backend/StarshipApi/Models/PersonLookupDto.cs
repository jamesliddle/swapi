namespace StarshipApi.Models
{
    public record PersonLookupDto
    {
        public string Url { get; init; } = "";
        public string Name { get; init; } = "";
    }
}
