import './App.css';
import React, { useEffect, useState } from "react";
import Companies from "./Companies";
const rawDataUrl: string = `${process.env.REACT_APP_API_URL}/api/edgar/json`;
const requirementsUrl: string = `${process.env.REACT_APP_API_URL}/Fora Coding Challenge v1.1.pdf`;
const swaggerUrl: string = `${process.env.REACT_APP_API_URL}/swagger`;
let environmentName: string = '';
let copyrightYear: number = 2024;
let reactVersion: string = '17.0';

function App() {

    const [companies, setCompanies] = useState([]);
    const [author, setAuthor] = useState('');
    const [webSite, setWebSite] = useState('');
    const [pipeline, setPipeline] = useState('');
    const [repo, setRepo] = useState('');
    const [aspnetVersion, setAspnetVersion] = useState('');
    
    function filter(search:string) {
        
        let suffix = search === undefined || search.trim() === '' ? '' : `/${search}`;

        let url = `${process.env.REACT_APP_API_URL}/api/edgar/companies${suffix}`;

        fetch(url, { method: 'Get', mode: "cors" })
            .then(resp => resp.json())
            .then(data => setCompanies(data))
            .catch(error => console.error(error))
    }

      // A thrown-together way to force this component to update after companies are sorted.
    function useForceUpdate() {
        const [value, setValue] = useState(0); // integer state
        console.log(value);
        return () => setValue(value => value + 1); // update state to force render
        // A function that increments 👆🏻 the previous state like here 
        // is better than directly setting `setValue(value + 1)`
    }

    const forceUpdate = useForceUpdate();

    function onSort(companies: any) {
        setCompanies(companies);
        forceUpdate();
    }

    reactVersion = React.version;
    copyrightYear = new Date().getFullYear();

    // Get All Companies
    useEffect(() => {
        fetch(`${process.env.REACT_APP_API_URL}/api/edgar/companies`, { method: 'Get', mode: "cors" })
          .then(resp => resp.json())
          .then(data => setCompanies(data))
          .catch(error => console.error(error))
      }, []);

    // Get Author
    useEffect(() => {
        fetch(`${process.env.REACT_APP_API_URL}/api/edgar/author`, { method: 'Get', mode: "cors" })
            .then(res => res.text())
            .then(result => setAuthor(result))
            .catch(error => console.error(error))
      }, []);
    
      // Get Website
      useEffect(() => {
        fetch(`${process.env.REACT_APP_API_URL}/api/edgar/website`, { method: 'Get', mode: "cors" })
          .then(resp => resp.text())
          .then(data => setWebSite(data))
          .catch(error => console.error(error))
      }, []);

      // Get Repo
      useEffect(() => {
        fetch(`${process.env.REACT_APP_API_URL}/api/edgar/repo`, { method: 'Get', mode: "cors" })
          .then(resp => resp.text())
          .then(data => setRepo(data))
          .catch(error => console.error(error))
      }, []);

      // Get Pipeline
      useEffect(() => {
        fetch(`${process.env.REACT_APP_API_URL}/api/edgar/pipeline`, { method: 'Get', mode: "cors" })
          .then(resp => resp.text())
          .then(data => setPipeline(data))
          .catch(error => console.error(error))
      }, []);

      // Get ASP.NET Version
      useEffect(() => {
        fetch(`${process.env.REACT_APP_API_URL}/api/edgar/aspnetVersion`, { method: 'Get', mode: "cors" })
          .then(resp => resp.text())
          .then(data => setAspnetVersion(data))
          .catch(error => console.error(error))
      }, []);

    switch (process.env.NODE_ENV?.toLowerCase()) {
        case "development":
        case "local": {
            environmentName = `- (${process.env.NODE_ENV})`;
            break;
        }
    }

  return (
    <>
            <div id="headingDiv" className="w3-row w3-theme-dark w3-padding-24">
                <div id="logoDiv" className="w3-col m3 w3-center">
                    <img src="https://www.sec.gov/files/sec-logo.png" alt="The Securities and Exchange Commission logo" title="The Securities and Exchange Commission logo" />
                    <p>This is NOT an official SEC app.</p>
                </div>
                <div id="brandDiv" className="w3-col m6 w3-center">
                    <h1 className="w3-xxxlarge">EDGAR Company Funding { environmentName }</h1>
                    <div className="w3-row">
                      <div className="w3-col s4"><a target="_blank" rel="noreferrer" title="Read the requirements" href={requirementsUrl}><i className='fa fa-book w3-card-4'></i>Requirements</a></div>
                      <div className="w3-col s4"><a target="_blank" rel="noreferrer" title="See the raw data" href={rawDataUrl}><i className='fa fa-database w3-card-4'></i>Data</a></div>
                      <div className="w3-col s4"><a target="_blank" rel="noreferrer" title="Test the API" href={swaggerUrl}><i className='fa fa-globe w3-card-4'></i>API</a></div>
        </div >
  </div >
  <div id="authorDiv" className="w3-col m3">
                  <p>Created by <a target="_blank" rel="noreferrer" title="Get to know the author" href={webSite}>{author}</a></p>
    <p>Clone the <a target="_blank" rel="noreferrer" title="Clone the code" href={repo}>code repository</a></p >
        <p>View the <a target="_blank" rel="noreferrer" title="View the build/release pipeline" href={pipeline}>build/release pipeline</a></p >
            <p>A sample interview project for Fora Financial (www.forafinancial.com) and Soltech</p>
  </div >
</div >


<div className="w3-padding-24 w3-center">
  <h2>
    <div className="search-container w3-center">
                      <input id="searchInput" className="w3-border w3-round-xlarge w3-padding-14" type="text" autoComplete="off" onChange={(event) => filter(event.target.value)} placeholder="Company" />

      <div className="search-icon">
        <i className="fa fa-search"></i>
      </div>

    </div>
  </h2>
          </div>

<div className="w3-responsive">
  <Companies companies={companies} onSort={onSort}/>
</div >

        <footer className="w3-container w3-theme-dark w3-padding-16 w3-center">
            <h3>Created with ASP.NET Core { aspnetVersion }, Node 18.19.1, and React { reactVersion }</h3>
            <p className="w3-tiny">&copy; { copyrightYear }. All rights reserved.</p>
        </footer>
    </>
  )
}

export default App

