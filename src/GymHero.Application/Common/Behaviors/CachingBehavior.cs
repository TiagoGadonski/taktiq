using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace GymHero.Application.Common.Behaviors;

/// <summary>
/// Caching behavior for MediatR requests that implement ICacheableQuery
/// </summary>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(IDistributedCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only cache queries that implement ICacheableQuery
        if (request is not ICacheableQuery cacheableQuery)
        {
            return await next();
        }

        var cacheKey = cacheableQuery.CacheKey;
        var cachedResponse = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedResponse))
        {
            _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
            return JsonSerializer.Deserialize<TResponse>(cachedResponse)!;
        }

        _logger.LogInformation("Cache miss for key: {CacheKey}", cacheKey);
        var response = await next();

        if (response != null)
        {
            var serializedResponse = JsonSerializer.Serialize(response);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheableQuery.CacheDuration
            };

            await _cache.SetStringAsync(cacheKey, serializedResponse, cacheOptions, cancellationToken);
            _logger.LogInformation("Cached response for key: {CacheKey} with duration: {Duration}",
                cacheKey, cacheableQuery.CacheDuration);
        }

        return response;
    }
}

/// <summary>
/// Marker interface for queries that should be cached
/// </summary>
public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan CacheDuration { get; }
}
