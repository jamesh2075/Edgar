import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
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
  search: string = '';
  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.getCompanies();
    this.getAuthor();
    this.getWebsite();
    this.getRepository();
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
        this.author = `Created by: ${result}`;
      },
      (error) => {
        console.error(error);
      }
    )
  }

  getWebsite() {
    this.http.get(`${environment.apiUrl}/api/edgar/website`, { responseType: 'text' }).subscribe(
      (result) => {
        this.website = `See my profile: ${result}`;
      },
      (error) => {
        console.error(error);
      }
    )
  }

  getRepository() {
    this.http.get(`${environment.apiUrl}/api/edgar/repo`, { responseType: 'text' }).subscribe(
      (result) => {
        this.repo = `See the Code: ${result}`;
      },
      (error) => {
        console.error(error);
      }
    )
  }

  title = 'Edgar Company Funding';
}
