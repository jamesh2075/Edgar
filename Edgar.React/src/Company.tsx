import React from 'react'
import CompanyInfo from './CompanyInfo'

// deconstructed props
function Company(props:CompanyInfo) {

  return (
      <tr>
        <td>{ props.id }</td>
        <td>{ props.name }</td>
          <td>{props.standardFundableAmount.toLocaleString("en-US", { style: "currency", currency: "USD" }) }</td>
          <td>{props.specialFundableAmount.toLocaleString("en-US", { style: "currency", currency: "USD" })}</td>
      </tr>
    )
}

export default Company