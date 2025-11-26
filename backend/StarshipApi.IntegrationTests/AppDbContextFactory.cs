using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StarshipApi.Data;

namespace StarshipApi.IntegrationTests;

public static class AppDbContextFactory
{
    public static AppDbContext CreateDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        var context = new AppDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }
}
