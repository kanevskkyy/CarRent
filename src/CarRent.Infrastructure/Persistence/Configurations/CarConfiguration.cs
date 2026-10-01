using CarRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarRent.Infrastructure.Persistence.Configurations;

public class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.DailyPrice)
            .HasColumnType("decimal(10,2)");
        
        builder.ToTable(t => t.HasCheckConstraint("ck_cars_daily_price_positive", "\"DailyPrice\" > 0"));

        builder.Property(c => c.PhotoUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.BodyType)
            .HasConversion<string>() 
            .HasMaxLength(20);

        builder.Property(c => c.EngineType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.DriveType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.TransmissionType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(c => c.DailyPrice); 

        builder.HasMany(c => c.Bookings)
            .WithOne(b => b.Car)
            .HasForeignKey(b => b.CarId)
            .OnDelete(DeleteBehavior.Restrict); 
    }
}