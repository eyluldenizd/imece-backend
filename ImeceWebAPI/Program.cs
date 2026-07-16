using Application;
using Application.Services;
using Application.Common.Storage;
using Infrastructure;
using Infrastructure.Repositories;
using ImeceWebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001",
                "http://127.0.0.1:3000",
                "http://127.0.0.1:3001")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

builder.Services.AddScoped<AnnouncementService>();

builder.Services.AddScoped<TodayInHistoryRepository>();
builder.Services.AddScoped<TodayInHistoryService>();

builder.Services.AddScoped<EmergencyNumberRepository>();
builder.Services.AddScoped<EmergencyNumberService>();

builder.Services.AddScoped<ServiceRouteRepository>();
builder.Services.AddScoped<ServiceRouteService>();
builder.Services.AddScoped<ECardRepository>();
builder.Services.AddScoped<ECardService>();



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddApplication()
    .AddInfrastructure();

var app = builder.Build();

app.UseMiddleware<ImeceWebAPI.Middleware.ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();