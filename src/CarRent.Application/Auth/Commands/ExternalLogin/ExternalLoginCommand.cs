using CarRent.Application.Abstractions;
using CarRent.Application.DTOs.Auth;

namespace CarRent.Application.Auth.Commands.ExternalLogin;

public record ExternalLoginCommand(string Provider, ExternalLoginRequest Request) : ICommand<AuthResponse>;