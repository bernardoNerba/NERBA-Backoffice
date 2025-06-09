using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Companies.Models;
using NERBABO.ApiService.Core.Students.Models;

namespace NERBABO.ApiService.Data.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Companies");

            builder.HasKey(c => c.Id);

            builder.HasMany(c => c.Students)
                .WithOne(s => s.Company)
                .HasForeignKey(s => s.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(c => c.Name)
                .HasColumnType("varchar(155)")
                .IsRequired();

            builder.Property(c => c.Address)
                .HasColumnType("text")
                .IsRequired(false);

            builder.Property(c => c.Locality)
                .HasColumnType("varchar(55)")
                .IsRequired(false);

            builder.Property(c => c.ZipCode)
                .HasColumnType("varchar(9)")
                .IsRequired(false);

            builder.Property(c => c.Email)
                .HasColumnType("varchar(155)")
                .IsRequired(false);

        }
    }
}
