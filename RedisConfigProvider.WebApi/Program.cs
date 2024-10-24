using RedisConfigProvider.Extensions;

var builder = WebApplication.CreateBuilder(args);

var connStr = builder.Configuration.GetValue<string>("RedisConnStr");
builder.Host.ConfigureAppConfiguration((_, configBuilder) =>
{
    configBuilder.AddRedisConfiguration(connStr, 1, false, TimeSpan.FromSeconds(3));
});
// Add services to the container.

builder.Services.AddRedisPublishService(connStr, 1);
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
