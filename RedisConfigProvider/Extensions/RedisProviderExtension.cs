using Microsoft.Extensions.Configuration;
using RedisConfigProvider;
using StackExchange.Redis;
using System;

namespace Microsoft.Extensions.Configuration
{
    public static class RedisProviderExtension
    {
        public static IConfigurationBuilder AddRedisConfiguration(this IConfigurationBuilder builder, string connStr,
            int dbNumber, bool reloadOnChange = false, TimeSpan? reloadInterval = null)
        {
            return AddConfiguration(builder, new RedisConfigOptions
            {
                ConnectionMultiplexer = ConnectionMultiplexer.Connect(connStr),
                DbNumber = dbNumber,
                ReloadOnChange = reloadOnChange,
                ReloadInterval = reloadInterval
            });
        }
        public static IConfigurationBuilder AddConfiguration(this IConfigurationBuilder builder,
            RedisConfigOptions options)
        {
            return builder.Add(new RedisConfigSource(options));
        }
    }
}
