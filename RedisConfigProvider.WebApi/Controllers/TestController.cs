using Microsoft.AspNetCore.Mvc;
using RedisConfigProvider.PublishConfig;
using RedisConfigProvider.Operate;
using System.Collections.Concurrent;


namespace RedisConfigProvider.WebApi.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly IConfiguration configuration;
    private readonly IRedisConfigPublish publish;

    public TestController(IConfiguration configuration, IRedisConfigPublish publish)
    {
        this.configuration = configuration;
        this.publish = publish;
    }
    [HttpPost]
    public async Task<ActionResult<string>> AddConfigAsync()
    {
        var keyValues = new Dictionary<string, ConcurrentQueue<string>>
        {
            { "key1", new ConcurrentQueue<string>(new[] { "value1-1", "value1-2" ,"value1-3", "value1-4" ,"value1-5", "value1-6" ,"value1-7", "value1-8" ,"value1-9", "value1-0" }) },
            { "key2", new ConcurrentQueue<string>(new[] { "value2-1", "value2-2" ,"value2-3", "value2-4","value2-5", "value2-6"}) },
            { "key3", new ConcurrentQueue<string>(new[] { "value3-1", "value3-2" ,"value3-3", "value3-4" ,"value3-5", "value3-6" }) } ,
            { "key4", new ConcurrentQueue<string>(new[] { "value4-1", "value4-2","value4-3", "value4-4"  }) }
        };

        var res = await publish.PublishAsync(keyValues);
        return Ok(res);
    }
    [HttpGet]
    public async Task<ActionResult<Dictionary<string, ConcurrentQueue<string>>>> GetConfigAsync()
    {
        var keyValues = new Dictionary<string, ConcurrentQueue<string>>
        {
            { "key1", new ConcurrentQueue<string>(new[] { "value1-1", "value1-2" ,"value1-3", "value1-4" ,"value1-5", "value1-6" ,"value1-7", "value1-8" ,"value1-9", "value1-0" }) },
            { "key2", new ConcurrentQueue<string>(new[] { "value2-1", "value2-2" ,"value2-3", "value2-4","value2-5", "value2-6"}) },
            { "key3", new ConcurrentQueue<string>(new[] { "value3-1", "value3-2" ,"value3-3", "value3-4" ,"value3-5", "value3-6" }) } ,
            { "key4", new ConcurrentQueue<string>(new[] { "value4-1", "value4-2","value4-3", "value4-4"  }) }
        };

        //var res = keyValues.ConvertToDictionary();
        keyValues.Remove("key1", "value1-2");
        keyValues.Remove("key4", "value4-1");
        return Ok(keyValues);
    }

    [HttpPost]
    public async Task<ActionResult<string>> TaskAsync()
    {
        bool res = true;
        for (int i = 0; i < 100; i++)
        {
            string key = $"key"; // 使用相同的键进行测试。  
            string value = $"value{i}";

            var isSuss = await publish.PublishAsync(key, value);
            if (!isSuss)
            {
                res = false;
            }
        }
        return Ok(res);
    }
}
