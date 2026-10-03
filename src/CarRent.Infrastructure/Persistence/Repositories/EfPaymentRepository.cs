using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;

namespace CarRent.Infrastructure.Persistence.Repositories;

public class EfPaymentRepository : EfGenericRepository<Payment>, IPaymentRepository
{
    public EfPaymentRepository(CarRentDbContext context) : base(context)
    {
        
    }
}