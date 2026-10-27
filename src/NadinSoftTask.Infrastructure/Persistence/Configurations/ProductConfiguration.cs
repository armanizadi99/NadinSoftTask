using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NadinSoftTask.Domain.Products;
using NadinSoftTask.Infrastructure.Persistence.Converters;

namespace NadinSoftTask.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    private static readonly ValueComparer<Email> EmailComparer = new(
        (left, right) => left == right,
        value => value.GetHashCode(),
        value => value);

    private static readonly ValueComparer<Phone> PhoneComparer = new(
        (left, right) => left == right,
        value => value.GetHashCode(),
        value => value);

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Id)
            .UseIdentityColumn();

        builder.Property(product => product.OwnerId)
            .HasConversion(UserIdValueConverter.Instance)
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        builder.HasIndex(product => product.OwnerId)
            .HasDatabaseName("IX_Products_OwnerId");

        builder.Property(product => product.Name)
            .HasColumnType("varchar(300)")
            .UseCollation("Persian_100_CI_AS_SC_UTF8")
            .IsRequired();

        builder.Property(product => product.ManufacturePhone)
            .HasConversion(
                phone => phone.Value,
                value => new Phone(value))
            .HasColumnType("varchar(42)")
            .UseCollation("Latin1_General_100_CI_AS_SC_UTF8")
            .IsRequired()
            .Metadata.SetValueComparer(PhoneComparer);

        builder.Property(product => product.ManufactureEmail)
            .HasConversion(
                email => email.Value,
                value => new Email(value))
            .HasColumnType("varchar(254)")
            .UseCollation("Latin1_General_100_CI_AS_SC_UTF8")
            .IsRequired()
            .Metadata.SetValueComparer(EmailComparer);

        builder.Property(product => product.IsAvailable)
            .HasColumnType("bit")
            .IsRequired();

        builder.Property(product => product.ProduceDate)
            .HasColumnType("date")
            .IsRequired();

        builder.HasIndex(product => new { product.ProduceDate, product.ManufactureEmail })
            .IsUnique()
            .HasDatabaseName("UX_Products_ProduceDate_ManufactureEmail");
    }
}
