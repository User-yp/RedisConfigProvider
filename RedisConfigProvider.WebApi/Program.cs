using RedisConfigProvider.Extensions;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var connStr = builder.Configuration.GetValue<string>("RedisConnStr");
builder.Host.ConfigureAppConfiguration((_, configBuilder) =>
{
    configBuilder.AddConfiguration(() => ConnectionMultiplexer.Connect(connStr), 1, true, TimeSpan.FromSeconds(3));
});
// Add services to the container.

builder.Services.AddRedisPublishService(options =>
{
    options.ConnectionMultiplexer = () => ConnectionMultiplexer.Connect(connStr);
    options.DbNumber = 1;
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
