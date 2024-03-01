using Edgar.Api.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Edgar.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Edgar.Api.Tests
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
                { "Project:Author", "Test Author" },
                { "Project:Repository","Test Repo" },
                { "Project:Pipeline","Test Pipeline" },
                { "Project:Bio","Test Website" },
            } as IEnumerable<KeyValuePair<string, string?>>;

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
                if (edgarList is not null)
                    controller = new EdgarController(edgarList, config);
            }
        }

        [TestMethod("Get JSON Data")]
        public void GetJsonTest()
        {
            var result = controller?.GetJson();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType(result.Value, typeof(List<EdgarCompanyInfo>));
        }

        [TestMethod("Get All Companies")]
        public void GetAllCompaniesTest()
        {
            var allCompanies = controller?.GetAllCompanies();
            Assert.IsNotNull(allCompanies);
            Assert.AreEqual(allCompanies.Count(), 91);
        }

        [TestMethod("Get Companies by Name")]
        [DataRow("L")]
        [DataRow("l")]
        [DataRow("LU")]
        [DataRow("lu")]
        [DataRow("Lu")]
        [DataRow("Lumen")]
        public void GetCompaniesByNameTest(string company)
        {
            const string companyName = "Lumen Technologies, Inc.";
            var companies = controller?.GetCompaniesByName(company);
            Assert.IsNotNull(companies);
            Assert.IsTrue(companies.Count() > 0);
            var firstOne = companies.First();
            Assert.AreEqual(firstOne.Name, companyName);
        }

        [TestMethod("Get Author of the Project")]
        public void GetAuthorTest()
        {
            Assert.AreEqual(controller?.GetAuthor(), "Test Author");
        }

        [TestMethod("Get Author Bio Website")]
        public void GetAuthorBioWebsiteTest()
        {
            Assert.AreEqual(controller?.GetBioWebsite(), "Test Website");
        }

        [TestMethod("Get Code Repository")]
        public void GetRepositoryTest()
        {
            Assert.AreEqual(controller?.GetRepository(), "Test Repo");
        }

        [TestMethod("Get Build Pipeline")]
        public void GetPipelineTest()
        {
            Assert.AreEqual(controller?.GetPipeline(), "Test Pipeline");
        }

        [TestMethod("Get ASP.NET Core Version")]
        public void GetAspNetCoreVersionTest()
        {
            Assert.AreEqual(controller?.GetAspNetCoreVersion(), System.Environment.Version.ToString());
        }
    }
}