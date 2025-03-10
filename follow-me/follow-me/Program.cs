using FollowMe.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ����������� Kestrel ��� ������ � HTTP (8080) � HTTPS (8081)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080); // HTTP
});

// ��������� ������������ �� appsettings.json
var configuration = builder.Configuration;

// ����������� HttpClient ��� ��������
// if (configuration.GetValue<bool>("UseStubs"))
// {
    // ���������� ���-����������
    builder.Services.AddScoped<IGroundControlService, GroundControlStubService>();
    builder.Services.AddScoped<IOrchestratorService, OrchestratorStubService>();
// }
// else
// {
//     // ���������� �������� ���������� � BaseAddress �� ������������
//     builder.Services.AddHttpClient<IGroundControlService, GroundControlService>(client =>
//     {
//         client.BaseAddress = new Uri(configuration["GroundControlSettings:BaseUrl"]);
//     });

//     builder.Services.AddHttpClient<IOrchestratorService, OrchestratorService>(client =>
//     {
//         client.BaseAddress = new Uri(configuration["OrchestratorSettings:BaseUrl"]);
//     });
// }

// ����������� ������ ��������
builder.Services.AddSingleton<CarRepository>();

// ��������� ������� � ���������
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ����������� Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Follow Me API",
        Version = "v1",
        Description = "API ��� ���������� �������� ������������� ���������."
    });
});

// ����������� CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .WithExposedHeaders("Content-Disposition"); // ��������� ���������� ������
        });
});

var app = builder.Build();

// �������� Swagger ������ � ������ ����������
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Follow Me API v1");
    });
}

// ��������� �������� � HTTP �� HTTPS
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseDeveloperExceptionPage(); // ���������� ������ � ����������
app.UseRouting();
app.UseCors("AllowAll");

app.MapControllers();

app.Run();