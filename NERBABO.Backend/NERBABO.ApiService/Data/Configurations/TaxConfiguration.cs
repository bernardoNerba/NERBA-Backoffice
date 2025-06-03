using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Global.Models;

namespace NERBABO.ApiService.Data.Configurations;

public class TaxConfiguration : IEntityTypeConfiguration<Tax>
{
    public void Configure(EntityTypeBuilder<Tax> builder)
    {
        builder.ToTable("Taxes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasColumnType("varchar(50)")
            .IsRequired(true);

        builder.Property(x => x.ValuePercent)
            .HasColumnType("integer")
            .IsRequired(true);

        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("boolean")
            .HasDefaultValue(true);
    }
}
