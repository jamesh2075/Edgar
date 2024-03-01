// See https://aka.ms/new-console-template for more information
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

using Soltech.Samples.Fora.EdgarData;

// Retrieve the ASPNETCORE_ENVIRONMENT environment variable.
// In Visual Studio, this can be set in the Debug properties menu (or launchSettings.json file).
// In Windows, this can be set in System properties
// On the command line, this can be set using: set ASPNETCORE_ENVIRONMENT=[env]
// In Azure, this can be set on the Configuration tab of the web app
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

// If the ASPNECORE_ENVIRONMENT environment variable is not set, then set it to Development.
environment = (string.IsNullOrEmpty(environment) ? "Development" : environment);

Console.WriteLine($"This WebJob is running in the {environment} environment.");

// Create an IConfiguration instance for retrieving settings
// Different settings can exist in different environments (Development, Staging, Production, etc)
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

using var channel = new InMemoryChannel();

IServiceCollection services = new ServiceCollection();
services.Configure<TelemetryConfiguration>(config => config.TelemetryChannel = channel);

services.AddLogging(builder =>
{
    // Application Insights is registered as a logging provider
    var appInsightsConnectionString = config.GetConnectionString("ApplicationInsights");
    builder.AddApplicationInsights(
        configureTelemetryConfiguration: (config) => config.ConnectionString = appInsightsConnectionString,
        configureApplicationInsightsLoggerOptions: (options) => { }
    );

    // The Terminal window is registered as a logging provider
    // A custom formatter is specified to control the color of the log messages
    builder.AddConfiguration(config.GetSection("Logging"));
    // builder.Services.Configure<ConsoleColorOptions>(config.GetSection("Logging:Console:FormatterOptions"));
    builder.AddConsole(options => options.FormatterName = "CustomColorFormatter")
            .AddConsoleFormatter<CustomColorFormatter, ConsoleColorOptions>(options =>
            {
                // These are read from the configuration file

                //options.ErrorColor = ConsoleColor.Red;
                //options.WarningColor = ConsoleColor.DarkYellow;
                //options.InformationColor = options.DefaultColor;
            });
});

IServiceProvider serviceProvider = services.BuildServiceProvider();
ILogger<Program> logger = serviceProvider.GetRequiredService<ILogger<Program>>();

using HttpClient sharedClient = new(){};

// This is required to prevent authentication errors, according to the Requirements documentation
sharedClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
sharedClient.DefaultRequestHeaders.UserAgent.ParseAdd("PostmanRuntime/7.34.0");

const string baseUrl = "https://data.sec.gov/api/xbrl/companyfacts";

async Task DownloadFiles()
{
    var cikFile = $@"{Environment.CurrentDirectory}\CIKs.txt";

    using (StreamReader sr = new StreamReader(cikFile))
    {
        List<EdgarCompanyInfo> list = new List<EdgarCompanyInfo>();

        string data = sr.ReadToEnd();
        string[] ciks = data.Replace("\r","").Replace("\n","").Split(",");
        string message = $"Processing {ciks.Length} CIKs.";
        logger.LogInformation(message);
        foreach (string? item in ciks)
        {
            if (string.IsNullOrEmpty(item)) continue;

            if (!Int32.TryParse(item, out int number))
            {
                logger.LogError($"{item} could not be parsed into a number!");
                continue;
            }

            // Left-pad the number with zeroes (0000012345) to create a valid CIK
            string cik = number.ToString("D10");

            var edgarApiEndpoint = $"{baseUrl}/CIK{cik}.json";

            try
            {
                using var response = await sharedClient.GetAsync(edgarApiEndpoint);
                if (!response.IsSuccessStatusCode)
                {
                    message = $"{item,10} was not found!";
                    logger.LogWarning(message);
                    continue;
                }

                var info = await response.Content.ReadFromJsonAsync<EdgarCompanyInfo?>();
                if (info != null)
                {
                    list.Add(info);
                }
                else
                {
                    message = $"{item,10} - Could not parse JSON data correctly!";
                    logger.LogError(message);
                    logger.LogError(response.Content.ToString());
                }

                message = $"{item,10} {info?.EntityName}";
                logger.LogInformation(message);
            }
            catch (Exception ex)
            {
                message = $"{item,10} - An error occurred retrieving JSON data!";
                logger.LogError(ex, message);
            }
        }

        var json = JsonSerializer.Serialize(list);

        var outputDirs = config?.GetSection("DataOutputDirectories")?.Get<List<OutputDirectory>>();
        outputDirs?.ToList().ForEach(d =>
        {
            var path = Path.Combine(d.Directory, "Data.json");
            try
            {
                File.WriteAllText(path, json);
                logger.LogInformation($"Successfully saved data to {path}");
            }
            catch(DirectoryNotFoundException ex)
            {
                logger.LogError(ex, $"There was an error saving data to {path}");
            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        });
    }
}

try
{
    Console.WriteLine("Downloading the Edgar CIKs...");

    await DownloadFiles();
    
    Console.WriteLine("Downloading the Edgar CIKs...Done!");
}
finally
{
    // Explicitly call Flush() followed by Delay, as required in console apps.
    // This ensures that even if the application terminates, telemetry is sent to the back end.
    channel.Flush();

    await Task.Delay(TimeSpan.FromMilliseconds(1000));
}

class OutputDirectory
{
    public string Directory { get; set; } = "";
}

class CustomColorFormatter : ConsoleFormatter
{
    private readonly ConsoleColorOptions customOptions;

    public CustomColorFormatter(IOptionsMonitor<ConsoleColorOptions> options) : base("CustomColorFormatter")
    {
        customOptions = options.CurrentValue;
    }
    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        // Determine which color to log the message based on the severity of the message (LogLevel)
        var color = logEntry.LogLevel switch
        {
            LogLevel.Error or LogLevel.Critical => customOptions.ErrorColor,
            LogLevel.Warning => customOptions.WarningColor,
            _ => customOptions.DefaultColor
        };

        Console.ForegroundColor = color;

        string? message =
            logEntry.Formatter?.Invoke(
                logEntry.State, logEntry.Exception);

        if (message is not null)
            textWriter.WriteLine(message);
    }
}

public sealed class ConsoleColorOptions : ConsoleFormatterOptions
{
    public ConsoleColor DefaultColor { get; set; } = Console.ForegroundColor;
    public ConsoleColor ErrorColor { get; set; } = Console.ForegroundColor;
    public ConsoleColor WarningColor { get; set; } = Console.ForegroundColor;
    public ConsoleColor InformationColor { get; set; } = Console.ForegroundColor;
}