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
using System.Linq;
using System.Runtime.CompilerServices;

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
                Calculator calculator = new Calculator(edgar);

                decimal standardFundableAmount = 0;
                decimal specialFundableAmount = 0;

                // How to calculate Standard Fundable Amount:
                // Bullet Point 1
                // Company must have income data for all years between (and including) 2018 and 2022.
                // If they did not, their Standard Fundable Amount is $0.
                var validIncome = calculator.HasValidIncome();

                // Bullet Point 2
                // Company must have had positive income in both 2021 and 2022:
                // If they did not, their Standard Fundable Amount is $0..
                var income2021 = calculator.Get2021Income();
                var income2022 = calculator.Get2022Income();
                validIncome &= calculator.HasPositiveIncome();
                
                if (validIncome)
                {
                    // Bullet Point 3
                    // Using highest income between 2018 and 2022:
                    // If income is greater than or equal to $10B, standard fundable amount is 12.33%
                    // of income.
                    // If income is less than $10B, standard fundable amount is 21.51 % of income.
                    standardFundableAmount = calculator.GetStandardFndableAmount();

                    // How to calculate the Special Fundable Amount:
                    // Initially, the Special Fundable Amount is the same as Standard Fundable Amount.
                    // If the company name starts with a vowel, add 15% to the standard funding amount.
                    // If the company’s 2022 income was less than their 2021 income, subtract 25% from their
                    // standard funding amount.
                    specialFundableAmount = calculator.GetSpecialFundableAmount(standardFundableAmount, income2021, income2022);
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
        public string? GetAuthor()
        {
            return Author?.Name ?? DefaultAuthor.Name;
        }

        /// <summary>
        /// Return the author's web site
        /// </summary>
        /// <returns>The author's web site</returns>
        [HttpGet("GetAuthorWebsite")]
        [Route("website")]
        public string? GetAuthorWebsite()
        {
            return Author?.Website ?? DefaultAuthor.Website;
        }

        /// <summary>
        /// Return the author's code repository
        /// </summary>
        /// <returns>The author's code repository</returns>
        [HttpGet("GetAuthorRepository")]
        [Route("repo")]
        public string? GetAuthorRepository()
        {
            return Author?.Repository ?? DefaultAuthor.Repository;
        }

        /// <summary>
        /// Return the author's code repository
        /// </summary>
        /// <returns>The author's code repository</returns>
        [HttpGet("GetAspNetCoreVersion")]
        [Route("aspnetVersion")]
        public string GetAspNetCoreVersion()
        {
            return System.Environment.Version.ToString();
        }
    }

    /// <summary>
    /// Represents the Author configuration section in appSettings.json "Author":{"Name", ...}
    /// (or App Settings in Azure or Environment variables)
    /// </summary>
    internal record AuthorOptions
    {
        /// <summary>
        /// My name (may change to my email address)
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// My website (currently my LinkedIn page, but may change to my business site
        /// </summary>
        public string? Website { get; set; }
        /// <summary>
        /// My repository (currently Github, but may change to Azure Repos)
        /// </summary>
        public string? Repository { get; set; }
    }

    /// <summary>
    /// Represents a valid EDGAR company: one with a valid CY year and 10-K data
    /// </summary>
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

    /// <summary>
    /// Performs calculations on EDGAR data
    /// </summary>
    public class Calculator
    {
        private EdgarCompanyInfo company;
        // Constants (Ideally, these should be placed in a configuration setting)
        const decimal highIncomePercentage = .1233m; // 12.33 percent for income 10B or greater
        const decimal lowIncomePercentage = .215m; // 21.5 percent for income less than 10B
        const decimal companyVowelPercentage = .15m; // 15 percent for companies whose name start with a vowel
        const decimal decreasedIncomePercentage = .25m; // 25 percent for companies whose 2022 income was less than 2021 income

        /// <summary>
        /// Initializes a new instance with the specified EDGAR company
        /// </summary>
        /// <param name="company">The EDGAR company</param>
        public Calculator(EdgarCompanyInfo company)
        {
            this.company = company;
        }

        /// <summary>
        /// Returns <b>true</b> if there is income between 2018 and 2022;
        /// </summary>
        /// <returns><b>true</b> if there is income between 2018 and 2022;</returns>
        public bool HasValidIncome()
        {
            var anyValidYear = company.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd?.Where(u => u.Frame.Length > 2 && int.TryParse(u.Frame.Substring(2), out int year) && year >= 2018 && year <= 2022).Any();
            return anyValidYear ?? false;
        }

        /// <summary>
        /// Returns <b>true</b> if the company name starts with a vowel
        /// </summary>
        /// <returns><b>true</b> if the company name starts with a vowel</returns>
        public bool StartsWithVowel()
        {
            var vowels = new char[] { 'A', 'E', 'I', 'O', 'U' }; // What about sometimes Y :)
            var name = string.IsNullOrEmpty(company.EntityName) ? " " : company.EntityName;
            return (vowels.Contains(name[0]));
        }

        /// <summary>
        /// Returns the special fundable amount.
        /// </summary>
        /// <param name="standardFundableAmount"></param>
        /// <param name="income2021">2021 income</param>
        /// <param name="income2022">2022 income</param>
        /// <returns>The special fundable amount</returns>
        /// <remarks>This amount is calculated as follows:
        /// If the company name starts with a vowel, then the amount is equal to the standard fundable amount plus 15%.
        /// Otherwise, if the 2022 income is less than the 2021 income, the amount is equal to the standard fundable amount minus 25%.
        /// </remarks>
        public decimal GetSpecialFundableAmount(decimal standardFundableAmount, decimal? income2021, decimal? income2022)
        {
            var specialFundableAmount = standardFundableAmount;
            if (StartsWithVowel())
            {
                specialFundableAmount = (companyVowelPercentage * standardFundableAmount) + standardFundableAmount;
            }
            else if (income2022 < income2021)
            {
                specialFundableAmount = standardFundableAmount - (decreasedIncomePercentage * standardFundableAmount);
            }
            return specialFundableAmount;
        }

        /// <summary>
        /// Returns the standard fundable amount.
        /// </summary>
        /// <returns>The standard fundable amount</returns>
        /// <remarks>This amount is calculated as follows:
        /// First get the highest income between 2018 and 2022.
        /// If the income is greater than $10B, then the standard fundable amount is equal to 12.33% of it.
        /// Otherwise, the standard fundable amount is equal to 21.5% of it.
        /// </remarks>
        public decimal GetStandardFndableAmount()
        {
            var standardFundableAmount = 0m;

            var list = from u in company.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd
                       let year = int.Parse(u.Frame.Substring(2))
                       where year >= 2018 && year <= 2022
                       select u;

            decimal highestIncome = list.Max(u => u.Val);
            if (highestIncome >= 10000000000)
                standardFundableAmount = highIncomePercentage * highestIncome;
            else
                standardFundableAmount = lowIncomePercentage * highestIncome;

            return standardFundableAmount;
        }

        /// <summary>
        /// Returns <b>true</b> 2021 and 2022 income are positive
        /// </summary>
        /// <returns><b>true</b> if 2021 and 2022 income are positive</returns>
        public bool HasPositiveIncome()
        {
            return Get2022Income() > 0 && Get2021Income() > 0;
        }

        /// <summary>
        /// Returns the income for 2021.
        /// </summary>
        /// <returns>The income for 2021</returns>
        public decimal? Get2021Income()
        {
            decimal? income2021 = company.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd?.Where(u => int.Parse(u.Frame.Substring(2)) == 2021).FirstOrDefault()?.Val;
            
            return income2021;
        }

        /// <summary>
        /// Returns the income for 2022
        /// </summary>
        /// <returns>The income for 2022</returns>
        public decimal? Get2022Income()
        {
            decimal? income2022 = company.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd?.Where(u => int.Parse(u.Frame.Substring(2)) == 2022).FirstOrDefault()?.Val;
            
            return income2022;
        }
    }
}
