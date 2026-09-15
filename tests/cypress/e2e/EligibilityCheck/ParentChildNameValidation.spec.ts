import { GOV_UK_ONE_LOGIN_SITE, GOV_UK_ONE_LOGIN_URL } from "../../support/constants";

let schoolApprovedForPrivateBeta = "Kilmorie Primary School, 100718, SE23 2SP, Lewisham";
let schoolApprovedForPrivateBetaSearchString = "Kilmorie Primary";

describe('Test that approved accented characters are accepted in name input fields', () => {

    it('Parent first and last names on Enter_Details should accept approved accented characters', () => {
        cy.completePrivateBetaSchoolCheck();
        let approvedChars = "OBrien" + //plain letters
            "O'Brien" + //straight apostrophe (U+0027)
            "O\u2019Brien" + //right curly apostrophe (U+2019)
            "O\u2018Brien" + //left curly apostrophe (U+2018)
            "Smith-Jones" + //hyphen
            "St. Claire" + //period and space
            "van den Berg" + //spaces
            "ÁáÉéÍíÓóÚúÝýĆćĹĺŃńŔŕŚśŹź" + //acute
            "ÀàÈèÌìÒòÙùẀẁỲỳ" + //grave
            "ÂâÊêÎîÔôÛûĈĉĜĝĤĥĴĵŜŝŴŵŶŷ" + //circumflex
            "ÃãÑñÕõĨĩŨũẼẽỸỹ" + //tilde
            "ÄäËëÏïÖöÜüŸÿ" + //umlaut or diaeresis
            "ÇçĢģĶķĻļŅņŖŗŞşŢţ" + //cedilla
            "ÅåŮů" + //ring
            "ĀāĒēĪīŌōŪūȲȳ" + //macron
            "ĂăĔĕĞğĬĭŎŏŬŭ" + //breve
            "ĊċĖėĠġİẊẋŻż" + //dot above
            "ĄąĘęĮįŲų" + //ogonek
            "ŐőŰű"; //double acute

        // Test the validation for First name and Last name accept the DWP predefined list of approved characters 
        cy.get('#FirstName').should('be.visible').type(approvedChars);
        cy.get('#LastName').should('be.visible').type(approvedChars);

        cy.contains('Save and continue').click();
        cy.get('#error-summary')
            .should('not.contain.text', 'Enter a first name with valid characters')
            .and('not.contain.text', 'Enter a last name with valid characters');
        //Verify that we did successfully submit the form because we received validation errors for the two unfilled inputs
        cy.get('#error-summary')
            .should('contain.text', 'Enter a date of birth')
            .and('contain.text', 'Select yes if you have a National Insurance number');

        //Continue to Add_Child_Details to check the Child Name validation with valid Parent Details
        //Replace Last name as we need to use 'Tester' for the check to proceed.
        cy.get('#LastName').should('be.visible').clear().type('TESTER');
        cy.get('#DateOfBirth\\.Day').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Month').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Year').should('be.visible').type('1980');
        cy.get('#IsNinoSelected').click();
        cy.get('#NationalInsuranceNumber').type('NN123456C')
        cy.contains('Save and continue').click();

        cy.get('h1', { timeout: 60000 }).should('include.text', 'Apply for free school meals for your children');
        const authorizationHeader: string = Cypress.env('AUTHORIZATION_HEADER');
        cy.intercept('GET', `${GOV_UK_ONE_LOGIN_SITE}/**`, (req) => {
            req.headers['Authorization'] = authorizationHeader;
        }).as('interceptForGET');
        cy.contains('Continue to GOV.UK One Login', { timeout: 60000 }).click();

        cy.origin(GOV_UK_ONE_LOGIN_URL, () => {
            let currentUrl = "";
            cy.url().then((url) => {
                currentUrl = url;
            });
            cy.wait(2000);

            cy.visit(currentUrl, {
                auth: {
                    username: Cypress.env('AUTH_USERNAME'),
                    password: Cypress.env('AUTH_PASSWORD')
                },
            });

            cy.wait(2000);

            cy.contains('Sign in').click();

            cy.log(":)");

            cy.get('input[name=email]').type(Cypress.env('ONEGOV_EMAIL'));
            cy.contains('Continue').click();

            cy.log(":(");

            cy.get('input[name=password]').type(Cypress.env('ONEGOV_PASSWORD'));
            cy.contains('Continue').click();

            // Check for updated terms page and handle it if present
            cy.url().then(url => {
                if (url.includes('updated-terms-and-conditions')) {
                    cy.log('Updated terms page detected');
                    cy.contains('Continue').click();
                }
            });
        });

        cy.wait(2000);
        cy.url().should('include', '/Check/Enter_Child_Details');
        cy.get('h1').should('include.text', 'Add details of your children');


        cy.get('[id="ChildList[0].FirstName"]').type(approvedChars);
        cy.get('[id="ChildList[0].LastName"]').type(approvedChars);
        cy.get('[id="ChildList[0].School"]').type(schoolApprovedForPrivateBetaSearchString);
        cy.get('#schoolList0')
            .contains(schoolApprovedForPrivateBeta)
            .click({ force: true })
        cy.get('[id="ChildList[0].DateOfBirth.Day"]').type('01');
        cy.get('[id="ChildList[0].DateOfBirth.Month"]').type('01');
        cy.get('[id="ChildList[0].DateOfBirth.Year"]').type('2007');
        cy.contains('Save and continue').click();

        cy.get('h1', { timeout: 15000 }).should('contain.text', 'Check your answers before sending');
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Name', approvedChars);
        cy.CheckValuesInSummaryCard('Child 1', 'Name', approvedChars);
    });
});