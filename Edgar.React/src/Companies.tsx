import React from 'react';
import CompanyInfo from './CompanyInfo'
import Company from './Company'
import sort from './Sort';
import './App.css';

function Companies(props:any) {

    let companies:CompanyInfo[] = props.companies;

return (
<table className="w3-table-striped">
    <thead>
      <tr className="w3-theme-dark">
                          <th><span onClick={() => {sort(companies, 'id'); props.onSort(companies);}}>CIK</span></th>
                          <th><span onClick={() => {sort(companies, 'name'); props.onSort(companies);}}>Name</span></th>
                      <th><span onClick={() => {sort(companies, 'standardFundableAmount'); props.onSort(companies);}}>Standard Fundable Amount</span></th>
                  <th><span onClick={() => {sort(companies, 'specialFundableAmount'); props.onSort(companies);}}>Special Fundable Amount</span></th >
      </tr >
    </thead >
    <tbody>
        { companies.map(company => <Company id={company.id} name={company.name} standardFundableAmount={company.standardFundableAmount} specialFundableAmount={company.specialFundableAmount} />) }
    </tbody >
  </table >
)}
 
export default Companies;