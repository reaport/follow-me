using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using FollowMe.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы в контейнер
builder.Services.AddControllers();

// Добавляем Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Follow Me API",
        Version = "v1",
        Description = "API для управления машинами сопровождения самолетов."
    });
});

// Загружаем конфигурацию
var useStubs = builder.Configuration.GetValue<bool>("UseStubs");

// Регистрируем сервис в зависимости от конфигурации
if (useStubs)
{
    builder.Services.AddScoped<IGroundControlService, GroundControlStubService>();
    builder.Services.AddScoped<IOrchestratorService, OrchestratorStubService>();
}
else
{
    builder.Services.AddHttpClient<IGroundControlService, GroundControlService>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["GroundControlSettings:BaseUrl"]);
    });

    builder.Services.AddHttpClient<IOrchestratorService, OrchestratorService>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["OrchestratorSettings:BaseUrl"]);
    });
}

var app = builder.Build();

// Настраиваем конвейер обработки HTTP-запросов
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Follow Me API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();