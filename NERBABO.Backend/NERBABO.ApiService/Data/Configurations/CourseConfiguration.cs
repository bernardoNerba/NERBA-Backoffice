using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Courses.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NERBABO.ApiService.Data.Configurations
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.ToTable("Courses");
            
            builder.HasKey(c => c.Id);

            builder.HasOne(p => p.Frame)
                .WithMany(f => f.Courses)
                .HasForeignKey(c => c.FrameId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(true);

            builder.Property(c => c.Title)
                .HasColumnType("varchar(255)")
                .IsRequired(true);

            builder.Property(c => c.Objectives)
                .HasColumnType("varchar(510)")
                .IsRequired(false);

            builder.Property(c => c.Destinators)
                .HasColumnType("integer[]")
                .IsRequired(false);

            builder.Property(c => c.Area)
                .HasColumnType("varchar(55)")
                .IsRequired(false);

            builder.HasMany(c => c.Modules)
                .WithMany(m => m.Courses);
        }
    }
}
