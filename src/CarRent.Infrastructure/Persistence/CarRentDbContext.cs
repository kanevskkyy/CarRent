using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Persistence;

public class CarRentDbContext: IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public CarRentDbContext(DbContextOptions<CarRentDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Car> Cars { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CarConfiguration).Assembly);
    }
}