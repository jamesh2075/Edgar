import { HttpClient } from '@angular/common/http';
import { Component, OnInit, VERSION } from '@angular/core';
import { environment } from '../environments/environment';
import { CurrencyPipe } from '@angular/common';

interface Company {
  id: number;
  name: string;
  standardFundableAmount: number;
  specialFundableAmount: number;
}


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {

  public companies: Company[] = [];
  public author: string = '';
  public website: string = '';
  public repo: string = '';
  public pipeline: string = '';
  public aspnetVersion: string = '';
  public rawDataUrl: string = `${environment.apiUrl}/api/edgar/json`;
  public requirementsUrl: string = `${environment.apiUrl}/Fora Coding Challenge v1.1.pdf`;
  public swaggerUrl: string = `${environment.apiUrl}/swagger`;
  public environmentName: string = '';
  public copyrightYear: number = 2024;
  public angularVersion: string = '17.2.1';

  search: string = '';

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.getCompanies();
    this.getAuthor();
    this.getWebsite();
    this.getRepository();
    this.getPipeline();
    this.getAspNetCoreVersion();

    switch (environment.name.toLowerCase()) {
      case "development":
      case "local": {
        this.environmentName = `- (${environment.name})`;
        break;
      }
    }
    
    this.angularVersion = VERSION.full;
    this.copyrightYear = new Date().getFullYear();
  }

  getCompanies() {
    this.http.get<Company[]>(`${environment.apiUrl}/api/edgar/companies`).subscribe(
      (result) => {
        this.companies = result;
      },
      (error) => {
        console.error(error);
      }
    )
  }

  filter() {
    if (this.search == undefined || this.search.trim() == '') {
      this.getCompanies();
      return;
    }

    this.http.get<Company[]>(`${environment.apiUrl}/api/edgar/companies/${this.search}`).subscribe(
      (result) => {
        this.companies = result;
      },
      (error) => {
        console.error(error);
      }
    )
  }

  ascending:boolean = false;
  previousField:string = "";

  sort(field: string) {

    if (field == this.previousField) {
      this.ascending = !this.ascending; // If the same field is being sorted, reverse the sort
    }
    else {
      this.ascending = true; // Otherwise, sort the field in ascending order
    }

    this.previousField = field;

    this.companies.sort((a: Company, b: Company) => {

      // Convert the Company instances into regular JavaScript objects
      // so that their properties can be indexed by name.
      // This eliminates the need to each field (i.e. a.id, a.name, a.standardFundableAmount...)
      var objA = JSON.parse(JSON.stringify(a));
      var objB = JSON.parse(JSON.stringify(b));

      // Create the sort algorithm.
      // Return 1 if the first item is greater than the second.
      // Return -1 if the second item is greater than the first.
      // Return 0 if the two items are equal
      var greater = this.ascending ? 1 : -1;
      var lesser = this.ascending ? -1 : 1;
      var result =
        objA[field].toLowerCase() > objB[field].toLowerCase() ? greater :
          objA[field].toLowerCase() < objB[field].toLowerCase() ? lesser : 0;

      return result;
    });
  }

  getAuthor() {
    this.http.get(`${environment.apiUrl}/api/edgar/author`, { responseType: 'text' }).subscribe(
      (result) => {
        this.author = `${result}`;
      },
      (error) => {
        console.error(error);
      }
    )
  }

  getWebsite() {
    this.http.get(`${environment.apiUrl}/api/edgar/website`, { responseType: 'text' }).subscribe(
      (result) => {
        this.website = `${result}`;
      },
      (error) => {
        console.error(error);
      }
    )
  }

  getRepository() {
    this.http.get(`${environment.apiUrl}/api/edgar/repo`, { responseType: 'text' }).subscribe(
      (result) => {
        this.repo = `${result}`;
      },
      (error) => {
        console.error(error);
      }
    )
  }

  getPipeline() {
    this.http.get(`${environment.apiUrl}/api/edgar/pipeline`, { responseType: 'text' }).subscribe(
      (result) => {
        this.pipeline = `${result}`;
      },
      (error) => {
        console.error(error);
      }
    )
  }

  getAspNetCoreVersion() {
    this.http.get(`${environment.apiUrl}/api/edgar/aspnetVersion`, { responseType: 'text' }).subscribe(
      (result) => {
        this.aspnetVersion = `${result}`;
      },
      (error) => {
        console.error(error);
      }
    )
  }

  title = 'EDGAR Company Funding';
}
