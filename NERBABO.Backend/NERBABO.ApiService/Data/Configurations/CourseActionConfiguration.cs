using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Actions.Models;
using System.Reflection.Emit;

namespace NERBABO.ApiService.Data.Configurations
{
    public class CourseActionConfiguration : IEntityTypeConfiguration<CourseAction>
    {
        public void Configure(EntityTypeBuilder<CourseAction> builder)
        {
            builder.ToTable("Actions");
            builder.HasKey(a => a.Id);

            builder.HasIndex(u => new { u.AdministrationCode })
                .IsUnique();

            builder.HasOne(a => a.Course)
                .WithMany(c => c.Actions)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Coordenator)
                .WithMany(c => c.Actions)
                .HasForeignKey(a => a.CoordenatorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(a => a.TeacherModuleActions)
                .WithOne(tma => tma.Action)
                .HasForeignKey(tma => tma.ActionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(a => a.Title)
                .HasColumnType("varchar(255)")
                .IsRequired(true);

            builder.Property(a => a.AdministrationCode)
                .HasColumnType("varchar(10)")
                .IsRequired(true);

            builder.Property(a => a.Address)
                .HasColumnType("varchar(255)")
                .IsRequired(false);

            builder.Property(a => a.Locality)
                .HasColumnType("varchar(55)")
                .IsRequired(true);

            builder.Property(a => a.WeekDays)
                .HasColumnType("integer[]")
                .IsRequired(false);

        }
    }
}
