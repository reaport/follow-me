using FollowMe.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Загрузка конфигурации из appsettings.json и переменных среды
var configuration = builder.Configuration;

// Логирование значений конфигурации
Console.WriteLine("UseStubs: " + configuration.GetValue<bool>("UseStubs"));
Console.WriteLine("GroundControlSettings:BaseUrl: " + configuration["GroundControlSettings:BaseUrl"]);
Console.WriteLine("OrchestratorSettings:BaseUrl: " + configuration["OrchestratorSettings:BaseUrl"]);

// Проверка конфигурации
var groundControlBaseUrl = configuration["GroundControlSettings:BaseUrl"];
if (string.IsNullOrEmpty(groundControlBaseUrl))
{
    throw new InvalidOperationException("GroundControlSettings:BaseUrl is not configured.");
}

var orchestratorBaseUrl = configuration["OrchestratorSettings:BaseUrl"];
if (string.IsNullOrEmpty(orchestratorBaseUrl))
{
    throw new InvalidOperationException("OrchestratorSettings:BaseUrl is not configured.");
}

// Настройка HttpClient для сервисов
if (configuration.GetValue<bool>("UseStubs"))
{
    // Используем заглушки
    builder.Services.AddScoped<IGroundControlService, GroundControlStubService>();
    builder.Services.AddScoped<IOrchestratorService, OrchestratorStubService>();
}
else
{
    // Используем реальные сервисы с BaseAddress из конфигурации
    builder.Services.AddHttpClient<IGroundControlService, GroundControlService>(client =>
    {
        client.BaseAddress = new Uri(groundControlBaseUrl);
    });

    builder.Services.AddHttpClient<IOrchestratorService, OrchestratorService>(client =>
    {
        client.BaseAddress = new Uri(orchestratorBaseUrl);
    });
}

// Настройка репозитория машин
builder.Services.AddSingleton<CarRepository>();

// Добавление контроллеров и представлений
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Настройка Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Follow Me API",
        Version = "v1",
        Description = "API для управления машинами сопровождения."
    });
});

// Настройка CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .WithExposedHeaders("Content-Disposition"); // Разрешение специальных заголовков
        });
});

var app = builder.Build();

// Включение Swagger в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Follow Me API v1");
    });
}

// Перенаправление с HTTP на HTTPS
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseDeveloperExceptionPage(); // Отображение ошибок в разработке
app.UseRouting();
app.UseCors("AllowAll");

app.MapControllers();

app.Run();