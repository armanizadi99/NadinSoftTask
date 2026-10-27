using Moq;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Domain.Users;

namespace NadinSoftTask.Tests.Application.TestSupport;

internal sealed class AuthTestContext
{
    public AuthTestContext()
    {
        UnitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    public Mock<IUserRepository> UserRepository { get; } = new(MockBehavior.Strict);

    public Mock<ITokenService> TokenService { get; } = new(MockBehavior.Strict);

    public Mock<IRefreshTokenRepository> RefreshTokenRepository { get; } = new(MockBehavior.Strict);

    public Mock<IUnitOfWork> UnitOfWork { get; } = new(MockBehavior.Strict);

    public void GivenCreateSucceeds(string username, string password, UserDto user) =>
        UserRepository
            .Setup(repository => repository.CreateAsync(
                username,
                password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(user));

    public void GivenCreateFails(string username, string password, DomainError error) =>
        UserRepository
            .Setup(repository => repository.CreateAsync(
                username,
                password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Failure(error));

    public void GivenUser(string username, UserDto user) =>
        UserRepository
            .Setup(repository => repository.GetByUsernameAsync(
                username,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

    public void GivenMissingUser(string username) =>
        UserRepository
            .Setup(repository => repository.GetByUsernameAsync(
                username,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

    public void GivenUserById(string userId, UserDto user) =>
        UserRepository
            .Setup(repository => repository.GetByIdAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

    public void GivenMissingUserById(string userId) =>
        UserRepository
            .Setup(repository => repository.GetByIdAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

    public void GivenPasswordValidity(string userId, string password, bool isValid) =>
        UserRepository
            .Setup(repository => repository.CheckPasswordAsync(
                userId,
                password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(isValid);

    public void GivenAccessToken(
        string userId,
        string username,
        string accessToken,
        DateTime expiresAt) =>
        TokenService
            .Setup(service => service.GenerateAccessToken(userId, username))
            .Returns((accessToken, expiresAt));

    public void GivenRefreshToken(string refreshToken) =>
        TokenService
            .Setup(service => service.GenerateRefreshToken())
            .Returns(refreshToken);

    public void GivenRefreshTokenExpiration(DateTime expiresAt) =>
        TokenService
            .Setup(service => service.GetRefreshTokenExpiration())
            .Returns(expiresAt);

    public void GivenStoredRefreshToken(string token, RefreshToken refreshToken) =>
        RefreshTokenRepository
            .Setup(repository => repository.GetByTokenAsync(
                token,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

    public void GivenMissingRefreshToken(string token) =>
        RefreshTokenRepository
            .Setup(repository => repository.GetByTokenAsync(
                token,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

    public void GivenAddSucceeds(Func<RefreshToken, bool>? predicate = null) =>
        RefreshTokenRepository
            .Setup(repository => repository.AddAsync(
                It.Is<RefreshToken>(refreshToken => predicate == null || predicate(refreshToken)),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

    public void VerifyCreatedOnce(string username, string password) =>
        UserRepository.Verify(
            repository => repository.CreateAsync(
                username,
                password,
                It.IsAny<CancellationToken>()),
            Times.Once);

    public void VerifyUserLookupOnce(string username) =>
        UserRepository.Verify(
            repository => repository.GetByUsernameAsync(
                username,
                It.IsAny<CancellationToken>()),
            Times.Once);

    public void VerifyUserByIdLookupOnce(string userId) =>
        UserRepository.Verify(
            repository => repository.GetByIdAsync(
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);

    public void VerifyUserByIdNotLookedUp() =>
        UserRepository.Verify(
            repository => repository.GetByIdAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    public void VerifyPasswordCheckedOnce(string userId, string password) =>
        UserRepository.Verify(
            repository => repository.CheckPasswordAsync(
                userId,
                password,
                It.IsAny<CancellationToken>()),
            Times.Once);

    public void VerifyPasswordNotChecked() =>
        UserRepository.Verify(
            repository => repository.CheckPasswordAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    public void VerifyRefreshTokenLookupOnce(string token) =>
        RefreshTokenRepository.Verify(
            repository => repository.GetByTokenAsync(
                token,
                It.IsAny<CancellationToken>()),
            Times.Once);

    public void VerifyAccessTokenGeneratedOnce(string userId, string username) =>
        TokenService.Verify(
            service => service.GenerateAccessToken(userId, username),
            Times.Once);

    public void VerifyAccessTokenNotGenerated() =>
        TokenService.Verify(
            service => service.GenerateAccessToken(
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);

    public void VerifyRefreshTokenGeneratedOnce() =>
        TokenService.Verify(
            service => service.GenerateRefreshToken(),
            Times.Once);

    public void VerifyRefreshTokenNotGenerated() =>
        TokenService.Verify(
            service => service.GenerateRefreshToken(),
            Times.Never);

    public void VerifyRefreshTokenExpirationRequestedOnce() =>
        TokenService.Verify(
            service => service.GetRefreshTokenExpiration(),
            Times.Once);

    public void VerifyRefreshTokenExpirationNotRequested() =>
        TokenService.Verify(
            service => service.GetRefreshTokenExpiration(),
            Times.Never);

    public void VerifyRefreshTokenAddedOnce() =>
        RefreshTokenRepository.Verify(
            repository => repository.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

    public void VerifyNotAdded() =>
        RefreshTokenRepository.Verify(
            repository => repository.AddAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    public void VerifySavedOnce() =>
        UnitOfWork.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

    public void VerifyNotSaved() =>
        UnitOfWork.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
}
