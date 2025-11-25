using Microsoft.EntityFrameworkCore;
using StarshipApi.Data;
using StarshipApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISwapiFilmService, SwapiFilmService>();
builder.Services.AddHttpClient();      // for SWAPI API calls
builder.Services.AddMemoryCache();     // for title caching

builder.Services.AddScoped<StarshipSeedService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .WithOrigins("http://localhost:4200"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var svcs = scope.ServiceProvider;
    var ctx = svcs.GetRequiredService<AppDbContext>();

    ctx.Database.Migrate(); // ensure database exists / migrations applied

    var seeder = svcs.GetRequiredService<StarshipSeedService>();
    // Safe to block: seeding only during startup
    seeder.SeedFromSwapiAsync().GetAwaiter().GetResult();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.MapControllers();

app.Run();
