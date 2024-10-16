using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedisConfigProvider.Publish;

namespace RedisConfigProvider.Extensions;

public static class RedisPublishExtension
{
    public static IServiceCollection AddRedisPublishService(this IServiceCollection service, Action<RedisConfigOptions> options)
    {
        service.Configure(options);
        return service.AddSingleton<IRedisConfigPublish, RedisConfigPublish>();
    }
}

