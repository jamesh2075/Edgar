using Soltech.Samples.Fora.EdgarApi.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Soltech.Samples.Fora.EdgarData;
using Microsoft.Extensions.DependencyInjection;

namespace Soltech.Samples.Fora.EdgarTests.UnitTests
{
    

    [TestClass()]
    public class EdgarControllerTests
    {
        private readonly EdgarController? controller = null;

        public EdgarControllerTests()
        {
            // Generate fake Configuration settings
            var testConfiguration = new Dictionary<string, string>
            {
                { "Author:Name", "Test Author" },
                { "Author:Repository","Test Repo" },
                { "Author:Website","Test Website" },
            };

            // Build the Configuration
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfiguration)
                .Build();

            // Load the Data.json file and parse it into EDGAR data
            // Then constructor the API controller
            // The controller uses dependency injection
            using (StreamReader reader = new StreamReader($@"TestFiles\Data.json"))
            {
                var jsonData = reader.ReadToEnd();
                var edgarList = JsonSerializer.Deserialize<List<EdgarCompanyInfo>>(jsonData);
                controller = new EdgarController(edgarList, config);
            }
        }

        [TestMethod()]
        public void GetJsonTest()
        {
            var result = controller.GetJson();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(List<EdgarCompanyInfo>));
        }

        [TestMethod()]
        public void GetAllCompaniesTest()
        {
            var allCompanies = controller.GetAllCompanies();
            Assert.IsNotNull(allCompanies);
            Assert.AreEqual(allCompanies.Count(), 91);
        }

        [TestMethod()]
        [DataRow("L")]
        [DataRow("l")]
        [DataRow("LU")]
        [DataRow("lu")]
        [DataRow("Lu")]
        [DataRow("Lumen")]
        public void GetCompaniesByNameTest(string company)
        {
            const string companyName = "Lumen Technologies, Inc.";
            var companies = controller.GetCompaniesByName(company);
            Assert.IsNotNull(companies);
            Assert.IsTrue(companies.Count() > 0);
            var firstOne = companies.First();
            Assert.AreEqual(firstOne.Name, companyName);
        }

        [TestMethod()]
        public void GetAuthorTest()
        {
            Assert.AreEqual(controller.GetAuthor(), "Test Author");
        }

        [TestMethod()]
        public void GetAuthorWebsiteTest()
        {
            Assert.AreEqual(controller.GetAuthorWebsite(), "Test Website");
        }

        [TestMethod()]
        public void GetAuthorRepositoryTest()
        {
            Assert.AreEqual(controller.GetAuthorRepository(), "Test Repo");
        }

        [TestMethod()]
        public void GetAspNetCoreVersionTest()
        {
            Assert.AreEqual(controller.GetAspNetCoreVersion(), System.Environment.Version.ToString());
        }
    }
}