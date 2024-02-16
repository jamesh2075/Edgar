using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Soltech.Samples.Fora.EdgarApi;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Net.NetworkInformation;

namespace Soltech.Samples.Fora.EdgarApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EdgarController : ControllerBase
    {
        private IWebHostEnvironment environment;

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
        const decimal decreasedIncome = .25m; // 25 percent for companies whose 2022 income was less than 2021 income
        public EdgarController(IWebHostEnvironment environment)
        {
            this.environment = environment;

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
                    var list = from u in edgar.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd let year = int.Parse(u.Frame.Substring(2))
                               where year >= 2018 && year <= 2022 select u;

                    decimal highestIncome = list.Max(u => u.Val);
                    if (highestIncome >= 10000000000)
                        standardFundableAmount = highIncomePercentage * highestIncome;
                    else
                        standardFundableAmount = lowIncomePercentage * highestIncome;
                }

                // Initially, the Special Fundable Amount is the same as Standard Fundable Amount.
                // If the company name starts with a vowel, add 15% to the standard funding amount.
                // If the company’s 2022 income was less than their 2021 income, subtract 25% from their
                // standard funding amount.

                specialFundableAmount = standardFundableAmount;
                var companyName = String.IsNullOrEmpty(edgar.EntityName) ? " " : edgar.EntityName;
                var vowels = new char[] { 'A', 'E', 'I', 'O', 'U' }; // What about sometimes Y :)
                if (vowels.Contains(companyName[0]))
                    specialFundableAmount = (companyVowelPercentage * standardFundableAmount) + standardFundableAmount;


                var company = new Company();
                company.Name = edgar.EntityName;
                company.Id = edgar.Cik;
                company.StandardFundableAmount = standardFundableAmount;
                company.SpecialFundableAmount = specialFundableAmount;

                return company;
            });
        }

        [HttpGet(Name = "GetJson")]
        [Produces("application/json")]
        [Route("json")]
        public JsonResult GetJson()
        {
            return new JsonResult(edgarList);
        }

        [HttpGet(Name = "GetAllCompanies")]
        [Route("Companies")]
        public IEnumerable<Company> GetAllCompanies()
        {
            
            return companyList;
        }

        [HttpGet(Name = "GetCompaniesByName")]
        [Route("Companies/{name}")]
        public IEnumerable<Company> GetCompaniesByName(string name)
        {
            var filteredCompanies = companyList.Where(c => c.Name.ToLower().StartsWith(name.ToLower())).ToList();

            return filteredCompanies;
        }

        [Route("Author")]
        public string GetAuthor()
        {
            return "James Henry";
        }
    }

    public class Company
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public decimal StandardFundableAmount { get; set; }
        public decimal SpecialFundableAmount { get; set; }
    }
}
