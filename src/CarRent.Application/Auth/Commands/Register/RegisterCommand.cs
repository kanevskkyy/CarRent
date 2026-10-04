using CarRent.Application.Abstractions;
using CarRent.Application.DTOs.Auth;

namespace CarRent.Application.Auth.Commands.Register;

public record RegisterCommand(RegisterRequest Request) : ICommand<AuthResponse>;
