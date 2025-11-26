using Microsoft.EntityFrameworkCore;
using StarshipApi.Data;
using StarshipApi.Services;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

bool isIntegrationTest = env.IsEnvironment("IntegrationTests");

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(conn);
});

builder.Services.AddScoped<ISwapiFilmService, SwapiFilmService>();
builder.Services.AddScoped<StarshipSeedService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (!isIntegrationTest)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment() && !isIntegrationTest)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();

public partial class Program { }
