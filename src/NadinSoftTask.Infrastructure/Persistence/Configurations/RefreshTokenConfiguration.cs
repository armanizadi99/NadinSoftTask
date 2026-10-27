using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NadinSoftTask.Domain.Users;
using NadinSoftTask.Infrastructure.Persistence.Converters;

namespace NadinSoftTask.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    private static readonly ValueComparer<byte[]> HashComparer = new(
        (left, right) => left != null && right != null && left.SequenceEqual(right),
        value => value.Aggregate(17, (hash, item) => HashCode.Combine(hash, item)),
        value => value.ToArray());

    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(refreshToken => refreshToken.Id);

        builder.Property(refreshToken => refreshToken.Id)
            .UseIdentityColumn();

        builder.Property(refreshToken => refreshToken.TokenHash)
            .HasColumnType("binary(32)")
            .IsRequired()
            .Metadata.SetValueComparer(HashComparer);

        builder.HasIndex(refreshToken => refreshToken.TokenHash)
            .IsUnique()
            .HasDatabaseName("UX_RefreshTokens_TokenHash");

        builder.Property(refreshToken => refreshToken.UserId)
            .HasConversion(UserIdValueConverter.Instance)
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        builder.HasIndex(refreshToken => refreshToken.UserId)
            .HasDatabaseName("IX_RefreshTokens_UserId");

        builder.Property(refreshToken => refreshToken.ExpiresAt)
            .HasConversion(DateTimeConverters.UtcDateTime)
            .HasColumnType("smalldatetime")
            .IsRequired();

        builder.Property(refreshToken => refreshToken.CreatedAt)
            .HasConversion(DateTimeConverters.UtcDateTime)
            .HasColumnType("smalldatetime")
            .IsRequired();

        builder.Property(refreshToken => refreshToken.RevokedAt)
            .HasConversion(DateTimeConverters.NullableUtcDateTime)
            .HasColumnType("smalldatetime");

        builder.Ignore(refreshToken => refreshToken.Token);
        builder.Ignore(refreshToken => refreshToken.IsRevoked);
    }
}
