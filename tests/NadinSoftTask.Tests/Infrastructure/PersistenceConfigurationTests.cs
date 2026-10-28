using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using NadinSoftTask.Domain.Products;
using NadinSoftTask.Infrastructure.Identity;
using NadinSoftTask.Infrastructure.Persistence;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Infrastructure;

public class PersistenceConfigurationTests
{
    private const string ConnectionString =
        "Server=localhost;Database=NadinSoftTaskDb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=True";

    [Fact]
    public void ProductEmailPredicate_TranslatesToSql()
    {
        using AppDbContext dbContext = CreateDbContext();
        Email email = new("owner@acme.com");

        string sql = dbContext.Products
            .Where(product => product.ManufactureEmail == email)
            .ToQueryString();

        sql.ShouldContain("ManufactureEmail");
    }

    [Fact]
    public void ProductOwnerPredicate_TranslatesToSql()
    {
        using AppDbContext dbContext = CreateDbContext();
        string ownerId = Guid.NewGuid().ToString();

        string sql = dbContext.Products
            .Where(product => product.OwnerId == ownerId)
            .ToQueryString();

        sql.ShouldContain("OwnerId");
    }

    [Fact]
    public void RefreshTokenHashPredicate_TranslatesToSql()
    {
        using AppDbContext dbContext = CreateDbContext();
        byte[] tokenHash = RefreshToken.HashToken("refresh-token");

        string sql = dbContext.RefreshTokens
            .Where(refreshToken => refreshToken.TokenHash == tokenHash)
            .ToQueryString();

        sql.ShouldContain("TokenHash");
    }

    [Fact]
    public void ProductModel_UsesCompactColumnTypes()
    {
        using AppDbContext dbContext = CreateDbContext();
        IEntityType product = FindDesignTimeEntityType<Product>(dbContext);

        product.FindProperty(nameof(Product.Id))!.GetColumnType().ShouldBe("int");
        product.FindProperty(nameof(Product.OwnerId))!.GetColumnType().ShouldBe("uniqueidentifier");
        product.FindProperty(nameof(Product.Name))!.GetColumnType().ShouldBe("varchar(300)");
        product.FindProperty(nameof(Product.Name))!.GetCollation().ShouldBe("Persian_100_CI_AS_SC_UTF8");
        product.FindProperty(nameof(Product.ManufacturePhone))!.GetColumnType().ShouldBe("varchar(42)");
        product.FindProperty(nameof(Product.ManufacturePhone))!.GetCollation()
            .ShouldBe("Latin1_General_100_CI_AS_SC_UTF8");
        product.FindProperty(nameof(Product.ManufactureEmail))!.GetColumnType().ShouldBe("varchar(254)");
        product.FindProperty(nameof(Product.ManufactureEmail))!.GetCollation()
            .ShouldBe("Latin1_General_100_CI_AS_SC_UTF8");
        product.FindProperty(nameof(Product.IsAvailable))!.GetColumnType().ShouldBe("bit");
        product.FindProperty(nameof(Product.ProduceDate))!.GetColumnType().ShouldBe("date");
    }

    [Fact]
    public void RefreshTokenModel_StoresOnlyHashAndTimestamps()
    {
        using AppDbContext dbContext = CreateDbContext();
        IEntityType refreshToken = FindEntityType<RefreshToken>(dbContext);

        refreshToken.FindProperty(nameof(RefreshToken.TokenHash))!.GetColumnType().ShouldBe("binary(32)");
        refreshToken.FindProperty(nameof(RefreshToken.UserId))!.GetColumnType().ShouldBe("uniqueidentifier");
        refreshToken.FindProperty(nameof(RefreshToken.ExpiresAt))!.GetColumnType().ShouldBe("smalldatetime");
        refreshToken.FindProperty(nameof(RefreshToken.CreatedAt))!.GetColumnType().ShouldBe("smalldatetime");
        refreshToken.FindProperty(nameof(RefreshToken.RevokedAt))!.GetColumnType().ShouldBe("smalldatetime");
        refreshToken.FindProperty(nameof(RefreshToken.IsRevoked)).ShouldBeNull();
    }

    [Fact]
    public void UserModel_UsesGuidKeyAndDropsUnusedIdentityColumns()
    {
        using AppDbContext dbContext = CreateDbContext();
        IEntityType user = FindEntityType<ApplicationUser>(dbContext);

        user.FindProperty(nameof(ApplicationUser.Id))!.GetColumnType().ShouldBe("uniqueidentifier");
        user.FindProperty(nameof(ApplicationUser.UserName))!.GetColumnType().ShouldBe("varchar(50)");
        user.FindProperty(nameof(ApplicationUser.NormalizedUserName))!.GetColumnType().ShouldBe("varchar(50)");
        user.FindProperty(nameof(ApplicationUser.PasswordHash))!.GetColumnType().ShouldBe("varchar(256)");
        user.FindProperty(nameof(ApplicationUser.Email)).ShouldBeNull();
        user.FindProperty(nameof(ApplicationUser.NormalizedEmail)).ShouldBeNull();
        user.FindProperty(nameof(ApplicationUser.EmailConfirmed)).ShouldBeNull();
        user.FindProperty(nameof(ApplicationUser.PhoneNumber)).ShouldBeNull();
        user.FindProperty(nameof(ApplicationUser.PhoneNumberConfirmed)).ShouldBeNull();
        user.FindProperty(nameof(ApplicationUser.TwoFactorEnabled)).ShouldBeNull();
    }

    [Fact]
    public void ProductModel_UsesOwnerAndDuplicateGuardIndexes()
    {
        using AppDbContext dbContext = CreateDbContext();
        IEntityType product = FindEntityType<Product>(dbContext);

        product.GetIndexes().ShouldContain(index =>
            index.GetDatabaseName() == "IX_Products_OwnerId");

        product.GetIndexes().ShouldContain(index =>
            index.GetDatabaseName() == "UX_Products_ProduceDate_ManufactureEmail" &&
            index.IsUnique);

        IEntityType refreshToken = FindEntityType<RefreshToken>(dbContext);
        refreshToken.GetIndexes().ShouldContain(index =>
            index.GetDatabaseName() == "UX_RefreshTokens_TokenHash" &&
            index.IsUnique);
    }

    private static AppDbContext CreateDbContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new AppDbContext(options);
    }

    private static IEntityType FindEntityType<TEntity>(AppDbContext dbContext) =>
        dbContext.Model.FindEntityType(typeof(TEntity))
        ?? throw new InvalidOperationException($"Entity type '{typeof(TEntity).Name}' is not mapped.");

    private static IEntityType FindDesignTimeEntityType<TEntity>(AppDbContext dbContext) =>
        dbContext.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(TEntity))
        ?? throw new InvalidOperationException($"Entity type '{typeof(TEntity).Name}' is not mapped.");
}
