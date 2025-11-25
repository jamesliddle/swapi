using System;

namespace StarshipApi.Models
{
    public class StarshipReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Model { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string CostInCredits { get; set; } = "";
        public string Length { get; set; } = "";
        public string MaxAtmospheringSpeed { get; set; } = "";
        public string Crew { get; set; } = "";
        public string Passengers { get; set; } = "";
        public string CargoCapacity { get; set; } = "";
        public string Consumables { get; set; } = "";
        public string HyperdriveRating { get; set; } = "";
        public string MGLT { get; set; } = "";
        public string StarshipClass { get; set; } = "";
        public string[] Pilots { get; set; } = Array.Empty<string>();
        public string[] Films { get; set; } = Array.Empty<string>();
        public string Url { get; set; } = "";
        public string? Created { get; set; }
        public string? Edited { get; set; }
    }
}