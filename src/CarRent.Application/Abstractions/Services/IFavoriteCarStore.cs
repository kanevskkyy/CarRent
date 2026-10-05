namespace CarRent.Application.Interfaces.Services;

public interface IFavoriteCarStore
{
    Task AddAsync(Guid userId, Guid carId);
    Task RemoveAsync(Guid userId, Guid carId);
    Task RemoveManyAsync(Guid userId, IEnumerable<Guid> carIds);
    Task<HashSet<Guid>> GetFavoriteIdsAsync(Guid userId, IEnumerable<Guid> carIds);

    Task<(IReadOnlyList<Guid> CarIds, long TotalCount)> GetPageAsync(Guid userId, int pageNumber, int pageSize);
}