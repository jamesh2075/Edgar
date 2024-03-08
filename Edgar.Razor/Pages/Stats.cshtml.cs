using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Edgar.Razor.Pages
{
    public class PrivacyModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;
        private readonly HttpClient _client;

        public PrivacyModel(ILogger<PrivacyModel> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _client = clientFactory.CreateClient(Constants.EdgarApiHttpClient);
        }

        [BindProperty, DataType(DataType.Currency)]
        public decimal? MinimumStandardFundableAmount {get; set;}
        [BindProperty, DataType(DataType.Currency)]
        public decimal? MaximumStandardFundableAmount { get; set;}
        [BindProperty, DataType(DataType.Currency)]
        public decimal? MedianStandardFundableAmount { get; set; }
        [BindProperty, DataType(DataType.Currency)]
        public decimal? AverageStandardFundableAmount { get; set; }
        [BindProperty, DataType(DataType.Currency)]
        public decimal? ModeStandardFundableAmount { get; set; }
        [BindProperty, DataType(DataType.Currency)]
        public decimal? StandardDeviationStandardFundableAmount { get; set; }

        [BindProperty, DataType(DataType.Currency)]
        public decimal? MinimumSpecialFundableAmount { get; set; }
        [BindProperty, DataType(DataType.Currency)]
        public decimal? MaximumSpecialFundableAmount { get; set; }
        [BindProperty, DataType(DataType.Currency)]
        public decimal? MedianSpecialFundableAmount { get; set; }
        [BindProperty, DataType(DataType.Currency)]
        public decimal? AverageSpecialFundableAmount { get; set; }
        [BindProperty, DataType(DataType.Currency)]
        public decimal? ModeSpecialFundableAmount { get; set; }
        [BindProperty, DataType(DataType.Currency)]
        public decimal? StandardDeviationSpecialFundableAmount { get; set; }

        public async Task OnGet()
        {
            var companies = await _client.GetFromJsonAsync<List<Company>>(Constants.CompaniesEndpoint);

            MinimumStandardFundableAmount = companies?.Min(c => c.StandardFundableAmount);
            MaximumStandardFundableAmount = companies?.Max(c => c.StandardFundableAmount);
            AverageStandardFundableAmount = companies?.Average(c => c.StandardFundableAmount);
            MedianStandardFundableAmount = companies?.Select(c => c.StandardFundableAmount).Median();
            ModeStandardFundableAmount = companies?.Select(c => c.StandardFundableAmount).Mode();
            StandardDeviationStandardFundableAmount = companies?.Select(c => c.StandardFundableAmount).StandardDeviation();

            MinimumSpecialFundableAmount = companies?.Min(c => c.SpecialFundableAmount);
            MaximumSpecialFundableAmount = companies?.Max(c => c.SpecialFundableAmount);
            AverageSpecialFundableAmount = companies?.Average(c => c.SpecialFundableAmount);
            MedianSpecialFundableAmount = companies?.Select(c => c.SpecialFundableAmount).Median();
            ModeSpecialFundableAmount = companies?.Select(c => c.SpecialFundableAmount).Mode(); ;
            StandardDeviationSpecialFundableAmount = companies?.Select(c => c.SpecialFundableAmount).StandardDeviation();
        }
    }

    internal static class Statistics
    {
        public static decimal Median(this IEnumerable<decimal> list)
        {
            int count = list.Count();
            var orderedList = list.OrderBy(value => value);
            decimal median = orderedList.ElementAt(count / 2) + orderedList.ElementAt((count - 1) / 2);
            median /= 2;
            return median;
        }

        public static decimal Mode(this IEnumerable<decimal> list)
        {
            return
                list
                    .GroupBy(value => value)
                    .OrderByDescending(group => group.Count())
                    .Select(group => group.Key)
                    .First();
        }

        public static decimal StandardDeviation(this IEnumerable<decimal> list)
        {
            double result = 0;

            if (list.Any())
            {
                double average = (double)list.Average();
                double sum = list.Sum(d => Math.Pow((double)d - average, 2));
                result = Math.Sqrt((sum) / list.Count());
            }
            return (decimal)result;
        }
    }

}
