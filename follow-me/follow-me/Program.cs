using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using FollowMe.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы в контейнер
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

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

// Настройка маршрутов для MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Index}/{id?}");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();