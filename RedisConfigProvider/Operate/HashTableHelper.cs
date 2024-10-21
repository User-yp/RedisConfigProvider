using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RedisConfigProvider.Operate
{
    public static class HashTableHelper
    {
        /// <summary>
        /// 向哈希表中插入键值对，若不存在键则新建
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static Dictionary<string, ConcurrentQueue<string>> Add(this Dictionary<string, ConcurrentQueue<string>> dictionary, string key, string value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary[key] = new ConcurrentQueue<string>();
            }
            dictionary[key].Enqueue(value);
            return dictionary;
        }

        /// <summary>
        /// 向哈希表中插入键值对，若不存在键则新建
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static Dictionary<string, ConcurrentQueue<string>> AddRange(this Dictionary<string, ConcurrentQueue<string>> dictionary, string key, params string[] values)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary[key] = new ConcurrentQueue<string>();
            }
            foreach (var value in values)
            {
                dictionary[key].Enqueue(value);
            }
            return dictionary;
        }
        public static Dictionary<string, ConcurrentQueue<string>> AddRange(this Dictionary<string, ConcurrentQueue<string>> dictionary, string key, List<string> values)
            => dictionary.AddRange(key, values?.ToArray() ?? Array.Empty<string>());

        /// <summary>
        /// 清空所有键值对
        /// </summary>
        /// <param name="dictionary"></param>    
        public static void Clear(this Dictionary<string, ConcurrentQueue<string>> dictionary)
            => dictionary.Clear();

        /// <summary>
        /// 移除指定键中的一个值,若值集合为空则移除键。
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>返回移除后的表，若不包含值则返回原表。</returns>    
        public static Dictionary<string, ConcurrentQueue<string>> Remove(this Dictionary<string, ConcurrentQueue<string>> dictionary, string key, string value)
        {
            if (!dictionary.ContainsKey(key))
                throw new ArgumentException($"dictionary not contains key: {key}");

            if (!dictionary.TryGetValue(key, out var valueQueue))//原则上不允许key-nullValue的出现，但这里还是校验一下
                throw new ArgumentException($"Value queue for key '{key}' is null.");

            ConcurrentQueue<string> newQueue = new ConcurrentQueue<string>();
            var removed = valueQueue.FirstOrDefault(str => str == value);
            if (removed != null)
            {
                foreach (string s in dictionary[key])
                {
                    if (s != value)
                        newQueue.Enqueue(s);
                }
                if (newQueue.IsEmpty)
                    dictionary.Remove(key);
                else
                    dictionary[key] = newQueue;
            }
            return dictionary;
        }

        /// <summary>
        /// 移除指定键中的一个值,若值集合为空则移除键。
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>返回移除后的表，若不包含值则返回原表。</returns>    
        public static Dictionary<string, ConcurrentQueue<string>> RemoveRange(this Dictionary<string, ConcurrentQueue<string>> dictionary, string key, params string[] values)
        {
            // 检查字典是否包含指定的键  
            if (!dictionary.ContainsKey(key))
                throw new ArgumentException($"dictionary not contains key: {key}");

            // 获取当前队列  
            if (!dictionary.TryGetValue(key, out var valueQueue))
                throw new ArgumentException($"Value queue for key '{key}' is null.");

            // 检查所有待移除的值是否都存在于队列中  
            if (!values.All(value => valueQueue.Contains(value)))
                throw new ArgumentException("Not all values are present in the queue.");
            var newQueue = new ConcurrentQueue<string>();
            // 重新填充新队列，移除指定值  
            foreach (var s in valueQueue)
            {
                if (!values.Contains(s))
                    newQueue.Enqueue(s);
            }
            // 根据新队列更新字典  
            if (newQueue.IsEmpty)
                dictionary.Remove(key); // 如果新队列为空，移除键  
            else
                dictionary[key] = newQueue; // 否则更新字典中的队列  

            return dictionary;
        }
        public static Dictionary<string, ConcurrentQueue<string>> RemoveRange(this Dictionary<string, ConcurrentQueue<string>> dictionary, string key, List<string> values)
            => RemoveRange(dictionary, key, values?.ToArray() ?? Array.Empty<string>());

        /// <summary>
        /// 检查某个键是否存在
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>    
        public static bool ContainsKey(this Dictionary<string, ConcurrentQueue<string>> dictionary, string key)
            => dictionary.ContainsKey(key);

        /// <summary>
        /// 获取一个键对应的所有值不出队
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<string> GetValuesNoPop(this Dictionary<string, ConcurrentQueue<string>> dictionary, string key)
        {
            if (!dictionary.ContainsKey(key))
                throw new ArgumentException($"dictionary not contains key: {key}");

            if (dictionary.TryGetValue(key, out var values))
            {
                return values.ToList();
            }
            return new List<string>(); // 返回一个空集合  
        }

        /// <summary>
        /// 获取一个键对应的所有值出队,移出该键
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<string> GetValuesPop(this Dictionary<string, ConcurrentQueue<string>> dictionary, string key)
        {
            if (!dictionary.ContainsKey(key))
                throw new ArgumentException($"dictionary not contains key: {key}");

            var list = new List<string>();
            if (dictionary.TryGetValue(key, out var values))
            {
                list = values.ToList();
                values.Clear();
                dictionary.Remove(key);
                return list;
            }
            return list; // 返回一个空集合  
        }

        // 检查某个键中是否包含特定的值  
        public static bool ContainsValue(this Dictionary<string, ConcurrentQueue<string>> dictionary, string key, string value)
            => dictionary.TryGetValue(key, out var values) && values.Contains(value);

        /// <summary>
        /// 获取所有键的集合
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetAllKeys(this Dictionary<string, ConcurrentQueue<string>> dictionary)
            => dictionary.Keys;

        /// <summary>
        /// 获取所有值集合
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static IEnumerable<ConcurrentQueue<string>> GetAllValuesList(this Dictionary<string, ConcurrentQueue<string>> dictionary)
            => dictionary.Values.ToList();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ConvertToDictionary(this Dictionary<string, ConcurrentQueue<string>> dictionary)
        {
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            if (dictionary.Keys.Count == 0)
                return keyValues;
            foreach (var key in dictionary.Keys)
            {
                for (int i = 0; i < dictionary[key].Count; i++)
                {
                    dictionary[key].TryDequeue(out var result);
                    if (keyValues[key] != result)
                        keyValues.Add(key, result);
                }
            }
            return keyValues;
        }
    }
}
