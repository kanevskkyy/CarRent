using MediatR;
using CarRent.Application.Abstractions;
using CarRent.Application.Auth.Commands.RevokeToken;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;

namespace CarRent.Application.Auth.Commands.RevokeToken;

public class RevokeTokenCommandHandler : ICommandHandler<RevokeTokenCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public RevokeTokenCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RevokeTokenCommand command, CancellationToken cancellationToken)
    {
        Domain.Entities.RefreshToken? storedToken = await _unitOfWork.RefreshTokens
            .GetByTokenAsync(command.Request.RefreshToken, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
            throw new UnauthorizedException("Refresh token is invalid or already revoked.");

        storedToken.IsRevoked = true;
        storedToken.RevokedOnUtc = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
