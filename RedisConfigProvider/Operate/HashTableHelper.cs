﻿using System.Collections.Concurrent;

namespace RedisConfigProvider.Operate;

public static class HashTableHelper
{
    /// <summary>
    /// 向哈希表中插入键值对，若不存在键则新建
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void Add(this Dictionary<string, ConcurrentQueue<string>> dictionary, string key, string value)
    {
        if (!dictionary.ContainsKey(key))
        {
            dictionary[key] = new ConcurrentQueue<string>();
        }
        dictionary[key].Enqueue(value);
    }

    /// <summary>
    /// 清空所有键值对
    /// </summary>
    /// <param name="dictionary"></param>    
    public static void Clear(this Dictionary<string, ConcurrentQueue<string>> dictionary) => dictionary.Clear();

    /// <summary>
    /// 移除指定键中的一个值,若值集合为空则移除键。
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns>返回移除后的表，若不包含值则返回原表。</returns>    
    public static Dictionary<string, ConcurrentQueue<string>> Remove(this Dictionary<string, ConcurrentQueue<string>> dictionary, string key, string value)
    {
        ConcurrentQueue<string> newQueue = new ConcurrentQueue<string>();
        if (dictionary.ContainsKey(key))
        {
            var removed = dictionary[key].FirstOrDefault(str => str == value);
            if (removed != null)
            {
                foreach (string s in dictionary[key])
                {
                    if (s != value)
                        newQueue.Enqueue(s);
                }
                dictionary[key] = newQueue;
            }
            if (dictionary[key].Count == 0)
            {
                dictionary.Remove(key);
            }
            return dictionary;
        }
        return dictionary;
    }

    /// <summary>
    /// 检查某个键是否存在
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <returns></returns>    
    public static bool ContainsKey(this Dictionary<string, ConcurrentQueue<string>> dictionary, string key)
    {
        return dictionary.ContainsKey(key);
    }
    /// <summary>
    /// 获取一个键对应的所有值不出队
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static List<string> GetValuesNoPop(this Dictionary<string, ConcurrentQueue<string>> dictionary, string key)
    {
        var list = new List<string>();
        if (dictionary.TryGetValue(key, out var values))
        {
            return values.ToList();
        }
        return new List<string>(); // 返回一个空集合  
    }
    /// <summary>
    /// 获取一个键对应的所有值出队
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static List<string> GetValuesPop(this Dictionary<string, ConcurrentQueue<string>> dictionary, string key)
    {
        var list = new List<string>();
        if (dictionary.TryGetValue(key, out var values))
        {
            for (int i = 0; i < values.Count; i++)
            {
                values.TryDequeue(out var result);
                list.Add(result);
            }
            return values.ToList();
        }
        if (dictionary[key].Count == 0)
            dictionary.Remove(key);
        return new List<string>(); // 返回一个空集合  
    }
    // 检查某个键中是否包含特定的值  
    public static bool ContainsValue(this Dictionary<string, ConcurrentQueue<string>> dictionary, string key, string value)
    {
        return dictionary.TryGetValue(key, out var values) && values.Contains(value);
    }
    /// <summary>
    /// 获取所有键的集合
    /// </summary>
    /// <param name="dictionary"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetAllKeys(this Dictionary<string, ConcurrentQueue<string>> dictionary) => dictionary.Keys;

    /// <summary>
    /// 获取所有值集合
    /// </summary>
    /// <param name="dictionary"></param>
    /// <returns></returns>
    public static IEnumerable<ConcurrentQueue<string>> GetAllValuesList(this Dictionary<string, ConcurrentQueue<string>> dictionary) => dictionary.Values.ToList();

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