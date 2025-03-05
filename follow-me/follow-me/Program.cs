using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Настраиваем Kestrel для работы с HTTP (8080) и HTTPS (8081)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080); // HTTP
    options.ListenAnyIP(8081, listenOptions => listenOptions.UseHttps()); // HTTPS
});

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

// Настройка маршрутов
app.MapControllerRoute(
    name: "default",
    pattern: "admin/{controller=Admin}/{action=Index}/{id?}");

app.MapControllers();

app.Run();