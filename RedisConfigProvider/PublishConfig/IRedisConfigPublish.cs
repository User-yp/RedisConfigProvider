using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedisConfigProvider.PublishConfig
{
    public interface IRedisConfigPublish
    {
        Task<bool> PublishAsync<T>(T TConfig);
        Task<bool> PublishAsync<T>(string key, T TConfig);
        Task<bool> PublishAsync(string key, string value);
        Task<bool> PublishAsync(Dictionary<string, ConcurrentQueue<string>> dictionary);
    }
}
