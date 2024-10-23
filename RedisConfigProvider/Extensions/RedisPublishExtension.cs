using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedisConfigProvider.PublishConfig;
using StackExchange.Redis;
using System;

namespace RedisConfigProvider.Extensions
{
    public static class RedisPublishExtension
    {
        public static IServiceCollection AddRedisPublishService(this IServiceCollection service, string connStr,int dbNumber)
        {
            service.Configure<RedisConfigOptions>(options =>
            {
                options.ConnectionMultiplexer = ConnectionMultiplexer.Connect(connStr);
                options.DbNumber = dbNumber;
            });
            return service.AddSingleton<IRedisConfigPublish, RedisConfigPublish>();
        }
    }

}
