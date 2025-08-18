using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Frames.Models;

namespace NERBABO.ApiService.Data.Configurations;

public class FrameConfiguration : IEntityTypeConfiguration<Frame>
{
    public void Configure(EntityTypeBuilder<Frame> builder)
    {
        builder.ToTable("Frames");

        builder.HasKey(f => f.Id);

        builder.HasIndex(f => new { f.Operation, f.Program })
            .IsUnique(true);

        builder.Property(f => f.Intervention)
            .HasColumnType("varchar(55)")
            .IsRequired(true);

        builder.Property(f => f.Program)
            .HasColumnType("varchar(150)")
            .IsRequired(true);

        builder.Property(f => f.InterventionType)
            .HasColumnType("varchar(150)")
            .IsRequired(true);

        builder.Property(f => f.Operation)
            .HasColumnType("varchar(150)")
            .IsRequired(true);

        builder.Property(f => f.OperationType)
            .HasColumnType("varchar(150)")
            .IsRequired(true);

        builder.Property(f => f.ProgramLogo)
            .HasColumnType("varchar(500)")
            .IsRequired(false);

        builder.Property(f => f.FinancementLogo)
            .HasColumnType("varchar(500)")
            .IsRequired(true);

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();
    }
}
