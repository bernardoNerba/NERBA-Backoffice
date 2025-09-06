using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.ModuleAvaliations.Models;

namespace NERBABO.ApiService.Data.Configurations;

public class ModuleAvaliationConfiguration : IEntityTypeConfiguration<ModuleAvaliation>
{
    public void Configure(EntityTypeBuilder<ModuleAvaliation> builder)
    {
        builder.ToTable("ModuleAvaliations");
        
        builder.HasKey(ma => ma.Id);
        
        builder.Property(ma => ma.ModuleTeachingId)
            .IsRequired();
        
        builder.Property(ma => ma.ActionEnrollmentId)
            .IsRequired();
        
        builder.Property(ma => ma.Grade)
            .IsRequired();
        
        builder.HasIndex(ma => new { ma.ModuleTeachingId, ma.ActionEnrollmentId })
            .IsUnique();
        
        builder.HasOne(ma => ma.ModuleTeaching)
            .WithMany(mt => mt.Avaliations)
            .HasForeignKey(ma => ma.ModuleTeachingId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(ma => ma.ActionEnrollment)
            .WithMany(ae => ae.Avaliations)
            .HasForeignKey(ma => ma.ActionEnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}