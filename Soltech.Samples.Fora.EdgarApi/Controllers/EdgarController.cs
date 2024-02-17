using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Soltech.Samples.Fora.EdgarApi;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Configuration;
using Soltech.Samples.Fora.EdgarData;

namespace Soltech.Samples.Fora.EdgarApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EdgarController : ControllerBase
    {
        private IWebHostEnvironment environment;
        internal AuthorOptions? Author;
        internal static readonly AuthorOptions DefaultAuthor = new AuthorOptions
        {
            Name = "James Henry",
            Website = "https://www.linkedin.com/in/james-h-2459a92",
            Repository = ""

        };

        // All the company data, even non-10K data and those that do not follow the CYyyyy format
        private List<EdgarCompanyInfo> edgarList = new List<EdgarCompanyInfo>();

        // Filtered company data: Only 10K data and those that follow the CYyyyy format
        private List<EdgarCompanyInfo> filteredEdgarList = new List<EdgarCompanyInfo>();

        // Desired Response Format
        private List<Company> companyList = new List<Company>();

        // Constants (Ideally, these should be placed in a configuration setting)
        const decimal highIncomePercentage = .1233m; // 12.33 percent for income 10B or greater
        const decimal lowIncomePercentage = .215m; // 21.5 percent for income less than 10B
        const decimal companyVowelPercentage = .15m; // 15 percent for companies whose name start with a vowel
        const decimal decreasedIncomePercentage = .25m; // 25 percent for companies whose 2022 income was less than 2021 income
        
        /// <summary>
        /// Contains actions that return EDGAR data provided by the SEC
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="config"></param>
        public EdgarController(IWebHostEnvironment environment, IConfiguration config)
        {
            this.environment = environment;

            Author = config?.GetSection("Author")?.Get<AuthorOptions>();

            var dataPath = $@"{environment.ContentRootPath}\Data.json";

            // TODO: Get path to the Data.json file
            using (StreamReader reader = new StreamReader(dataPath))
            {
                var jsonData = reader.ReadToEnd();
                edgarList = JsonSerializer.Deserialize<List<EdgarCompanyInfo>>(jsonData);
            }

            filteredEdgarList = new List<EdgarCompanyInfo>(edgarList);

            filteredEdgarList.ForEach(c =>
            {
                var onlyValidData = c.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd?.Where(u => u.Form?.ToLower() == "10-k")
                    .Where(u => Regex.IsMatch(u.Frame, @"^CY\d{4}$")).ToList();

                if (c.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd != null && onlyValidData != null)
                    c.Facts.UsGaap.NetIncomeLoss.Units.Usd = onlyValidData.ToArray();
            });

            companyList = filteredEdgarList.ConvertAll<Company>((edgar) =>
            {

                decimal standardFundableAmount = 0;
                decimal specialFundableAmount = 0;

                // How to calculate Standard Fundable Amount:
                // Bullet Point 1
                // Company must have income data for all years between (and including) 2018 and 2022.
                // If they did not, their Standard Fundable Amount is $0.
                var anyFirstYear = edgar.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd?.Where(u => int.Parse(u.Frame.Substring(2)) >= 2018).Any();
                var anyLastYear = edgar.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd?.Where(u => int.Parse(u.Frame.Substring(2)) <= 2022).Any();
                bool firstYearValid = false;
                bool lastYearValid = false;

                if (anyFirstYear ?? false)
                {
                    var min = edgar.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd?.Min(u => int.Parse(u.Frame.Substring(2)));
                    firstYearValid = min <= 2018;
                }
                if (anyLastYear ?? false)
                {
                    var max = edgar.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd?.Max(u => int.Parse(u.Frame.Substring(2)));
                    lastYearValid = max >= 2022;
                }
                var validIncome = firstYearValid && lastYearValid;


                // Bullet Point 2
                // Company must have had positive income in both 2021 and 2022:
                // If they did not, their Standard Fundable Amount is $0..
                var income2021 = edgar.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd?.Where(u => int.Parse(u.Frame.Substring(2)) == 2021).FirstOrDefault();
                var income2022 = edgar.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd?.Where(u => int.Parse(u.Frame.Substring(2)) == 2022).FirstOrDefault();

                var valid2021Income = edgar.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd?.Where(u => int.Parse(u.Frame.Substring(2)) == 2021 && u.Val > 0).Any() ?? false;
                var valid2022Income = edgar.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd?.Where(u => int.Parse(u.Frame.Substring(2)) == 2022 && u.Val > 0).Any() ?? false;
                validIncome &= valid2021Income && valid2022Income;


                // Bullet Point 3
                // Using highest income between 2018 and 2022:
                // If income is greater than or equal to $10B, standard fundable amount is 12.33%
                // of income.
                // If income is less than $10B, standard fundable amount is 21.51 % of income.
                if (validIncome)
                {
                    var list = from u in edgar.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd
                               let year = int.Parse(u.Frame.Substring(2))
                               where year >= 2018 && year <= 2022
                               select u;

                    decimal highestIncome = list.Max(u => u.Val);
                    if (highestIncome >= 10000000000)
                        standardFundableAmount = highIncomePercentage * highestIncome;
                    else
                        standardFundableAmount = lowIncomePercentage * highestIncome;
                }

                // How to calculate the Special Fundable Amount:
                // Initially, the Special Fundable Amount is the same as Standard Fundable Amount.
                // If the company name starts with a vowel, add 15% to the standard funding amount.
                // If the company’s 2022 income was less than their 2021 income, subtract 25% from their
                // standard funding amount.

                if (validIncome)
                {
                    specialFundableAmount = standardFundableAmount;
                    var companyName = String.IsNullOrEmpty(edgar.EntityName) ? " " : edgar.EntityName;
                    var vowels = new char[] { 'A', 'E', 'I', 'O', 'U' }; // What about sometimes Y :)
                    if (vowels.Contains(companyName[0]))
                    {
                        specialFundableAmount = (companyVowelPercentage * standardFundableAmount) + standardFundableAmount;
                    }
                    else if (income2022?.Val < income2021?.Val)
                    {
                        specialFundableAmount = standardFundableAmount - (decreasedIncomePercentage * standardFundableAmount);
                    }
                }

                var company = new Company();
                company.Name = edgar.EntityName;
                company.Id = edgar.Cik;
                company.StandardFundableAmount = standardFundableAmount;
                company.SpecialFundableAmount = specialFundableAmount;

                return company;
            });
        }

        /// <summary>
        /// Returns a JavaScript Object Notation (JSON) document representing ALL
        /// data retrieved from the SEC for the CIKs listed in the requirements document
        /// </summary>
        /// <returns>A JSON document</returns>
        [HttpGet]
        [Produces("application/json")]
        [Route("json")]
        public JsonResult GetJson()
        {
            return new JsonResult(edgarList);
        }

        /// <summary>
        /// Returns all companies that have valid CY years and 10-K data
        /// </summary>
        /// <returns>All valid companies</returns>
        [HttpGet("GetAllCompanies")]
        [Route("Companies")]
        public IEnumerable<Company> GetAllCompanies()
        {

            return companyList;
        }

        /// <summary>
        /// Returns all companies that start with the specified name,
        /// and that have valid CY years and 10-K data
        /// </summary>
        /// <param name="name">The name to filter</param>
        /// <returns>The companies that match the specified name</returns>
        [HttpGet("GetCompaniesByName")]
        [Route("Companies/{name}")]
        public IEnumerable<Company> GetCompaniesByName(string name)
        {
            var filteredCompanies = companyList.Where(c => c.Name.ToLower().StartsWith(name.ToLower())).ToList();

            return filteredCompanies;
        }

        /// <summary>
        /// Return the author of this API
        /// This allows it to be used across multiple places, including Swagger and the Angular UI
        /// </summary>
        /// <returns>The author of this API</returns>
        [HttpGet("GetAuthor")]
        [Route("Author")]
        public string GetAuthor()
        {
            return Author?.Name ?? DefaultAuthor.Name;
        }

        /// <summary>
        /// Return the author's web site
        /// </summary>
        /// <returns>The author's web site</returns>
        [HttpGet("GetAuthorWebsite")]
        [Route("website")]
        public string GetAuthorWebsite()
        {
            return Author?.Website ?? DefaultAuthor.Website;
        }

        /// <summary>
        /// Return the author's code repository
        /// </summary>
        /// <returns>The author's code repository</returns>
        [HttpGet("GetAuthorRepository")]
        [Route("repo")]
        public string GetAuthorRepository()
        {
            return Author?.Repository ?? DefaultAuthor.Repository;
        }
    }

    /// <summary>
    /// Represents the Author configuration section in appSettings.json "Author":{"Name", ...}
    /// (or App Settings in Azure or Environment variables)
    /// </summary>
    internal class AuthorOptions
    {
        public string Name { get; set; }
        public string Website { get; set; }
        public string Repository { get; set; }
    }

    /// <summary>
    /// Represents a valid EDGAR company: one with a valid CY year and 10-K data
    /// </summary>
    public class Company
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public decimal StandardFundableAmount { get; set; }
        public decimal SpecialFundableAmount { get; set; }
    }
}
