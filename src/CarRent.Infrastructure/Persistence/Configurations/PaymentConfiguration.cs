using CarRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarRent.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.TransactionId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(10,2)");
        
        builder.ToTable(t => t.HasCheckConstraint("ck_payments_amount_positive", "\"Amount\" > 0"));

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(p => p.BookingId)
            .IsUnique();
    }
}