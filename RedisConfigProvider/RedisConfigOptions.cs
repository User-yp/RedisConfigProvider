using StackExchange.Redis;

namespace Microsoft.Extensions.Configuration;

public class RedisConfigOptions
{
    public Func<ConnectionMultiplexer> ConnectionString { get; set; }
    public int DbNumber { get; set; } = 0;
    public bool ReloadOnChange { get; set; }
    public TimeSpan? ReloadInterval { get; set; }
}

