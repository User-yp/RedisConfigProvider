using Microsoft.Extensions.Configuration;

namespace RedisConfigProvider
{
    public class RedisConfigSource : IConfigurationSource
    {
        private readonly RedisConfigOptions options;

        public RedisConfigSource(RedisConfigOptions options)
        {
            this.options = options;
        }
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new RedisConfigProvider(options);
        }
    }
}
