using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using CarRent.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Persistence.Repositories;

public class EfGenericRepository<T> : IGenericRepository<T> where T : class
{
    protected CarRentDbContext context;
    protected DbSet<T> dbSet;

    public EfGenericRepository(CarRentDbContext context)
    {
        this.context = context;
        dbSet = this.context.Set<T>();
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        dbSet.Update(entity);
    }

    public IQueryable<T> ApplySpecification(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        if (spec == null) throw new ArgumentNullException(nameof(spec));

        SpecificationEvaluator evaluator = new SpecificationEvaluator();
        return evaluator
            .GetQuery(dbSet.AsQueryable(), spec)
            .AsSplitQuery();
    }

    public void Delete(T entity)
    {
        dbSet.Remove(entity);
    }
}