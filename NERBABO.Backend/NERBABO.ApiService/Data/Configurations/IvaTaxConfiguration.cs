using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Global.Models;

namespace NERBABO.ApiService.Data.Configurations;

public class IvaTaxConfiguration : IEntityTypeConfiguration<IvaTax>
{
    public void Configure(EntityTypeBuilder<IvaTax> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasColumnType("varchar(50)")
            .IsRequired(true);

        builder.Property(x => x.ValuePercent)
            .HasColumnType("integer")
            .IsRequired(true);

        builder.Property(x => x.IsActive)
            .HasColumnName("boolean")
            .HasDefaultValue(true);
    }
}
