using CarRent.Application.Auth.Commands.RevokeToken;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Auth;

public class RevokeTokenCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
    private readonly RevokeTokenCommandHandler _handler;

    public RevokeTokenCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
        _unitOfWorkMock.Setup(u => u.RefreshTokens).Returns(_refreshTokenRepoMock.Object);

        _handler = new RevokeTokenCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTokenNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        RevokeTokenCommand command = new(new RevokeTokenRequest { RefreshToken = "missing-token" });
        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(command.Request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Refresh token is invalid or already revoked.");
    }

    [Fact]
    public async Task Handle_WhenTokenAlreadyRevoked_ThrowsUnauthorizedException()
    {
        // Arrange
        RefreshToken revokedToken = new()
        {
            Token = "already-revoked",
            IsRevoked = true,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(1)
        };
        RevokeTokenCommand command = new(new RevokeTokenRequest { RefreshToken = revokedToken.Token });

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(command.Request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(revokedToken);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Refresh token is invalid or already revoked.");
    }

    [Fact]
    public async Task Handle_WhenTokenActive_RevokesTokenAndSaves()
    {
        // Arrange
        RefreshToken activeToken = new()
        {
            Token = "active-token",
            IsRevoked = false,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(7)
        };
        RevokeTokenCommand command = new(new RevokeTokenRequest { RefreshToken = activeToken.Token });

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(command.Request.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeToken);

        // Act
        MediatR.Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        activeToken.IsRevoked.Should().BeTrue();
        activeToken.RevokedOnUtc.Should().NotBeNull();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
