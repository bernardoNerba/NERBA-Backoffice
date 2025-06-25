using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Actions.Models;

namespace NERBABO.ApiService.Data.Configurations
{
    public class TeacherModuleActionConfiguration : IEntityTypeConfiguration<TeacherModuleAction>
    {
        public void Configure(EntityTypeBuilder<TeacherModuleAction> builder)
        {
            builder.ToTable("TeacherModuleActions");
            builder.HasKey(tma => tma.Id);

            builder.HasOne(tma => tma.Teacher)
                .WithMany(t => t.TeacherModuleActions)
                .HasForeignKey(tma => tma.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tma => tma.Action)
                .WithMany(a => a.TeacherModuleActions)
                .HasForeignKey(tma => tma.ActionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tma => tma.Module)
                .WithMany(m => m.TeacherModuleActions)
                .HasForeignKey(tma => tma.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
