using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Reports.Models;

namespace NERBABO.ApiService.Data.Configurations;

public class SavedPdfConfiguration : IEntityTypeConfiguration<SavedPdf>
{
    public void Configure(EntityTypeBuilder<SavedPdf> builder)
    {
        builder.ToTable("SavedPdfs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PdfType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ReferenceId)
            .IsRequired();

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ContentHash)
            .IsRequired()
            .HasMaxLength(64); // SHA256 hash length

        builder.Property(x => x.GeneratedAt)
            .IsRequired();

        builder.Property(x => x.GeneratedByUserId)
            .IsRequired()
            .HasMaxLength(450); // Standard Identity User ID length

        // Create unique index on PdfType + ReferenceId to prevent duplicates
        builder.HasIndex(x => new { x.PdfType, x.ReferenceId })
            .IsUnique()
            .HasDatabaseName("IX_SavedPdfs_PdfType_ReferenceId");

        // Index for efficient queries by file path
        builder.HasIndex(x => x.FilePath)
            .HasDatabaseName("IX_SavedPdfs_FilePath");
    }
}