using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.People.Models;

namespace NERBABO.ApiService.Data.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("People");

        builder.HasKey(p => p.Id);

        builder.HasOne(p => p.User)
            .WithOne(u => u.Person)
            .HasForeignKey<User>(p => p.PersonId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.FirstName)
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(p => p.LastName)
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(p => p.NIF)
            .HasColumnType("char(9)")
            .IsRequired();

        builder.Property(p => p.IdentificationNumber)
            .HasColumnType("varchar(10)")
            .IsRequired(false);

        builder.Property(p => p.IdentificationValidationDate)
            .HasColumnType("date")
            .IsRequired(false);

        builder.Property(p => p.NISS)
            .HasColumnType("varchar(11)")
            .IsRequired(false);

        builder.Property(p => p.IBAN)
            .HasColumnType("varchar(25)")
            .IsRequired(false);

        builder.Property(p => p.BirthDate)
            .HasColumnType("date")
            .IsRequired(false);

        builder.Property(p => p.Address)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(p => p.ZipCode)
            .HasColumnType("varchar(8)")
            .IsRequired(false);

        builder.Property(p => p.PhoneNumber)
            .HasColumnType("varchar(9)")
            .IsRequired(false);

        builder.Property(p => p.Email)
            .HasColumnType("varchar(100)")
            .IsRequired(false);

        builder.Property(p => p.Email)
            .HasColumnType("varchar(100)")
            .IsRequired(false);

        builder.Property(p => p.Naturality)
            .HasColumnType("varchar(100)")
            .IsRequired(false);

        builder.Property(p => p.IdentificationType)
            .HasColumnType("varchar(25)")
            .IsRequired(true);

        builder.Property(p => p.Gender)
            .HasColumnType("varchar(25)")
            .IsRequired(true);

        builder.Property(p => p.Habilitation)
            .HasColumnType("varchar(25)")
            .IsRequired(true);

        builder.HasIndex(p => p.NIF)
            .IsUnique();
    }
}
