using CarRent.Application.Interfaces.Services;
using StackExchange.Redis;

namespace CarRent.Infrastructure.Services;

public class RedisFavoriteCarStore : IFavoriteCarStore
{
    private readonly IDatabase _db;

    public RedisFavoriteCarStore(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    private static string Key(Guid userId) => $"favorites:{userId}";

    public async Task AddAsync(Guid userId, Guid carId)
    {
        await _db.SortedSetAddAsync(
            Key(userId),
            carId.ToString(),
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            When.NotExists);
    }

    public async Task RemoveAsync(Guid userId, Guid carId)
    {
        await _db.SortedSetRemoveAsync(Key(userId), carId.ToString());
    }

    public async Task RemoveManyAsync(Guid userId, IEnumerable<Guid> carIds)
    {
        RedisValue[] values = carIds.Select(id => (RedisValue)id.ToString()).ToArray();
        if (values.Length > 0)
            await _db.SortedSetRemoveAsync(Key(userId), values);
    }

    public async Task<(IReadOnlyList<Guid> CarIds, long TotalCount)> GetPageAsync(
        Guid userId, int pageNumber, int pageSize)
    {
        string key = Key(userId);

        long total = await _db.SortedSetLengthAsync(key);
        if (total == 0)
            return (Array.Empty<Guid>(), 0);

        long start = (long)(pageNumber - 1) * pageSize;
        long stop = start + pageSize - 1;

        RedisValue[] values = await _db.SortedSetRangeByRankAsync(key, start, stop, Order.Descending);

        List<Guid> ids = values
            .Select(v => Guid.TryParse(v.ToString(), out Guid id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToList();

        return (ids, total);
    }
}