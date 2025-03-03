using Microsoft.EntityFrameworkCore;
using MovieReservationsSystem.Models.Entities;
using MovieReservationsSystem.Models.DTO;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MovieReservationsSystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Users> Users { get; set; }
    public DbSet<Movies> Movies { get; set; }
    public DbSet<MovieReservations> MovieReservations { get; set; }
    public DbSet<TimeSlots> TimeSlots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure DateOnly to DateTime conversion
        // modelBuilder.Entity<Movies>()
        //     .Property(m => m.ReleaseDate)
        //     .HasConversion(
        //         v => v.ToDateTime(TimeOnly.MinValue), 
        //         v => DateOnly.FromDateTime(v));
        
        modelBuilder.Entity<Movies>()
            .Property(m => m.ReleaseDate)
            .HasConversion(
                v => TimeZoneInfo.ConvertTimeToUtc(v.ToDateTime(TimeOnly.MinValue), TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time")),
                v => DateOnly.FromDateTime(v));

// Use JSON serialization for array properties
        // modelBuilder.Entity<Movies>()
        //     .Property(m => m.Genres)
        //     .HasConversion(
        //         v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
        //         v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions)null));
        //
        // modelBuilder.Entity<Movies>()
        //     .Property(m => m.Actors)
        //     .HasConversion(
        //         v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
        //         v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions)null));
        //
        // modelBuilder.Entity<MovieReservations>()
        //     .Property(m => m.SeatNumbers)
        //     .HasConversion(
        //         v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
        //         v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions)null));
    }
}