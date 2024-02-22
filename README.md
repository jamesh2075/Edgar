**About This Project**

This is a sample project for Fora Financial.
Wiki / About the EDGAR App
The EDGAR App
This app consists of a front-end UI built with Angular. The back-end API uses ASP.NET  Core. The ASP.NET  Core Web API also includes a WebJob that can manually download data that is used by the API.

See a live version of the web site at https://edgarclient.azurewebsites.net.

View the OpenAPI (Swagger) documentation at https://edgarapi.azurewebsites.net/swagger.

To clone the code:

git clone https://github.com/jamesh2075/Edgar.git 

This project consists of two build pipelines and three release pipelines. One build pipeline builds the ASP.NET  Core API, MSTest Unit Tests, and the WebJob. The other build pipeline builds the Angular UI.

One release pipeline deploys the ASP.NET  Core API and WebJob to Azure. The second release pipeline deploys the Angular UI to Azure. These two pipelines run on a trigger that watches for build artifacts. The third pipeline runs on demand, and it deploys all apps.

The project was created to adhere to these requirements: [Fora Coding Challenge v1.1.pdf](https://edgarapi.azurewebsites.net/Fora%20Coding%20Challenge%20v1.1.pdf)https://edgarapi.azurewebsites.net/Fora%20Coding%20Challenge%20v1.1.pdf
