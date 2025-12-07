using Microsoft.Extensions.Caching.Memory;

namespace GymManager.Api.Utilities
{
    public class SimpleRateLimiter
    {
        private readonly IMemoryCache _cache;
        public SimpleRateLimiter(IMemoryCache cache) { _cache = cache; }

        public bool TryConsume(string key, int limit = 60, TimeSpan? period = null)
        {
            period ??= TimeSpan.FromMinutes(1);
            var entry = _cache.GetOrCreate(key, e =>
            {
                e.AbsoluteExpirationRelativeToNow = period;
                return new RateEntry { Count = 0 };
            });

            if (entry.Count >= limit) return false;
            entry.Count++;
            _cache.Set(key, entry, period.Value);
            return true;
        }

        private class RateEntry { public int Count { get; set; } }
    }
