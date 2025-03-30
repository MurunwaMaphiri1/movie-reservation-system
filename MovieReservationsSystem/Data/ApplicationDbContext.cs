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
        
        modelBuilder.Entity<MovieReservations>()
            .Property(e => e.ReservationDate)
            .HasColumnType("date");
        
        modelBuilder.Entity<TimeSlots>()
            .Property(e => e.TimeSlot)
            .HasColumnType("time");
        
    }
}