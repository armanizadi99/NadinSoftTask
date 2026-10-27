using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NadinSoftTask.Application.Common;

namespace NadinSoftTask.Infrastructure.Identity;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("AspNetUsers");

        // Identity configures an index on NormalizedEmail by default. Remove it before
        // ignoring the email members so the model remains valid.
        var emailIndex = builder.Metadata
            .GetIndexes()
            .FirstOrDefault(index => index.Properties.Any(
                property => property.Name == nameof(ApplicationUser.NormalizedEmail)));

        if (emailIndex is not null)
        {
            builder.Metadata.RemoveIndex(emailIndex);
        }

        // Identity maps email, phone and 2FA by default. This application authenticates
        // by username only, so those unused columns are removed from the model.
        builder.Ignore(user => user.Email);
        builder.Ignore(user => user.NormalizedEmail);
        builder.Ignore(user => user.EmailConfirmed);
        builder.Ignore(user => user.PhoneNumber);
        builder.Ignore(user => user.PhoneNumberConfirmed);
        builder.Ignore(user => user.TwoFactorEnabled);

        builder.Property(user => user.UserName)
            .HasMaxLength(UserConstraints.MaxUsernameLength)
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(user => user.NormalizedUserName)
            .HasMaxLength(UserConstraints.MaxUsernameLength)
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(256)
            .HasColumnType("varchar(256)");

        builder.Property(user => user.SecurityStamp)
            .HasMaxLength(64)
            .HasColumnType("varchar(64)");

        builder.Property(user => user.ConcurrencyStamp)
            .HasMaxLength(36)
            .HasColumnType("varchar(36)");

        builder.Property(user => user.LockoutEnd)
            .HasColumnType("datetimeoffset(0)");

        builder.Property(user => user.LockoutEnabled)
            .HasColumnType("bit");

        builder.Property(user => user.AccessFailedCount)
            .HasColumnType("int");
    }
}
