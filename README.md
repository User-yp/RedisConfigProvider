# RedisConfigProvider

轻量化redis配置服务器，将配置信息同步到.net WebApplicationBuilder IConfiguration。

支持热加载，及时读取。

redis作为配置服务器，分布式写入读取。

批量操作时，不同key异步写入，相同key同步写入，确保顺序正确性。

#### 使用方法：

###### 1.安装nuget包

```
dotnet add package RedisConfigProvider
```

###### 2.在Program.cs中注册读取服务

```c#
var connStr = builder.Configuration.GetValue<string>("RedisConnStr");
builder.Host.ConfigureAppConfiguration((_, configBuilder) =>
{
    configBuilder.AddConfiguration(() => ConnectionMultiplexer.Connect(connStr), DbNumber: 1, reloadOnChange: true, ReloadInterval:TimeSpan.FromSeconds(3));
});
```

Redis连接字符串connStr，你也可以从环境变量里读取，或者直接写明，但不建议。

```c#
var connStr = Environment.GetEnvironmentVariable("RedisConnStr");
or
var connStr="127.0.0.1:6379";//不建议
```

DbNumber：Redis数据库中用来存储配置信息的库

reloadOnChange：是否开启热加载，开启后可在指定时间后加载更新的数据

reloadInterval:将Redis的配置信息同步到IConfiguration的时间间隔。

###### 3.注册发布配置服务

并不是所有项目否需要发布配置，大多数项目仅需要读取，因此分开注册服务。

```c#
builder.Services.AddRedisPublishService(options =>
{
    options.ConnectionMultiplexer = () => ConnectionMultiplexer.Connect(connStr);
    options.DbNumber = 1;
});
```

与上文一样，Redis连接字符串connStr和发布到指定的Redis库DbNumber

###### 4.配置信息的发布与读取

在构造函数里注入发布服务

```c#
public class TestController : ControllerBase
{
    private readonly IConfiguration configuration;
    private readonly IRedisConfigPublish publish;

    public TestController(IConfiguration configuration, IRedisConfigPublish publish)
    {
        this.configuration = configuration;
        this.publish = publish;
    }
}
```

添加一个测试方法，发布测试配置

```c#
[HttpPost]
public async Task<ActionResult<string>> AddConfigAsync()
{
    var testConfig = new TestConfig()
    {
        key = "test",
        Number = 1,
        value = "testValue"
    };
    var res = await publish.PublishAsync(testConfig);
    return Ok(res);
}
```

现在已将配置发布到Redis，若开启了热加载，在指定时间后新的配置即可同步到IConfiguration

写入并读取配置

```c#
[HttpPost]
public async Task<ActionResult<string>> AddConfigAsync()
{
    var testConfig = new TestConfig()
    {
        key = "test",
        Number = 1,
        value = "testValue"
    };

    var isSucess = await publish.PublishAsync(testConfig);

    await Task.Delay(3000);
    var getConfig= configuration.GetSection(nameof(TestConfig)).Get<TestConfig>();
    return Ok(getConfig);
}
```

读取结果：

```json
{
  "number": 1,
  "key": "test",
  "value": "testValue"
}
```

#### 特别声明：

PublishAsync方法有四个重载

```c#
Task<bool> PublishAsync<T>(T TConfig);
Task<bool> PublishAsync<T>(string key, T TConfig);
Task<bool> PublishAsync(string key, string value);
Task<bool> PublishAsync(Dictionary<string, ConcurrentQueue<string>> dictionary);
```

以下是他们的使用实例

```c#
[HttpPost]
public async Task<ActionResult<string>> AddConfigAsync()
{
    var testConfig = new TestConfig()
    {
        key = "test",
        Number = 1,
        value = "testValue"
    };
    var testConfigs = new Dictionary<string, ConcurrentQueue<string>>
    {
        { "key1", new ConcurrentQueue<string>(new[] { "value1-1", "value1-2" ,"value1-3", "value1-4" ,"value1-5", "value1-6" ,"value1-7", "value1-8" ,"value1-9", "value1-0" }) },
        { "key2", new ConcurrentQueue<string>(new[] { "value2-1", "value2-2" ,"value2-3", "value2-4","value2-5", "value2-6"}) },
        { "key3", new ConcurrentQueue<string>(new[] { "value3-1", "value3-2" ,"value3-3", "value3-4" ,"value3-5", "value3-6" }) } ,
        { "key4", new ConcurrentQueue<string>(new[] { "value4-1", "value4-2","value4-3", "value4-4"  }) }
    };

    var res = await publish.PublishAsync(testConfig);//方法一

    var res2 = await publish.PublishAsync("customizeKey", testConfigs);//方法二

    string value = JsonConvert.SerializeObject(testConfig);
    var res3 = await publish.PublishAsync("specifykey", value);//方法三

    var res4 = await publish.PublishAsync(testConfigs);//方法四

    return Ok(res);
}
```

1.Task<bool> PublishAsync<T>(T TConfig);

如果你只传入了一个配置对象，默认的存储规则是用类名命名key名，value为序列化对象的值。

2.Task<bool> PublishAsync<T>(string key, T TConfig);

你可以自定义key值，value为序列化对象的值。

3.Task<bool> PublishAsync(string key, string value);

自定义key与value的值，需要注意的是value需要是序列化的值，否则读取时可能出错。

4.Task<bool> PublishAsync(Dictionary<string, ConcurrentQueue<string>> dictionary);

批量传入配置信息，遵循不同key异步写入，相同key同步写入规则，确保顺序正确性。

#### 项目地址：

```
https://github.com/User-yp/RedisConfigProvider
```

希望能为你提供帮助，若有需要改进的地方，欢迎fork。
