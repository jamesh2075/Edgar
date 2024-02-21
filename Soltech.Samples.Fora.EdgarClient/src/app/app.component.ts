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
