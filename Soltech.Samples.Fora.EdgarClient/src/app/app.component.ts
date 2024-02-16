import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { environment } from '../environments/environment';
interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

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
  public forecasts: WeatherForecast[] = [];
  public companies: Company[] = [];
  public author: string = '';
  search: string = '';
  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.getForecasts();
    this.getCompanies();
    this.getAuthor();
  }

  getForecasts() {
    this.http.get<WeatherForecast[]>('/weatherforecast').subscribe(
      (result) => {
        this.forecasts = result;
      },
      (error) => {
        console.error(error);
      }
    );
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

  title = 'Edgar Company Funding';
}
