import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { environment } from '../environments/environment';

describe('AppComponent', () => {
  let component: AppComponent;
  let fixture: ComponentFixture<AppComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AppComponent],
      imports: [HttpClientTestingModule]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create the app', () => {
    expect(component).toBeTruthy();
  });

  it('should retrieve companies from the Web API', () => {

    const mockCompanies = [
      { id: 12345, name: 'ABC Company', standardFundableAmount: 40000, specialFundableAmount: 50000 },
      { id: 67890, name: 'XYZ Company', standardFundableAmount: 80000, specialFundableAmount: 100000 }
    ];

    component.ngOnInit();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/edgar/companies`);
    expect(req.request.method).toEqual('GET');
    req.flush(mockCompanies);

    expect(component.companies).toEqual(mockCompanies);
  });
});
