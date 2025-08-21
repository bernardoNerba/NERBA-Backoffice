using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Global.Models;

namespace NERBABO.ApiService.Data.Configurations;

public class GeneralInfoConfiguration : IEntityTypeConfiguration<GeneralInfo>
{
    public void Configure(EntityTypeBuilder<GeneralInfo> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Designation)
            .HasColumnType("varchar(255)")
            .IsRequired(true);

        builder.Property(x => x.Site)
            .HasColumnType("text")
            .IsRequired(true);

        builder.Property(x => x.HourValueTeacher)
            .HasColumnType("decimal")
            .IsRequired(true);

        builder.Property(x => x.HourValueAlimentation)
            .HasColumnType("decimal")
            .IsRequired(true);

        builder.Property(x => x.IvaId)
            .HasColumnType("integer")
            .IsRequired(false);

        builder.Property(x => x.BankEntity)
            .HasColumnType("varchar(50)")
            .IsRequired(true);

        builder.Property(x => x.Iban)
            .HasColumnType("char(25)")
            .IsRequired(true);

        builder.Property(x => x.Nipc)
            .HasColumnType("char(9)")
            .IsRequired(true);

        builder.Property(x => x.Logo)
            .HasColumnType("varchar(500)")
            .IsRequired(false);

        builder.HasOne(x => x.IvaTax)
            .WithOne(t => t.GeneralInfo)
            .HasForeignKey<GeneralInfo>(x => x.IvaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
