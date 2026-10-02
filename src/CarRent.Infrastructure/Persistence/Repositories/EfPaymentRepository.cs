using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;

namespace CarRent.Infrastructure.Persistence.Repositories;

public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(CarRentDbContext context) : base(context)
    {
        
    }
}