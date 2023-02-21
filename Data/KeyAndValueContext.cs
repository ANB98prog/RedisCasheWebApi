using Microsoft.EntityFrameworkCore;

namespace RedisCasheWebApi.Data;

public class KeyAndValueContext : DbContext
{
    public KeyAndValueContext(DbContextOptions<KeyAndValueContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KeyAndValue>()
            .HasKey(e => e.Key);
    }

    public DbSet<KeyAndValue>? KeyAndValues { get; set; }
}

public static class Seeder
{
    public static async Task SeedAsync(this KeyAndValueContext context)
    {
        if (!context.KeyAndValues.Any())
        {
            int i = 1;
            foreach (string letter in Alphabet)
            {
                context.Add(new KeyAndValue
                {
                    Key = letter,
                    Value = $"The letter \"{letter}\" is at position {i++} in the alphabet"
                });
            }

            await context.SaveChangesAsync();
        }
    }

    private static IEnumerable<string> Alphabet
    {
        get
        {
            for (char letter = 'a'; letter <= 'z'; letter++)
            {
                yield return letter.ToString();
            }
        }
    }
}