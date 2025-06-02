using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NERBABO.ApiService.Core.Account.Models;

namespace NERBABO.ApiService.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.Property(u => u.UserName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.NormalizedUserName)
            .HasMaxLength(50);

        builder.Property(u => u.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.NormalizedEmail)
            .HasMaxLength(100);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(13);

        builder.Property(u => u.LockoutEnd)
            .HasColumnType("timestamp with time zone");

        builder.Property(u => u.EmailConfirmed)
            .HasColumnName("IsEmailConfirmed");

        builder.Property(u => u.PhoneNumberConfirmed)
            .HasColumnName("IsPhoneConfirmed");

        builder.Property(u => u.TwoFactorEnabled)
            .HasColumnName("IsTwoFactorEnabled");

        builder.Property(u => u.LockoutEnabled)
            .HasColumnName("IsLockoutEnabled");
    }
}
