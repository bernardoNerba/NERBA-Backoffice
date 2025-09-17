using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.SessionParticipations.Models;

namespace NERBABO.ApiService.Data.Configurations;

public class SessionParticipationConfiguration : IEntityTypeConfiguration<SessionParticipation>
{
    public void Configure(EntityTypeBuilder<SessionParticipation> builder)
    {
        builder.ToTable("SessionParticipations");
        
        builder.HasKey(sp => sp.Id);
        
        builder.Property(sp => sp.SessionId)
            .IsRequired();
        
        builder.Property(sp => sp.ActionEnrollmentId)
            .IsRequired();
        
        builder.Property(sp => sp.Presence)
            .HasConversion<int>()
            .IsRequired();
        
        builder.Property(sp => sp.Attendance)
            .HasColumnType("float")
            .IsRequired();
        
        builder.HasIndex(sp => new { sp.SessionId, sp.ActionEnrollmentId })
            .IsUnique();
        
        builder.HasOne(sp => sp.Session)
            .WithMany(s => s.Participants)
            .HasForeignKey(sp => sp.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(sp => sp.ActionEnrollment)
            .WithMany(ae => ae.Participations)
            .HasForeignKey(sp => sp.ActionEnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}