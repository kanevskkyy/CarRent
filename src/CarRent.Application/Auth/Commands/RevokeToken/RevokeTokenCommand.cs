using CarRent.Application.Abstractions;
using CarRent.Application.DTOs.Auth;

namespace CarRent.Application.Auth.Commands.RevokeToken;

public record RevokeTokenCommand(RevokeTokenRequest Request) : ICommand;
