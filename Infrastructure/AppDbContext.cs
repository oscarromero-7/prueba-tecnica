using Microsoft.EntityFrameworkCore;
using PruebaTecnica.Domain;

namespace PruebaTecnica.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Currency> Currencies => Set<Currency>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(u => u.Id);
            user.Property(u => u.Name).IsRequired().HasMaxLength(100);
            user.Property(u => u.Email).IsRequired().HasMaxLength(200);
            user.HasIndex(u => u.Email).IsUnique();
            user.Property(u => u.PasswordHash).HasMaxLength(500);
        });

        modelBuilder.Entity<Address>(address =>
        {
            address.HasKey(a => a.Id);
            address.Property(a => a.Street).IsRequired().HasMaxLength(200);
            address.Property(a => a.City).IsRequired().HasMaxLength(100);
            address.Property(a => a.Country).IsRequired().HasMaxLength(100);
            address.Property(a => a.ZipCode).HasMaxLength(20);

            address.HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Currency>(currency =>
        {
            currency.HasKey(c => c.Id);
            currency.Property(c => c.Code).IsRequired().HasMaxLength(10);
            currency.HasIndex(c => c.Code).IsUnique();
            currency.Property(c => c.Name).IsRequired().HasMaxLength(100);

            // SQLite no tiene decimal: EF lo guarda como TEXT (sin perder precisión), pero SQLite
            // no puede ordenar/comparar ese TEXT como número. Por eso no se filtra ni ordena por
            // RateToBase en SQL; los cálculos se hacen en C#.
            currency.Property(c => c.RateToBase).IsRequired().HasPrecision(18, 6);

            currency.HasData(
                new Currency { Id = 1, Code = "PYG", Name = "Guaraní paraguayo", RateToBase = 1m },
                new Currency { Id = 2, Code = "USD", Name = "Dólar estadounidense", RateToBase = 7300m },
                new Currency { Id = 3, Code = "EUR", Name = "Euro", RateToBase = 7900m },
                new Currency { Id = 4, Code = "BRL", Name = "Real brasileño", RateToBase = 1300m });
        });
    }
}
