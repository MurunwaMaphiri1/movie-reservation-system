using Microsoft.EntityFrameworkCore;
using MoviesReservationSystem.Models.Entities;
using MoviesReservationSystem.Models.DTO;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MoviesReservationSystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Users> Users { get; set; }
    public DbSet<Movies> Movies { get; set; }
    public DbSet<MovieReservations> MovieReservations { get; set; }
    public DbSet<TimeSlots> TimeSlots { get; set; }
    public DbSet<Employees> Employees { get; set; }

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