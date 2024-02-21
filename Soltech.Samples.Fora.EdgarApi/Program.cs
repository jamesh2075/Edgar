using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json;

using Soltech.Samples.Fora.EdgarApi.Controllers;
using Soltech.Samples.Fora.EdgarData;

// Create an application builder so that additional services can be added
var builder = WebApplication.CreateBuilder(args);

// Tell ASP.NET Core to add the service that handles API routes
builder.Services.AddControllers();

// Read the allowed CORS front-end URLs from application settings (or environment variables)
var corsOptions = builder.Configuration?.GetSection("CORS")?.Get<List<CorsOptions>>();

// Tell ASP.NET Core to add the service that handles CORS requests
builder.Services.AddCors(options =>
{
    // Specify a policy that allows certain front-end URLs to access this service
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

var dataPath = $@"{builder.Environment.ContentRootPath}\Data.json";
using (StreamReader reader = new StreamReader(dataPath))
{
    var jsonData = reader.ReadToEnd();
    var edgarList = JsonSerializer.Deserialize<List<EdgarCompanyInfo>>(jsonData);
    if (edgarList is not null)
        builder.Services.AddSingleton(edgarList);
}

// Build the application with all the necessary services added
var app = builder.Build();

// This allows users to browse to the JSON-generated OpenAPI document
app.UseSwagger();

// This allows users to browse to the HTML-generated OpenAPI document
app.UseSwaggerUI();

// This applies the specified CORS policy globally to all API endpoints
app.UseCors("EdgarAPI");

// This allows users to browse to the Requirements document
app.UseStaticFiles();

// Use automatic API routing based on controller/action endpoints
app.MapControllers();

// Specify a custom endpoint that automatically redirect root requests to the OpenAPI page
app.MapGet("/", async http => http.Response.Redirect("/swagger"));

// Finally, run the application
app.Run();

/// <summary>
/// Represents the custom CORS application configuration section in appSettings.json (or elsewhere)
/// </summary>
internal class CorsOptions
{
    public string Url { get; set; }
}


