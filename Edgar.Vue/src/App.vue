<script setup lang="ts">
    import HelloWorld from './components/HelloWorld.vue'
    import TheWelcome from './components/TheWelcome.vue'
    import EdgarCompanyList from './components/EdgarCompanyList.vue'
</script>

<template>
    <header>
        <div id="headingDiv" class="w3-row w3-theme-dark w3-padding-24">
            <div id="logoDiv" class="w3-col m3 w3-center">
                <img src="https://www.sec.gov/files/sec-logo.png" alt="The Securities and Exchange Commission logo" title="The Securities and Exchange Commission logo" class="w3-card-4" />
                <p>This is NOT an official SEC app.</p>
            </div>
            <div id="brandDiv" class="w3-col m6 w3-center">
                <h1 class="w3-xxxlarge">EDGAR Company Funding {{environmentName}}</h1>
                <div class="w3-row">
                    <div class="w3-col s4"><a target="_blank" title="Read the requirements" v-bind:href="requirementsUrl"><i class='fa fa-book w3-card-4'></i>Requirements</a></div>
                    <div class="w3-col s4"><a target="_blank" title="See the raw data" v-bind:href="rawDataUrl"><i class='fa fa-database w3-card-4'></i>Data</a></div>
                    <div class="w3-col s4"><a target="_blank" title="Test the API" v-bind:href="swaggerUrl"><i class='fa fa-globe w3-card-4'></i>API</a></div>
                </div>
            </div>
            <div id="authorDiv" class="w3-col m3">
                <p>Created by <a target="_blank" title="Get to know the author" v-bind:href="webSite">{{author}}</a></p>
                <p>Clone the <a target="_blank" title="Clone the code" v-bind:href="repo">code repository</a></p>
                <p>View the <a target="_blank" title="View the build/release pipeline" v-bind:href="pipeline">build/release pipeline</a></p>
                <p>A sample interview project for Fora Financial (www.forafinancial.com) and Soltech</p>
            </div>
        </div>

        <div class="w3-padding-24 w3-center">
            <h2>
                <div class="search-container">
                    <input id="searchInput" class="w3-border w3-round-xlarge w3-padding-14" type="text" @input="filter($event.target.value);" placeholder="Company" />

                    <!-- Search icon -->
                    <div class="search-icon">
                        <i class="fa fa-search"></i>
                    </div>

                </div>
            </h2>
        </div>

    </header>

    <main>
        <EdgarCompanyList ref="edgarCompanyListComponent" :url="companiesUrl"/>
    </main>

    <footer class="w3-container w3-theme-dark w3-padding-16 w3-center">
        <h3>Created with ASP.NET Core {{aspnetVersion}}, Node 18.19.1, and Vue {{vueVersion}}</h3>
        <p className="w3-tiny">&copy; {{copyrightYear}}. All rights reserved.</p>
    </footer>

</template>

<script lang="ts">
    import { defineComponent } from 'vue';
    
    type Forecasts = {
        date: string,
        temperatureC: string,
        temperatureF: string,
        summary: string
    }[];

    interface Data {
        baseUrl: string,
        requirementsUrl: string,
        rawDataUrl: string,
        swaggerUrl: string,
        companiesUrl: string,
        webSite: string,
        repo: string,
        pipeline: string,
        author: string,
        aspnetVersion: string,
        vueVersion: string,
        environmentName: string,
        copyrightYear: number
    }

    export default defineComponent({
        data(): Data {
            return {
                baseUrl: import.meta.env.VITE_API_URL,
                requirementsUrl: '',
                rawDataUrl: '',
                swaggerUrl: '',
                companiesUrl: '',
                copyrightYear: new Date().getFullYear(),
                author: '',
                pipeline: '',
                repo: '',
                aspnetVersion: '',
                vueVersion: '3.3.4',
                environmentName: ''
            };
        },
        created() {
            // fetch the data when the view is created and the data is
            // already being observed
            this.fetchData();
        },
        watch: {
            // call again the method if the route changes
            '$route': 'fetchData'
        },
        methods: {
            filter(name: string): void {
                this.$refs.edgarCompanyListComponent.filter(name);
            },
            fetchData(): void {
                this.author = `Is ${this.baseUrl} running??`;
                this.webSite = '';
                this.repo = '';
                this.pipeline = '';
                this.aspnetVersion = '`Is ${this.baseUrl} running??`';
                this.requirementsUrl = `${this.baseUrl}/Fora%20Coding%20Challenge%20v1.1.pdf`;
                this.rawDataUrl = `${this.baseUrl}/api/edgar/json`;
                this.swaggerUrl = `${this.baseUrl}/swagger`;
                this.companiesUrl = `${this.baseUrl}/api/edgar/companies`;

                const env = import.meta.env.MODE;

                switch (env.toLowerCase()) {
                    case "development":
                    case "staging": {
                        this.environmentName = `- (${env})`;
                        break;
                    }
                }

                fetch(`${this.baseUrl}/api/edgar/aspnetVersion`)
                    .then(r => r.text())
                    .then(result => {
                        this.aspnetVersion = result;
                        return;
                    });

                fetch(`${this.baseUrl}/api/edgar/author`)
                    .then(r => r.text())
                    .then(result => {
                        this.author = result;
                        return;
                    });

                fetch(`${this.baseUrl}/api/edgar/website`)
                    .then(r => r.text())
                    .then(result => {
                        this.webSite = result;
                        return;
                    });

                fetch(`${this.baseUrl}/api/edgar/repo`)
                    .then(r => r.text())
                    .then(result => {
                        this.repo = result;
                        return;
                    });

                fetch(`${this.baseUrl}/api/edgar/pipeline`)
                    .then(r => r.text())
                    .then(result => {
                        this.pipeline = result;
                        return;
                    });
            },
        },
    });
</script>

<style scoped>
    #headingDiv {
        width: 100%;
    }

    #headingDiv i {
        padding: 10px;
        font-size: 36px
    }

    #searchInput {
        width: auto;
        min-width: 300px;
        text-align: center;
    }

    #authorDiv {
    }

    /* Style the container to include relative positioning */
    .search-container {
        position: relative;
        display: inline-block;
    }

    /* Style the search icon */
    .search-icon {
        position: absolute;
        top: 50%;
        right: 10px;
        transform: translateY(-50%);
    }
</style>
