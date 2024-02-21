using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soltech.Samples.Fora.EdgarApi.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soltech.Samples.Fora.EdgarApi;
using Soltech.Samples.Fora.EdgarData;
using System.Security.AccessControl;
using System.Text.Json;

namespace Soltech.Samples.Fora.EdgarApi.Tests
{
    [TestClass()]
    public class CalculatorTests
    {
        private EdgarCompanyInfo? GetCompany(string file)
        {
            EdgarCompanyInfo company = null;
            using (StreamReader reader = new StreamReader($@"TestFiles\{file}.json"))
            {
                var jsonData = reader.ReadToEnd();
                company = JsonSerializer.Deserialize<EdgarCompanyInfo>(jsonData);
            }
            return company;
        }

        [TestMethod("Has Income Between 2018 and 2022")]
        public void HasValidIncomeTest()
        {
            var company = GetCompany("ValidIncome");
            Calculator calculator = new Calculator(company);
            bool valid = calculator.HasValidIncome();
            Assert.IsTrue(valid);
        }

        [TestMethod("Does Not Have Income Between 2018 and 2022")]
        public void HasInvalidIncomeTest()
        {
            EdgarCompanyInfo company = GetCompany("InvalidIncome");
            Calculator calculator = new Calculator(company);
            bool valid = calculator.HasValidIncome();
            Assert.IsFalse(valid);
        }

        [TestMethod("Name Starts with Vowel")]
        public void StartsWithVowelTest()
        {
            EdgarCompanyInfo company = new EdgarCompanyInfo { EntityName = "Aldi" };
            Calculator calculator = new Calculator(company);
            bool result = calculator.StartsWithVowel();
            Assert.IsTrue(result);
        }

        [TestMethod("Name Does Not Start with Vowel")]
        public void DoesNotStartWithVowelTest()
        {
            EdgarCompanyInfo company = new EdgarCompanyInfo { EntityName = "Best Buy" };
            Calculator calculator = new Calculator(company);
            bool result = calculator.StartsWithVowel();
            Assert.IsFalse(result);
        }

        [TestMethod("Get Standard Fundable Amount (>= $10 Billion)")]
        public void GetStandardFndableAmountFor10BillionTest()
        {
            EdgarCompanyInfo company = GetCompany("10BNameStartsWithVowel");
            Calculator calculator = new Calculator(company);
            var amount = calculator.GetStandardFndableAmount();
            Assert.AreEqual(amount, 1233000000m);
        }

        [TestMethod("Get Standard Fundable Amount (< $10 Billion)")]
        public void GetStandardFndableAmountForLessThan10BillionTest()
        {
            EdgarCompanyInfo company = GetCompany("NameStartsWithVowel");
            Calculator calculator = new Calculator(company);
            var amount = calculator.GetStandardFndableAmount();
            Assert.AreEqual(amount, 21500m);
        }

        [TestMethod("Get Special Fundable Amount (>= $10 Billion, Name starts with Vowel)")]
        public void GetSpecialFundableAmountFor10BillionVowelTest()
        {
            EdgarCompanyInfo company = GetCompany("10BNameStartsWithVowel");
            Calculator calculator = new Calculator(company);
            var amount = calculator.GetSpecialFundableAmount(1233000000, 10000000000, 10000);
            Assert.AreEqual(amount, 1417950000m);
        }

        [TestMethod("Get Special Fundable Amount (>= $10 Billion, Name does not start with Vowel)")]
        public void GetSpecialFundableAmountFor10BillionNoVowelTest()
        {
            EdgarCompanyInfo company = GetCompany("10BDecreasedIncome");
            Calculator calculator = new Calculator(company);
            var amount = calculator.GetSpecialFundableAmount(1233000000, 10000000000, 10000);
            Assert.AreEqual(amount, 924750000m);
        }

        [TestMethod("Get Special Fundable Amount (< $10 Billion, Name starts with Vowel)")]
        public void GetSpecialFundableAmountForLessThan10BillionVowelTest()
        {
            EdgarCompanyInfo company = GetCompany("NameStartsWithVowel");
            Calculator calculator = new Calculator(company);
            var amount = calculator.GetSpecialFundableAmount(21500, 100000, 10000);
            Assert.AreEqual(amount, 24725m);
        }

        [TestMethod("Get Special Fundable Amount (< $10 Billion, Name does not start with Vowel")]
        public void GetSpecialFundableAmountForLessThan10BillionNoVowelTest()
        {
            EdgarCompanyInfo company = GetCompany("DecreasedIncome");
            Calculator calculator = new Calculator(company);
            var amount = calculator.GetSpecialFundableAmount(21500, 100000, 10000);
            Assert.AreEqual(amount, 16125m);
        }

        [TestMethod("Get 2021 Income")]
        public void Get2021IncomeTest()
        {
            EdgarCompanyInfo company = GetCompany("DecreasedIncome");
            Calculator calculator = new Calculator(company);
            var amount = calculator.Get2021Income();
            Assert.AreEqual(amount, 100000m);
        }

        [TestMethod("Get 2022 Income")]
        public void Get2022IncomeTest()
        {
            EdgarCompanyInfo company = GetCompany("DecreasedIncome");
            Calculator calculator = new Calculator(company);
            var amount = calculator.Get2022Income();
            Assert.AreEqual(amount, 10000m);
        }

        [TestMethod("Has Positive Income in 2021 and 2022")]
        public void HasPositiveIncome()
        {
            EdgarCompanyInfo company = GetCompany("PositiveIncome");
            Calculator calculator = new Calculator(company);
            var result = calculator.HasPositiveIncome();
            Assert.IsTrue(result);
        }

        [TestMethod("Has Negative Income in 2021 or 2022")]
        public void HasNegativeIncome()
        {
            EdgarCompanyInfo company = GetCompany("NegativeIncome");
            Calculator calculator = new Calculator(company);
            var result = calculator.HasPositiveIncome();
            Assert.IsFalse(result);
        }
    }
}