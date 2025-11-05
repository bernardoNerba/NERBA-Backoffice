using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Modules.Models;

namespace NERBABO.ApiService.Data.Configurations;

public class ModuleCategoryConfiguration : IEntityTypeConfiguration<ModuleCategory>
{
    public void Configure(EntityTypeBuilder<ModuleCategory> builder)
    {
        builder.ToTable("ModuleCategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(155)
            .IsRequired();

        builder.Property(c => c.ShortenName)
            .HasMaxLength(15)
            .IsRequired();
    }
}