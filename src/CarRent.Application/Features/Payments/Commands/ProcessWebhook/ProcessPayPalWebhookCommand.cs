using CarRent.Application.Abstractions;
using CarRent.Application.Models.Payments;

namespace CarRent.Application.Payments.Commands.ProcessWebhook;

public record ProcessPayPalWebhookCommand(WebhookHeaders Headers, string RawBody) : ICommand;