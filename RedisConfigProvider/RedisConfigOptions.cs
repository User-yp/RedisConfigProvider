using StackExchange.Redis;
using System;

namespace Microsoft.Extensions.Configuration
{
    public class RedisConfigOptions
    {
        public Func<ConnectionMultiplexer> ConnectionMultiplexer { get; set; }
        public int DbNumber { get; set; } = 0;
        public bool ReloadOnChange { get; set; }
        public TimeSpan? ReloadInterval { get; set; }
    }

}
