using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Controller tabanlý API yapýsý
builder.Services.AddControllers();

// Infrastructure katmanýndaki DbContext ve diðer servis kayýtlarý
builder.Services.AddInfrastructure(builder.Configuration);

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Development ortamýnda Swagger'ý aç
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();