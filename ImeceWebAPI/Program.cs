using Application;
using Application.Services;
using Infrastructure;
using Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();