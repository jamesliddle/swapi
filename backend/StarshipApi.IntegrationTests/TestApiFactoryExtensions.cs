using Microsoft.Extensions.DependencyInjection;
using StarshipApi.Data;

namespace StarshipApi.IntegrationTests
{
    public static class TestApiFactoryExtensions
    {
        public static async Task ResetDatabaseAsync(this TestApiFactory factory)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }

        public static async Task SeedAsync(this TestApiFactory factory, params StarshipApi.Models.Starship[] items)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Starships.AddRange(items);
            await db.SaveChangesAsync();
        }
    }
}
