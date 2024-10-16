using Microsoft.Extensions.Configuration;
using RedisConfigProvider;
using StackExchange.Redis;

namespace Microsoft.Extensions.Configuration;

public static class RedisConfigProviderExtensions
{
    public static IConfigurationBuilder AddConfiguration(this IConfigurationBuilder builder, Func<ConnectionMultiplexer> ConnectionString,
        int DbNumber, bool reloadOnChange = false, TimeSpan? reloadInterval = null)
    {
        return AddConfiguration(builder, new RedisConfigOptions
        {
            ConnectionString = ConnectionString,
            DbNumber = DbNumber,
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
