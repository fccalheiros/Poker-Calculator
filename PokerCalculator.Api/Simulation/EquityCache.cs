// Simple non-cumulative result cache keyed by canonicalized hero/board/villain-ranges.
// Backed by IMemoryCache so eviction under memory pressure is handled by the framework
// via SizeLimit, rather than a manual TTL.
using Microsoft.Extensions.Caching.Memory;

namespace PokerCalculator.Api.Simulation;

public record CachedEquityResult(SimulationResult Result, long Simulations);

public class EquityCache
{
    private readonly IMemoryCache _cache;

    public EquityCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool TryGet(string key, out CachedEquityResult result)
    {
        if (_cache.TryGetValue(key, out CachedEquityResult? cached) && cached is not null)
        {
            result = cached;
            return true;
        }
        result = default!;
        return false;
    }

    public void Set(string key, CachedEquityResult result)
    {
        _cache.Set(key, result, new MemoryCacheEntryOptions { Size = 1 });
    }
}
