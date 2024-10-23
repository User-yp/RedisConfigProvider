using Microsoft.Extensions.Configuration;
using RedisConfigProvider.Operate;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;

namespace RedisConfigProvider
{
    public class RedisConfigProvider : ConfigurationProvider, IDisposable
    {
        private readonly RedisConfigOptions options;
        private readonly ReaderWriterLockSlim lockObj = new ReaderWriterLockSlim();
        private bool isDisposed = false;
        public RedisConfigProvider(RedisConfigOptions options)
        {
            this.options = options;
            TimeSpan interval = new TimeSpan();
            interval = options.ReloadInterval != null ? options.ReloadInterval.Value : TimeSpan.FromSeconds(3);
            if (options.ReloadOnChange)
            {
                ThreadPool.QueueUserWorkItem(obj => {
                    while (!isDisposed)
                    {
                        Load();
                        Thread.Sleep(interval);
                    }
                });
            }
        }
        public void Dispose()
        {
            isDisposed = true;
        }
        public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            lockObj.EnterReadLock();
            try
            {
                return base.GetChildKeys(earlierKeys, parentPath);
            }
            finally
            {
                lockObj.ExitReadLock();
            }
        }
        public override bool TryGet(string key, out string value)
        {
            lockObj.EnterReadLock();
            try
            {
                return base.TryGet(key, out value);
            }
            finally
            {
                lockObj.ExitReadLock();
            }
        }
        public override void Load()
        {
            base.Load();
            IDictionary<string, string> clonedData = null;
            try
            {
                lockObj.EnterWriteLock();
                clonedData = Data.Clone();
                int dbNumber = options.DbNumber;
                IDatabase database = null;
                Data.Clear();
                using (var conn = options.ConnectionMultiplexer)
                {
                    database = conn.GetDatabase(dbNumber);
                    DoLoad(database);
                }
            }
            catch (DbException)
            {
                //if DbException is thrown, restore to the original data.
                this.Data = clonedData;
                throw;
            }
            finally
            {
                lockObj.ExitWriteLock();
            }
            //OnReload cannot be between EnterWriteLock and ExitWriteLock, or "A read lock may not be acquired with the write lock held in this mode" will be thrown.
            if (DictionaryHelper.IsChanged(clonedData, Data))
            {
                OnReload();
            }
        }

        private void DoLoad(IDatabase redisDb)
        {
            // 获取 Redis 数据库中的所有键  
            var server = redisDb.Multiplexer.GetServer(redisDb.Multiplexer.Configuration);
            var keys = server.Keys(redisDb.Database);

            foreach (var key in keys)
            {
                // 获取键名（去掉前缀或根据实际需求处理）  
                string name = key.ToString();

                string value = redisDb.StringGet(key); // 从 Redis 获取值  

                if (string.IsNullOrEmpty(value))
                {
                    this.Data[name] = null; // 如果值为空，设置为 null  
                    continue;
                }

                value = value.ToString().Trim();

                // 判断值是否像 [...] 或 {}，可能是 json 数组值或 json 对象值  
                if ((value.StartsWith('[') && value.EndsWith(']')) || (value.StartsWith('{') && value.EndsWith('}')))
                {
                    TryLoadAsJson(name, value);
                }
                else
                {
                    this.Data[name] = value; // 直接存储其他值  
                }
            }
        }

        private void TryLoadAsJson(string name, string value)
        {
            var jsonOptions = new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
            try
            {
                var jsonRoot = JsonDocument.Parse(value, jsonOptions).RootElement;
                LoadJsonElement(name, jsonRoot);
            }
            catch (JsonException ex)
            {
                //if it is not valid json, parse it as plain string value
                this.Data[name] = value;
                Debug.WriteLine($"When trying to parse {value} as json object, exception was thrown. {ex}");
            }
        }

        private void LoadJsonElement(string name, JsonElement jsonRoot)
        {
            if (jsonRoot.ValueKind == JsonValueKind.Array)
            {
                int index = 0;
                foreach (var item in jsonRoot.EnumerateArray())
                {
                    //https://andrewlock.net/creating-a-custom-iconfigurationprovider-in-asp-net-core-to-parse-yaml/
                    //parse as "a:b:0"="hello";"a:b:1"="world"
                    string path = name + ConfigurationPath.KeyDelimiter + index;
                    LoadJsonElement(path, item);
                    index++;
                }
            }
            else if (jsonRoot.ValueKind == JsonValueKind.Object)
            {
                foreach (var jsonObj in jsonRoot.EnumerateObject())
                {
                    string pathOfObj = name + ConfigurationPath.KeyDelimiter + jsonObj.Name;
                    LoadJsonElement(pathOfObj, jsonObj.Value);
                }
            }
            else
            {
                //if it is not json array or object, parse it as plain string value
                this.Data[name] = jsonRoot.GetValueForConfig();
            }
        }
    }
}
