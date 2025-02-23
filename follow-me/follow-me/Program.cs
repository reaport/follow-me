using Microsoft.OpenApi.Models;
using FollowMe.Services;

var builder = WebApplication.CreateBuilder(args);

// ��������� ������� � ���������
builder.Services.AddControllers();

// ��������� ��������� Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Follow Me API",
        Description = "API ��� ���������� �������� ������������� ���������. ��� ��������� ���������� �������� [������������](https://docs.google.com/document/d/1-A99pLnf-T3KJgUowspAIestsUUSzbDQ0Sfr5KvSmdI/edit?tab=t.xxby8r33la9d).",
        Version = "1.0.0"
    });

    // ��������� ����� ��� �������� � �������
    c.MapType<Guid>(() => new OpenApiSchema { Type = "string", Format = "uuid" });
    c.MapType<double>(() => new OpenApiSchema { Type = "number", Format = "double" });

    // ��������� ������� ������
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
});

// ������������ HttpClient ��� �������������� � ������� API
builder.Services.AddHttpClient<GroundControlService>(client =>
{
    client.BaseAddress = new Uri("https://ground-control.reaport.ru");
});

var app = builder.Build();

// ����������� �������� ��������� HTTP-��������
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Follow Me API v1");
        c.RoutePrefix = "swagger"; // ������������� Swagger UI �� ���� /swagger
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();