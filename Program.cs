using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using RedisCasheWebApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddDbContext<KeyAndValueContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDatabase"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var keyAndValueContext = scope.ServiceProvider.GetRequiredService<KeyAndValueContext>();
    await keyAndValueContext.Database.EnsureDeletedAsync();
    await keyAndValueContext.Database.EnsureCreatedAsync();
    await keyAndValueContext.SeedAsync();
}

var casheExpiration = new DistributedCacheEntryOptions
{
    SlidingExpiration = TimeSpan.FromSeconds(5),
};

app.MapGet("/{key}", async (
    string key,
    IDistributedCache distributed,
    KeyAndValueContext keyAndValueContext
) =>
{
    string? value = distributed.GetString(key);
    if (value is not null)
    {
        return $"Key: {key}, Value: {value}. Source: Redis";
    }

    var keyAndValue = await keyAndValueContext.FindAsync<KeyAndValue>(key);

    if (keyAndValue is null)
    {
        return $"{key} is not found!";
    }

    await distributed.SetStringAsync(key, keyAndValue.Value, casheExpiration);

    return $"Key: {key}, Value: {keyAndValue.Value}. Source: MSSQL";
});

app.Run();
