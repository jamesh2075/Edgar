<template>
    <p v-if="!companies"><em>Loading... Please refresh after a few seconds. See <a href="https://aka.ms/jspsintegrationangular">https://aka.ms/jspsintegrationangular</a> for more details.</em></p>

    <div class="w3-responsive">
        <table v-if="companies" class="w3-table-striped">
            <thead>
                <tr class="w3-theme-dark">
                    <th><span @click="sort('id')">CIK</span></th>
                    <th><span @click="sort('name')">Name</span></th>
                    <th><span @click="sort('standardFundableAmount')">Standard Fundable Amount</span></th>
                    <th><span @click="sort('specialFundableAmount')">Special Fundable Amount</span></th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="company in companies" :key="company.id">
                    <td>{{ company.id }}</td>
                    <td>{{ company.name }}</td>
                    <td>{{ company.standardFundableAmount.toLocaleString("en-US", { style: "currency", currency: "USD" }) }}</td>
                    <td>{{ company.specialFundableAmount.toLocaleString("en-US", { style: "currency", currency: "USD" }) }}</td>
                </tr>
            </tbody>
        </table>
    </div>
</template>

<script lang="ts">
    import { defineComponent } from 'vue';

    type Company = {
        id: number,
        name: string,
        standardFundableAmount: number,
        specialFundableAmount: number
    }

    type Companies = Company[]

    interface Data {
        loading: boolean,
        companies: null | Companies,
        ascending: boolean,
        previousField: string
    }

    export default defineComponent({
        data(): Data {
            return {
                loading: false,
                companies: null,
                ascending: false,
                previousField: ''
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
            fetchData(): void {
                this.companies = null;
                this.loading = true;

                fetch('https://edgarapi.azurewebsites.net/api/edgar/companies')
                    .then(response => response.json())
                    .then(json => {
                        this.companies = json as Companies;
                        this.loading = false;
                        return;
                    }).catch(error => console.log(error));
            },
            sort(field:string): void {
                if (field === this.previousField) {
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
                    const objA = JSON.parse(JSON.stringify(a));
                    const objB = JSON.parse(JSON.stringify(b));

                    // Create the sort algorithm.
                    // Return 1 if the first item is greater than the second.
                    // Return -1 if the second item is greater than the first.
                    // Return 0 if the two items are equal
                    const greater = this.ascending ? 1 : -1;
                    const lesser = this.ascending ? -1 : 1;
                    let result = 0;
                    if (field === "name") {
                        result =
                            objA[field].toLowerCase() > objB[field].toLowerCase() ? greater :
                                objA[field].toLowerCase() < objB[field].toLowerCase() ? lesser : 0;
                    }
                    else {
                        result =
                            objA[field] > objB[field] ? greater :
                                objA[field] < objB[field] ? lesser : 0;
                    }

                    return result;
                });

            },
            filter(search:string) {
                let suffix = search === undefined || search.trim() === '' ? '' : `/${search}`;

                let url = `https://edgarapi.azurewebsites.net/api/edgar/companies${suffix}`;

                fetch(url, { method: 'Get', mode: "cors" })
                    .then(response => response.json())
                    .then(json => this.companies = json as Companies)
                    .catch(error => console.error(error))
            }
        },
    });
</script>

<style scoped>
/* 
Generic Styling, for Desktops/Laptops 
*/

table {
    width: 100%;
    border-collapse: collapse;
    table-layout: fixed;
    margin: 0 auto;
}
/* Zebra striping */
tr:nth-of-type(odd) {
    background: #eee;
}

th {
    background: #333;
    color: white;
    font-weight: bold;
}

th span {
    text-decoration-line: underline;
    cursor: pointer;
}

td, th {
    padding: 6px;
    border: 1px solid #ccc;
    text-align: left;
}
    /* 
Max width before this PARTICULAR table gets nasty
This query will take effect for any screen smaller than 760px
and also iPads specifically.
*/
@media only screen and (max-width: 760px), (min-device-width: 768px) and (max-device-width: 1024px) {

    /* Force table to not be like tables anymore */
    table, thead, tbody, th, td, tr {
        display: block;
    }

    /* Hide table headers (but not display: none;, for accessibility) */
    thead tr {
        position: absolute;
        top: -9999px;
        left: -9999px;
    }

    tr {
        border: 1px solid #ccc;
    }

    td {
        /* Behave  like a "row" */
        border: none;
        border-bottom: 1px solid #eee;
        position: relative;
        padding-left: 50%;
    }

    td:before {
        /* Now like a table header */
        position: absolute;
        /* Top/left values mimic padding */
        top: 6px;
        left: 6px;
        width: 45%;
        padding-right: 10px;
        white-space: nowrap;
    }

    /*
	Label the data
	*/
    td:nth-of-type(1):before {
        content: "CIK";
    }

    td:nth-of-type(2):before {
        content: "Name";
    }

    td:nth-of-type(3):before {
        content: "Standard Fundable Amount";
    }

    td:nth-of-type(4):before {
        content: "Special Fundable Amount";
    }
}

</style>