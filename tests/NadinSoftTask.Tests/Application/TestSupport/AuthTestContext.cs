using Moq;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Tests.Application.TestSupport;

internal sealed class AuthTestContext
{
    public Mock<IUserRepository> UserRepository { get; } = new(MockBehavior.Strict);

    public Mock<ITokenService> TokenService { get; } = new(MockBehavior.Strict);

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

    public void GivenPasswordValidity(string userId, string password, bool isValid) =>
        UserRepository
            .Setup(repository => repository.CheckPasswordAsync(
                userId,
                password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(isValid);

    public void GivenGenerateTokens(
        string userId,
        string username,
        Result<AuthResult> result) =>
        TokenService
            .Setup(service => service.GenerateTokensAsync(
                userId,
                username,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

    public void GivenRefreshTokens(string refreshToken, Result<AuthResult> result) =>
        TokenService
            .Setup(service => service.RefreshTokensAsync(
                refreshToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

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

    public void VerifyGenerateTokensOnce(string userId, string username) =>
        TokenService.Verify(
            service => service.GenerateTokensAsync(
                userId,
                username,
                It.IsAny<CancellationToken>()),
            Times.Once);

    public void VerifyGenerateTokensNotCalled() =>
        TokenService.Verify(
            service => service.GenerateTokensAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    public void VerifyRefreshTokensOnce(string refreshToken) =>
        TokenService.Verify(
            service => service.RefreshTokensAsync(
                refreshToken,
                It.IsAny<CancellationToken>()),
            Times.Once);

    public void VerifyRefreshTokensNotCalled() =>
        TokenService.Verify(
            service => service.RefreshTokensAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
}
