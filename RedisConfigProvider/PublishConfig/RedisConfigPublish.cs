using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using RedisConfigProvider.Operate;

namespace RedisConfigProvider.PublishConfig;

public class RedisConfigPublish : IRedisConfigPublish
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new ConcurrentDictionary<string, SemaphoreSlim>();
    private readonly Dictionary<string, ConcurrentQueue<string>> _keyValues = new Dictionary<string, ConcurrentQueue<string>>();

    private readonly ConnectionMultiplexer _conn;
    private readonly IDatabase _db;
    public RedisConfigPublish(IOptionsMonitor<RedisConfigOptions> options) : this(options.CurrentValue)
    {
    }
    public RedisConfigPublish(RedisConfigOptions options)
    {
        _conn = options.ConnectionMultiplexer();
        _db = _conn.GetDatabase(options.DbNumber);
    }

    public async Task<bool> PublishAsync<T>(string key, T TConfig)
    {
        string value = JsonConvert.SerializeObject(TConfig);

        return await PublishAsync(key, value);
    }
    public async Task<bool> PublishAsync<T>(T TConfig)
    {
        string key = TConfig!.GetType().Name;
        string value = JsonConvert.SerializeObject(TConfig);

        return await PublishAsync(key, value);
    }

    public async Task<bool> PublishAsync(Dictionary<string, ConcurrentQueue<string>> dictionary)
    {
        return await WriteReidsAsync(dictionary);
    }

    public async Task<bool> PublishAsync(string key, string value)
    {
        _keyValues.Add(key, value);
        var res = await WriteReidsAsync(_keyValues);
        return res;
    }
    private async Task<bool> WriteReidsAsync(Dictionary<string, ConcurrentQueue<string>> dictionary)
    {
        var tasks = new List<Task>();
        bool allSucceeded = true;

        foreach (var kvp in dictionary)
        {
            var key = kvp.Key;
            var queue = kvp.Value;

            // Start a new task for each key  
            tasks.Add(Task.Run(async () =>
            {
                // Get or create a semaphore for the current key  
                var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

                // Wait to enter the semaphore for this key  
                await semaphore.WaitAsync();
                try
                {
                    // Write all items in the queue to Redis  
                    while (queue.TryDequeue(out var value))
                    {
                        var res = await _db.StringSetAsync(key, value);
                        if (!res)
                        {
                            allSucceeded = false; // 如果有一个写入失败，标记为 false  
                        }
                    }
                }
                finally
                {
                    // Release the semaphore  
                    semaphore.Release();
                }
            }));
        }
        // Wait for all tasks to complete  
        await Task.WhenAll(tasks);
        return allSucceeded;
    }
    private async Task WriteReidsAsync(string key, string value)//string key, string value
    {
        SemaphoreSlim semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(); // 等待获取信号量  
        try
        {
            await _db.StringSetAsync(key, value); // 写入操作  
            Console.WriteLine($"键   {key}   写入  {value}");
            await Task.Delay(10);
        }
        finally
        {
            semaphore.Release(); // 释放信号量  
        }
    }
}