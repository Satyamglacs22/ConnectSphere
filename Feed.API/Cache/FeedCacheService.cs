using System.Text.Json;
using Feed.API.Entities;
using StackExchange.Redis;

namespace Feed.API.Cache
{
    public class FeedCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<FeedCacheService> _logger;

        // Cache TTL — feed stays in cache for 10 minutes
        private readonly TimeSpan _ttl = TimeSpan.FromMinutes(10);

        public FeedCacheService(
            IConnectionMultiplexer redis,
            ILogger<FeedCacheService> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        // Cache key format: "feed:{userId}"
        private string CacheKey(int userId) => $"feed:{userId}";

        // Get feed from cache
        public async Task<List<FeedItemEntity>?> GetFeed(int userId)
        {
            try
            {
                var db = _redis.GetDatabase();
                var cached = await db.StringGetAsync(CacheKey(userId));

                if (cached.IsNullOrEmpty)
                {
                    _logger.LogInformation(
                        "Cache MISS for user {userId}", userId);
                    return null;
                }

                _logger.LogInformation(
                    "Cache HIT for user {userId}", userId);

                return JsonSerializer.Deserialize<List<FeedItemEntity>>(
    cached.ToString()!);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "Redis error on GetFeed: {msg}", ex.Message);
                return null;
            }
        }

        // Save feed to cache
        public async Task SetFeed(int userId, List<FeedItemEntity> feed)
        {
            try
            {
                var db = _redis.GetDatabase();
                var serialized = JsonSerializer.Serialize(feed);

                await db.StringSetAsync(
                    CacheKey(userId),
                    serialized,
                    _ttl);

                _logger.LogInformation(
                    "Feed cached for user {userId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "Redis error on SetFeed: {msg}", ex.Message);
            }
        }

        // Invalidate cache when new post arrives
        public async Task InvalidateFeed(int userId)
        {
            try
            {
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(CacheKey(userId));

                _logger.LogInformation(
                    "Cache invalidated for user {userId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "Redis error on InvalidateFeed: {msg}", ex.Message);
            }
        }

        // Invalidate multiple users feeds at once
        public async Task InvalidateMultipleFeeds(IList<int> userIds)
        {
            try
            {
                var db = _redis.GetDatabase();
                var keys = userIds
                    .Select(id => (RedisKey)CacheKey(id))
                    .ToArray();

                await db.KeyDeleteAsync(keys);

                _logger.LogInformation(
                    "Cache invalidated for {count} users", userIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "Redis error on InvalidateMultipleFeeds: {msg}",
                    ex.Message);
            }
        }
    }
}