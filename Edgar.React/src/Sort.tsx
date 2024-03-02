import CompanyInfo from "./CompanyInfo";

let ascending: boolean = false;
let previousField: string = "";

function sort(companies:CompanyInfo[], field: string) {

    if (field === previousField) {
        ascending = !ascending; // If the same field is being sorted, reverse the sort
    }
    else {
        ascending = true; // Otherwise, sort the field in ascending order
    }

    previousField = field;

    companies.sort((a: CompanyInfo, b: CompanyInfo) => {

        // Convert the Company instances into regular JavaScript objects
        // so that their properties can be indexed by name.
        // This eliminates the need to each field (i.e. a.id, a.name, a.standardFundableAmount...)
        const objA = JSON.parse(JSON.stringify(a));
        const objB = JSON.parse(JSON.stringify(b));

        // Create the sort algorithm.
        // Return 1 if the first item is greater than the second.
        // Return -1 if the second item is greater than the first.
        // Return 0 if the two items are equal
        const greater = ascending ? 1 : -1;
        const lesser = ascending ? -1 : 1;
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
}

export default sort;