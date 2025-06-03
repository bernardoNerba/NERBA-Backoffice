using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Teachers.Models;

namespace NERBABO.ApiService.Data.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");
        builder.HasKey(t => t.Id);

        builder.HasOne(t => t.Person)
            .WithOne(p => p.Teacher)
            .HasForeignKey<Teacher>(t => t.PersonId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(t => t.IvaRegime)
            .WithMany(i => i.IvaTeachers)
            .HasForeignKey(t => t.IvaRegimeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(t => t.IrsRegime)
            .WithMany(i => i.IrsTeachers)
            .HasForeignKey(t => t.IrsRegimeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.Property(t => t.Ccp)
            .HasColumnType("varchar(55)")
            .IsRequired();

        builder.Property(t => t.AvarageRating)
            .HasColumnType("float")
            .HasDefaultValue(0.0f);

        builder.Property(t => t.Competences)
            .HasColumnType("text")
            .HasDefaultValue("N/A");

        builder.Property(t => t.IsActive)
            .HasColumnType("boolean")
            .HasDefaultValue(true);




    }
}
