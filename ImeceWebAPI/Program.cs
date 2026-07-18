using Application;
using ImeceWebAPI.Extensions;
using Infrastructure;

// WebApplicationFactory tabanlı integration testlerinin Program'a
// erişebilmesi için gerekli.

var builder = WebApplication.CreateBuilder(args);

// DI hatalarını (missing/circular dependency, captive dependency, geçersiz
// generic kayıt) mümkün olduğunca startup sırasında yakala.
builder.Host.UseDefaultServiceProvider((context, options) =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = context.HostingEnvironment.IsDevelopment();
});

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddImeceExceptionHandling(builder.Configuration);

builder.Services.AddImeceAuthentication(
    builder.Configuration,
    builder.Environment);

builder.Services.AddImeceApplication(
    builder.Configuration,
    builder.Environment,
    typeof(ApplicationAssemblyMarker).Assembly,
    typeof(InfrastructureAssemblyMarker).Assembly);

// Migration'sız schema senkronizasyon altyapısı (startup'ta Enabled ise çalışır).
builder.Services.AddImeceDatabase(
    builder.Configuration,
    builder.Environment);

var app = builder.Build();

app.UseImeceServiceRegistrationReport();

// Pipeline'ın en başında: sonraki tüm middleware'lerdeki hataları yakalar.
app.UseImeceExceptionHandling();

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

app.UseImeceAuthentication();

app.UseAuthorization();

app.MapControllers();

// Readiness sağlık kontrolü (secret içermez). "ready" etiketli kontroller.
app.MapHealthChecks("/health/ready");

// Şema senkronizasyonu: yalnızca DatabaseSchema:Enabled=true iken çalışır,
// request cancellation değil ApplicationStopping token kullanır.
await app.InitializeImeceDatabaseAsync();

// Production güvenli yapılandırma denetimi (Development davranışını bozmaz).
app.EnsureProductionSafety();

app.Run();

public partial class Program;
