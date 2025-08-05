using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.ModuleTeachings.Models;

namespace NERBABO.ApiService.Data.Configurations
{
    public class TeacherModuleActionConfiguration : IEntityTypeConfiguration<ModuleTeaching>
    {
        public void Configure(EntityTypeBuilder<ModuleTeaching> builder)
        {
            builder.ToTable("ModuleTeachings");
            builder.HasKey(tma => tma.Id);
            builder.HasIndex(tma => new { tma.ActionId, tma.ModuleId, tma.TeacherId });

            builder.HasOne(tma => tma.Teacher)
                .WithMany(t => t.ModuleTeachings)
                .HasForeignKey(tma => tma.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tma => tma.Action)
                .WithMany(a => a.ModuleTeachings)
                .HasForeignKey(tma => tma.ActionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tma => tma.Module)
                .WithMany(m => m.ModuleTeachings)
                .HasForeignKey(tma => tma.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
