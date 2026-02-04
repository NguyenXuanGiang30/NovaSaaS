using Microsoft.Extensions.Caching.Distributed;
using NovaSaaS.Application.Interfaces.Caching;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NovaSaaS.Infrastructure.Caching
{
    /// <summary>
    /// RedisCacheService - Implementation của ICacheService sử dụng Redis.
    /// Hỗ trợ multi-tenant caching và pattern-based invalidation.
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheService(
            IDistributedCache cache,
            IConnectionMultiplexer redis,
            ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _redis = redis;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        /// <inheritdoc />
        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var data = await _cache.GetStringAsync(key);
                if (string.IsNullOrEmpty(data))
                    return null;

                return JsonSerializer.Deserialize<T>(data, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache GET failed for key: {Key}", key);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            try
            {
                var data = JsonSerializer.Serialize(value, _jsonOptions);
                var options = new DistributedCacheEntryOptions();

                if (expiry.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiry.Value;
                }
                else
                {
                    // Default TTL: 30 phút
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                }

                await _cache.SetStringAsync(key, data, options);
                _logger.LogDebug("Cache SET: {Key} (TTL: {TTL})", key, expiry ?? TimeSpan.FromMinutes(30));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache SET failed for key: {Key}", key);
            }
        }

        /// <inheritdoc />
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogDebug("Cache REMOVE: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache REMOVE failed for key: {Key}", key);
            }
        }

        /// <inheritdoc />
        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var server = GetServer();
                if (server == null)
                {
                    _logger.LogWarning("Cannot get Redis server for pattern deletion");
                    return;
                }

                var db = _redis.GetDatabase();
                var keys = server.Keys(pattern: pattern);
                
                foreach (var key in keys)
                {
                    await db.KeyDeleteAsync(key);
                }

                _logger.LogDebug("Cache REMOVE by pattern: {Pattern}", pattern);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache REMOVE by pattern failed: {Pattern}", pattern);
            }
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var data = await _cache.GetStringAsync(key);
                return !string.IsNullOrEmpty(data);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache EXISTS check failed for key: {Key}", key);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null) where T : class
        {
            // Thử lấy từ cache trước
            var cached = await GetAsync<T>(key);
            if (cached != null)
            {
                _logger.LogDebug("Cache HIT: {Key}", key);
                return cached!;
            }

            // Cache miss - load từ factory
            _logger.LogDebug("Cache MISS: {Key}", key);
            var value = await factory();

            if (value != null)
            {
                await SetAsync(key, value, expiry);
            }

            return value;
        }

        /// <summary>
        /// Lấy Redis server để thực hiện pattern operations.
        /// </summary>
        private IServer? GetServer()
        {
            var endpoints = _redis.GetEndPoints();
            if (endpoints.Length == 0)
                return null;

            return _redis.GetServer(endpoints[0]);
        }
    }
}
