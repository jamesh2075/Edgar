using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("EdgarAPI",
        policy =>
        {
            policy.WithOrigins("https://localhost:4200", "https://edgarclient.azurewebsites.net", "http://localhost:5286").AllowCredentials();
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen((c) =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = $"Edgar Company Funding API - {builder.Environment.EnvironmentName}", Description = "Serving You the Food You Love", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.UseCors("EdgarAPI");

app.MapControllers();

app.Run();


