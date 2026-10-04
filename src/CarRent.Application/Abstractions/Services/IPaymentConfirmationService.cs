using CarRent.Domain.Entities;

namespace CarRent.Application.Interfaces.Services;

public interface IPaymentConfirmationService
{
    Task ConfirmAsync(Payment payment, CancellationToken cancellationToken);
}