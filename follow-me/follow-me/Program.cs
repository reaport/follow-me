using FollowMe.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Настраиваем Kestrel для работы с HTTP (8080) и HTTPS (8081)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080); // HTTP
    options.ListenAnyIP(8081, listenOptions => listenOptions.UseHttps()); // HTTPS
});

// Загружаем конфигурацию из appsettings.json
var configuration = builder.Configuration;

// Регистрация HttpClient для сервисов
if (configuration.GetValue<bool>("UseStubs"))
{
    // Используем мок-реализации
    builder.Services.AddScoped<IGroundControlService, GroundControlStubService>();
    builder.Services.AddScoped<IOrchestratorService, OrchestratorStubService>();
}
else
{
    // Используем реальные реализации с BaseAddress из конфигурации
    builder.Services.AddHttpClient<IGroundControlService, GroundControlService>(client =>
    {
        client.BaseAddress = new Uri(configuration["GroundControlSettings:BaseUrl"]);
    });

    builder.Services.AddHttpClient<IOrchestratorService, OrchestratorService>(client =>
    {
        client.BaseAddress = new Uri(configuration["OrchestratorSettings:BaseUrl"]);
    });
}

// Регистрация других сервисов
builder.Services.AddSingleton<CarRepository>();

// Добавляем сервисы в контейнер
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Настраиваем Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Follow Me API",
        Version = "v1",
        Description = "API для управления машинами сопровождения самолетов."
    });
});

// Настраиваем CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .WithExposedHeaders("Content-Disposition"); // Разрешает скачивание файлов
        });
});

var app = builder.Build();

// Включаем Swagger только в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Follow Me API v1");
    });
}

// ОТКЛЮЧАЕМ редирект с HTTP на HTTPS
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseDeveloperExceptionPage(); // Показывает ошибки в разработке
app.UseRouting();
app.UseCors("AllowAll");

app.MapControllers();

app.Run();