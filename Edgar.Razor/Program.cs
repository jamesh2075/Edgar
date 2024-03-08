using Edgar.Razor;
using Microsoft.AspNetCore.Cors.Infrastructure;
using System.Net;
using System.Net.Http;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure the correct base API URL (Development, Stating, or Production)
var webApiBaseUrl = builder.Configuration?.GetValue<string>(Constants.WebApiUrl) ?? "";

HttpClient client = new HttpClient();
client.BaseAddress = new Uri(webApiBaseUrl);

var httpBuilder = builder.Services.AddHttpClient(Constants.EdgarApiHttpClient, (c) => {
    c.BaseAddress = new Uri(webApiBaseUrl); 
});

// Retrieve these settings from the Web API
string author = await client.GetStringAsync("/api/edgar/author");
string website = await client.GetStringAsync("/api/edgar/website");
string repo = await client.GetStringAsync("/api/edgar/repo");
string pipeline = await client.GetStringAsync("/api/edgar/pipeline");
string aspnetVersion = await client.GetStringAsync("/api/edgar/aspnetVersion");

// These settings are static for now
var rawDataUrl = $"{webApiBaseUrl}/api/edgar/json";
var requirementsUrl = $"{webApiBaseUrl}/Fora Coding Challenge v1.1.pdf";
var swaggerUrl = $"{webApiBaseUrl}/swagger";

WebApiProjectSettings apiSettings = new WebApiProjectSettings()
{
    Author = author,
    Bio = website,
    Repository = repo,
    Pipeline = pipeline,
    RawDataUrl = rawDataUrl,
    RequirementsUrl = requirementsUrl,
    SwaggerUrl = swaggerUrl,
    AspNetVersion = aspnetVersion,
};
builder.Services.AddSingleton<WebApiProjectSettings>(apiSettings);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

internal static class Constants
{
    public static string EdgarApiHttpClient = "EdgarApiHttpClient";
    public static string WebApiUrl = "WebApiUrl";
    public static string CompaniesEndpoint = "/api/edgar/Companies";

}

public record Company
{
    /// <summary>
    /// The name of the company
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// The Central Index Key (CIK) of the company 
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// The calculated standard fundable amount
    /// </summary>
    public decimal StandardFundableAmount { get; set; }
    /// <summary>
    /// The calculated special fundable amount
    /// </summary>
    public decimal SpecialFundableAmount { get; set; }
}