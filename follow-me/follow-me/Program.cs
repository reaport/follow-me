using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ����������� Kestrel ��� ������ � HTTP (8080) � HTTPS (8081)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080); // HTTP
    options.ListenAnyIP(8081, listenOptions => listenOptions.UseHttps()); // HTTPS
});

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

// ��������� ���������
app.MapControllerRoute(
    name: "default",
    pattern: "admin/{controller=Admin}/{action=Index}/{id?}");

app.MapControllers();

app.Run();