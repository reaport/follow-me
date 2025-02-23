using Microsoft.OpenApi.Models;
using FollowMe.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы в контейнер
builder.Services.AddControllers();

// Добавляем поддержку Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Follow Me API",
        Description = "API для управления машинами сопровождения самолетов. Для подробной информации смотрите [документацию](https://docs.google.com/document/d/1-A99pLnf-T3KJgUowspAIestsUUSzbDQ0Sfr5KvSmdI/edit?tab=t.xxby8r33la9d).",
        Version = "1.0.0"
    });

    // Добавляем схемы для запросов и ответов
    c.MapType<Guid>(() => new OpenApiSchema { Type = "string", Format = "uuid" });
    c.MapType<double>(() => new OpenApiSchema { Type = "number", Format = "double" });

    // Добавляем примеры ошибок
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
});

// Регистрируем HttpClient для взаимодействия с внешним API
builder.Services.AddHttpClient<GroundControlService>(client =>
{
    client.BaseAddress = new Uri("https://ground-control.reaport.ru");
});

var app = builder.Build();

// Настраиваем конвейер обработки HTTP-запросов
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Follow Me API v1");
        c.RoutePrefix = "swagger"; // Устанавливаем Swagger UI по пути /swagger
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();