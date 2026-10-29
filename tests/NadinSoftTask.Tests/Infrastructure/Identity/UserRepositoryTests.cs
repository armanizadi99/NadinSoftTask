using Microsoft.AspNetCore.Identity;
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
    public async Task CreateAsync_WhenUsernameAlreadyExists_ReturnsConflictWithoutCreating()
    {
        TestContext context = new();
        context.GivenExistingUser();

        Result<UserDto> result = await context.Repository.CreateAsync(Username, Password);

        result.IsFailure.ShouldBeTrue();
        result.Error.Kind.ShouldBe(DomainErrorKind.Conflict);
        context.VerifyCreateNotCalled();
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

            _userManager
                .Setup(manager => manager.FindByNameAsync(Username))
                .ReturnsAsync((ApplicationUser?)null);

            Repository = new UserRepository(_userManager.Object);
        }

        public UserRepository Repository { get; }

        public void GivenExistingUser() =>
            _userManager
                .Setup(manager => manager.FindByNameAsync(Username))
                .ReturnsAsync(new ApplicationUser(Username));

        public void GivenCreateReturns(IdentityResult result) =>
            _userManager
                .Setup(manager => manager.CreateAsync(
                    It.Is<ApplicationUser>(user => user.UserName == Username),
                    Password))
                .ReturnsAsync(result);

        public void VerifyCreateNotCalled() =>
            _userManager.Verify(
                manager => manager.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>()),
                Times.Never);
    }
}
