// See https://aka.ms/new-console-template for more information
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Soltech.Samples.Fora.EdgarData;
using System.Text.Json;
using System.Text.Json.Serialization;

Console.WriteLine("Downloading the Edgar CIKs...");

HttpClient sharedClient = new(){};

var request = new HttpRequestMessage()
{
    Method = HttpMethod.Get,
};

request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

sharedClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
sharedClient.DefaultRequestHeaders.UserAgent.ParseAdd("PostmanRuntime/7.34.0");

const string baseUrl = "https://data.sec.gov/api/xbrl/companyfacts";
const string edgarApiEndpointFormat = "/CIK{0}.json";

ConsoleColor defaultColor = Console.ForegroundColor;
ConsoleColor errorColor = ConsoleColor.Red;

async Task DownloadFiles(string format)
{
    var cikFile = $@"{Environment.CurrentDirectory}\CIKs.txt";

    using (StreamReader sr = new StreamReader(cikFile))
    {
        List<EdgarCompanyInfo> list = new List<EdgarCompanyInfo>();

        string columns = sr.ReadLine();
        while (columns != null)
        {
            string[] ciks = columns.Split(",");
            foreach (var item in ciks)
            {
                if (string.IsNullOrEmpty(item)) continue;

                string cik = Int32.Parse(item).ToString("D10");
                string edgarApiEndpoint = string.Format(edgarApiEndpointFormat, cik);
                edgarApiEndpoint = $"{baseUrl}{edgarApiEndpoint}";

                var response = await sharedClient.GetAsync(edgarApiEndpoint);
                if (!response.IsSuccessStatusCode)
                {
                    // TODO: Log the item in error

                    Console.ForegroundColor = errorColor;
                    Console.WriteLine($"{item,10} was not found!");
                    continue;
                }

                
                var info = await response.Content.ReadFromJsonAsync<EdgarCompanyInfo?>();

                if (info != null)
                {
                    list.Add(info);
                }

                Console.ForegroundColor = defaultColor;
                Console.WriteLine($"{item,10} {info?.EntityName}");
            }

            columns = sr.ReadLine();
        }
        

        var json = JsonSerializer.Serialize(list);
        File.WriteAllText($@"{Environment.CurrentDirectory}\Data.json", json);

        // After this service runs, it copies the file to the EDGAR API.
        // Ideally, it should reside in some type of shared data store or folder

        // But for now, either manually copy it or let a WebJob copy it
        // az webapp deploy --resource-group <group-name> --name <app-name> --src-path Data.json --type=static
    }


}

await DownloadFiles(edgarApiEndpointFormat);

Console.WriteLine("Downloading the Edgar CIKs...Done!");
