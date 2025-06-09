using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Students.Models;

namespace NERBABO.ApiService.Data.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.ToTable("Students");

            builder.HasKey(t => t.Id);

            builder.HasOne(s => s.Person)
                .WithOne(p => p.Student)
                .HasForeignKey<Student>(s => s.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Company)
                .WithMany(p => p.Students)
                .HasForeignKey(s => s.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
