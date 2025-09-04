using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Enrollments.Models;

namespace NERBABO.ApiService.Data.Configurations;

public class MTEnrollmentConfiguration : IEntityTypeConfiguration<MTEnrollment>
{
    public void Configure(EntityTypeBuilder<MTEnrollment> builder)
    {
        builder.ToTable("MTEnrollments");
        builder.HasKey(tme => tme.Id);
        builder.HasIndex(tme => new { tme.ModuleTeachingId, tme.StudentId });

        builder.HasOne(tme => tme.ModuleTeaching)
                .WithMany(mt => mt.MTEnrollments)
                .HasForeignKey(tme => tme.ModuleTeachingId)
                .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(tme => tme.Student)
                .WithMany(s => s.MTEnrollments)
                .HasForeignKey(tme => tme.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
    }
}
