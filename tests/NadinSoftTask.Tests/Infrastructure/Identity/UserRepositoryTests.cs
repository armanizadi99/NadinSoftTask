using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Infrastructure.Identity;
using Shouldly;
using Xunit;

namespace NadinSoftTask.Tests.Infrastructure.Identity;

public class UserRepositoryTests
{
    private const string Username = "arman";
    private const string Password = "password";

    [Fact]
    public async Task CreateAsync_WhenUsernameIndexIsViolated_ReturnsConflict()
    {
        TestContext context = new();
        DbUpdateException exception = new(
            "An error occurred while saving the entity changes.",
            CreateSqlException(
                2601,
                "Cannot insert duplicate key row in object 'dbo.AspNetUsers' " +
                "with unique index 'UserNameIndex'."));

        context.GivenCreateThrows(exception);

        Result<UserDto> result = await context.Repository.CreateAsync(Username, Password);

        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Error.Kind.ShouldBe(DomainErrorKind.Conflict);
    }

    [Fact]
    public async Task CreateAsync_WhenUsernameConstraintIsViolated_ReturnsConflict()
    {
        TestContext context = new();
        DbUpdateException exception = new(
            "An error occurred while saving the entity changes.",
            CreateSqlException(
                2627,
                "Violation of UNIQUE KEY constraint 'UserNameIndex'."));

        context.GivenCreateThrows(exception);

        Result<UserDto> result = await context.Repository.CreateAsync(Username, Password);

        result.IsFailure.ShouldBeTrue();
        result.Error.Kind.ShouldBe(DomainErrorKind.Conflict);
    }

    [Fact]
    public async Task CreateAsync_WhenSqlExceptionIsNested_ReturnsConflict()
    {
        TestContext context = new();
        DbUpdateException exception = new(
            "An error occurred while saving the entity changes.",
            new InvalidOperationException(
                "Wrapped database error.",
                CreateSqlException(
                    2601,
                    "Cannot insert duplicate key row in object 'dbo.AspNetUsers' " +
                    "with unique index 'UserNameIndex'.")));

        context.GivenCreateThrows(exception);

        Result<UserDto> result = await context.Repository.CreateAsync(Username, Password);

        result.IsFailure.ShouldBeTrue();
        result.Error.Kind.ShouldBe(DomainErrorKind.Conflict);
    }

    [Fact]
    public async Task CreateAsync_WhenAnotherUniqueIndexIsViolated_PropagatesException()
    {
        TestContext context = new();
        DbUpdateException exception = new(
            "An error occurred while saving the entity changes.",
            CreateSqlException(
                2601,
                "Cannot insert duplicate key row in object 'dbo.Products' " +
                "with unique index 'UX_Products_ProduceDate_ManufactureEmail'."));

        context.GivenCreateThrows(exception);

        DbUpdateException thrown = await Should.ThrowAsync<DbUpdateException>(
            () => context.Repository.CreateAsync(Username, Password));

        thrown.ShouldBeSameAs(exception);
    }

    [Fact]
    public async Task CreateAsync_WhenDatabaseErrorIsNotAUniqueViolation_PropagatesException()
    {
        TestContext context = new();
        DbUpdateException exception = new(
            "An error occurred while saving the entity changes.",
            CreateSqlException(-2, "Timeout expired."));

        context.GivenCreateThrows(exception);

        DbUpdateException thrown = await Should.ThrowAsync<DbUpdateException>(
            () => context.Repository.CreateAsync(Username, Password));

        thrown.ShouldBeSameAs(exception);
    }

    [Fact]
    public async Task CreateAsync_WhenExceptionHasNoSqlError_PropagatesException()
    {
        TestContext context = new();
        DbUpdateException exception = new(
            "An error occurred while saving the entity changes.",
            new InvalidOperationException("The connection was closed."));

        context.GivenCreateThrows(exception);

        DbUpdateException thrown = await Should.ThrowAsync<DbUpdateException>(
            () => context.Repository.CreateAsync(Username, Password));

        thrown.ShouldBeSameAs(exception);
    }

    [Fact]
    public async Task CreateAsync_WhenIdentityReportsDuplicateUsername_ReturnsConflict()
    {
        TestContext context = new();
        IdentityResult identityResult = IdentityResult.Failed(new IdentityError
        {
            Code = nameof(IdentityErrorDescriber.DuplicateUserName),
            Description = "Username is already taken."
        });

        context.GivenCreateReturns(identityResult);

        Result<UserDto> result = await context.Repository.CreateAsync(Username, Password);

        result.IsFailure.ShouldBeTrue();
        result.Error.Kind.ShouldBe(DomainErrorKind.Conflict);
    }

    [Fact]
    public async Task CreateAsync_WhenIdentityReportsValidationError_ReturnsValidation()
    {
        TestContext context = new();
        IdentityResult identityResult = IdentityResult.Failed(new IdentityError
        {
            Code = "PasswordTooShort",
            Description = "Password is too short."
        });

        context.GivenCreateReturns(identityResult);

        Result<UserDto> result = await context.Repository.CreateAsync(Username, Password);

        result.IsFailure.ShouldBeTrue();
        result.Error.Kind.ShouldBe(DomainErrorKind.Validation);
        result.Error.Message.ShouldBe("Password is too short.");
    }

    [Fact]
    public async Task CreateAsync_WhenIdentitySucceeds_ReturnsCreatedUser()
    {
        TestContext context = new();
        context.GivenCreateReturns(IdentityResult.Success);

        Result<UserDto> result = await context.Repository.CreateAsync(Username, Password);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeSameAs(DomainError.None);
        result.Value.ShouldNotBeNull();
        result.Value.Username.ShouldBe(Username);
        Guid.TryParse(result.Value.Id, out _).ShouldBeTrue();
    }

    // SqlException has no public constructor, so build one through the internal
    // constructor used by Microsoft.Data.SqlClient.
    private static SqlException CreateSqlException(int number, string message)
    {
        object error = Activator.CreateInstance(
            typeof(SqlError),
            BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            args:
            [
                number,
                (byte)0,
                (byte)0,
                "test-server",
                message,
                "test-procedure",
                0,
                0u,
                null
            ],
            culture: null)!;

        object errors = Activator.CreateInstance(
            typeof(SqlErrorCollection),
            nonPublic: true)!;

        typeof(SqlErrorCollection)
            .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(errors, [error]);

        return (SqlException)Activator.CreateInstance(
            typeof(SqlException),
            BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            args: [message, errors, null, Guid.Empty],
            culture: null)!;
    }

    private sealed class TestContext
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManager;

        public TestContext()
        {
            _userManager = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object,
                Options.Create(new IdentityOptions()),
                new PasswordHasher<ApplicationUser>(),
                Array.Empty<IUserValidator<ApplicationUser>>(),
                Array.Empty<IPasswordValidator<ApplicationUser>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                new ServiceCollection().BuildServiceProvider(),
                NullLogger<UserManager<ApplicationUser>>.Instance);

            Repository = new UserRepository(_userManager.Object);
        }

        public UserRepository Repository { get; }

        public void GivenCreateReturns(IdentityResult result) =>
            _userManager
                .Setup(manager => manager.CreateAsync(
                    It.Is<ApplicationUser>(user => user.UserName == Username),
                    Password))
                .ReturnsAsync(result);

        public void GivenCreateThrows(DbUpdateException exception) =>
            _userManager
                .Setup(manager => manager.CreateAsync(
                    It.Is<ApplicationUser>(user => user.UserName == Username),
                    Password))
                .ThrowsAsync(exception);
    }
}
