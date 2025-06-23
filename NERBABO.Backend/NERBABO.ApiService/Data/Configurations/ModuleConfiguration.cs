using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Modules.Models;

namespace NERBABO.ApiService.Data.Configurations
{
    public class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.ToTable("Modules");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Name)
                .HasColumnType("varchar(255)")
                .IsRequired(true);

            builder.Property(m => m.Hours)
                .HasColumnType("float")
                .HasDefaultValue(0.0f);

            builder.Property(m => m.IsActive)
                .HasDefaultValue(true);

            builder.HasMany(m => m.Courses)
                .WithMany(c => c.Modules);
        }
    }
}
