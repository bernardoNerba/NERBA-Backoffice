using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Enrollments.Models;

namespace NERBABO.ApiService.Data.Configurations;

public class ActionEnrollmentConfiguration : IEntityTypeConfiguration<ActionEnrollment>
{
    public void Configure(EntityTypeBuilder<ActionEnrollment> builder)
    {
        builder.ToTable("ActionEnrollments");
        builder.HasKey(ae => ae.Id);
        builder.HasIndex(ae => new { ae.ActionId, ae.StudentId });

        builder.HasOne(ae => ae.Action)
                .WithMany(a => a.ActionEnrollments)
                .HasForeignKey(ae => ae.ActionId)
                .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(ae => ae.Student)
                .WithMany(s => s.ActionEnrollments)
                .HasForeignKey(ae => ae.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
    }
}