using ef_core_query_internal_server_error.Entities;
using Microsoft.EntityFrameworkCore;

namespace ef_core_query_internal_server_error.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Concurrency>(b =>
        {
            b.HasIndex(x => x.Name).IsUnique();
        });
    }

    public DbSet<Concurrency> Concurrencies { get; set; }
    public DbSet<Yolo> Yolos { get; set; }
}