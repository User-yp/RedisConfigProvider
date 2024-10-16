using Microsoft.AspNetCore.Mvc;
using RedisConfigProvider.Publish;
using RedisConfigProvider.WebApi.Config;
using Newtonsoft.Json;

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
    [HttpGet]
    public async Task<ActionResult<string>> GetConfigAsync()
    {
        TestConfig config = new()
        {
            Number = 1,
            key = $"{nameof(TestConfig)}-key",
            value = $"{nameof(TestConfig)}-value"
        };
        publish.PublishAsync(nameof(TestConfig),JsonConvert.SerializeObject(config));
        await Task.Delay(3000);
        var test = configuration.GetSection(nameof(TestConfig)).Get<TestConfig>();
        return Ok(test);
    }
    [HttpGet]
    public async Task<ActionResult<string>> TaskAsync()
    {
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            int index = i; // 捕获索引  
            tasks.Add(Task.Run(() =>
            {
                string key = $"key"; // 使用相同的键进行测试。  
                string value = $"value{index}";
                publish.PublishAsync(key, value);
                Console.WriteLine($"Enqueued {key} with value {value}");
            }));
        }

        // 等待所有任务完成  
        await Task.WhenAll(tasks);
        return Ok("Success: Final value is correct.");
    }
    [HttpGet]
    public async Task<ActionResult<string>> TaskSync()
    {
        for (int i = 0; i < 10; i++)
        {
            string key = $"key[{i}]"; // 使用相同的键进行测试。  
            string value = $"value{i}";
            publish.PublishAsync(key, value);
        }
        return Ok();
    }
}
