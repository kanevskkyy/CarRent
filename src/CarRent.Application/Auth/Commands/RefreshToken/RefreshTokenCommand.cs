using CarRent.Application.Abstractions;
using CarRent.Application.DTOs.Auth;

namespace CarRent.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(RefreshTokenRequest Request) : ICommand<AuthResponse>;
