using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Soltech.Samples.Fora.EdgarApi.Controllers;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Read the allowed CORS front-end URLs from application settings (or environment variables)
var corsOptions = builder.Configuration?.GetSection("CORS")?.Get<List<CorsOptions>>();

// Tell ASP.NET Core to add the service that handles CORS requests
builder.Services.AddCors(options =>
{
    options.AddPolicy("EdgarAPI",
        policy =>
        {
            policy.WithOrigins(corsOptions.ConvertAll<string>(c => c.Url).ToArray());
            //policy.WithOrigins("https://localhost:4200", "https://edgarclient.azurewebsites.net", "http://localhost:5286").AllowCredentials();
        });
});

// Tell ASP.NET Core to add the services that handle OpenAPI (Swagger) requests
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen((c) =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = $"EDGAR Company Funding API - {builder.Environment.EnvironmentName}",
        Description = "An API for providing a subset of EDGAR data from the Securities and Exchange Commission",
        Version = "v1",
        Contact = new OpenApiContact()
        {
            Name = EdgarController.DefaultAuthor.Name,
            Url = new Uri(EdgarController.DefaultAuthor.Website)
        }
    });
    c.DocInclusionPredicate((name, api) => api.HttpMethod != null);

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.UseCors("EdgarAPI");

app.UseStaticFiles();

app.MapControllers();

app.Run();

/// <summary>
/// Represents the custom CORS application configuration section in appSettings.json (or elsewhere)
/// </summary>
internal class CorsOptions
{
    public string Url { get; set; }
}


