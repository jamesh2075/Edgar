using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Net.Http;
using System.Reflection;

namespace Edgar.Razor.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly HttpClient _client;

        public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _client = clientFactory.CreateClient(Constants.EdgarApiHttpClient);
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; } = null;

        public async Task OnGet(string? search, string? field, string? previousField, bool? ascending)
        {
            if (!String.IsNullOrEmpty(search))
            {
                SearchString = search;
            }
            await GetCompanies(SearchString);

            if (String.IsNullOrEmpty(field))
            {
                return;
            }

            if (field != null && field == previousField)
            {
                ascending = !ascending; // If the same field is being sorted, reverse the sort
            }
            else if (field != null)
            {
                ascending = true; // Otherwise, sort the field in ascending order
            }
            previousField = field;

            switch (field)
            {
                case "Id":
                    Companies?.Sort((c1, c2) =>  ascending ?? true ? c1.Id.CompareTo(c2.Id) : c2.Id.CompareTo(c1.Id));
                    break;
                case "Name":
                    Companies?.Sort((c1, c2) => ascending ?? true ? c1.Name?.CompareTo(c2?.Name) ?? 0 : c2.Name?.CompareTo(c1?.Name) ?? 0);
                    break;
                case "StandardFundableAmount":
                    Companies?.Sort((c1, c2) => ascending ?? true ? c1.StandardFundableAmount.CompareTo(c2.StandardFundableAmount) : c2.StandardFundableAmount.CompareTo(c1.StandardFundableAmount));
                    break;
                case "SpecialFundableAmount":
                    Companies?.Sort((c1, c2) => ascending ?? false ? c1.SpecialFundableAmount.CompareTo(c2.SpecialFundableAmount) : c2.SpecialFundableAmount.CompareTo(c1.SpecialFundableAmount));
                    break;
            }

            ViewData["PreviousField"] = field;
            ViewData["Ascending"] = ascending;
        }

        public async Task GetCompanies(string? name)
        {
            string endpoint = $"{Constants.CompaniesEndpoint}/{name}";
            var companies = await _client.GetFromJsonAsync<List<Company>>(endpoint);
            TempData["Companies"] = companies;
        }

        public List<Company>? Companies
        {
            get
            {
                return TempData["Companies"] as List<Company>;
            }
        }
    }


}
