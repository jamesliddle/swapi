namespace StarshipApi.Models
{
    public class StarshipListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Model { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string CostInCredits { get; set; } = "";
    }
}
