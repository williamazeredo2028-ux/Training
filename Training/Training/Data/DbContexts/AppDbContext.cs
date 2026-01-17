using Microsoft.EntityFrameworkCore;
using TrainingApi.Domain.Entities;

namespace TrainingApi.Data.DbContexts;

public class AppDbContext : DbContext
{
    public DbSet<Device> Devices { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>().Property(d => d.State).HasConversion<string>();
    }
}