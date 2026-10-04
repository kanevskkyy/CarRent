using CarRent.Application.Abstractions;
using CarRent.Application.DTOs.Auth;

namespace CarRent.Application.Auth.Commands.Login;

public record LoginCommand(LoginRequest Request) : ICommand<AuthResponse>;
