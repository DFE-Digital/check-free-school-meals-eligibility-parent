import { GOV_UK_ONE_LOGIN_SITE, GOV_UK_ONE_LOGIN_URL } from "../../support/constants";

let schoolApprovedForPrivateBeta = "Kilmorie Primary School, 100718, SE23 2SP, Lewisham";
let schoolApprovedForPrivateBetaSearchString = "Kilmorie Primary";

describe('Parent with valid NASS number can complete full Eligibility check and application', () => {

    let lastName = Cypress.env('lastName');

    it('Parent can make the full journey using correct details', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start now').click()

        cy.get('[id="SelectedSchoolURN"]').type(schoolApprovedForPrivateBetaSearchString);
        cy.get('#schoolListResults', {timeout: 5000})
            .contains(schoolApprovedForPrivateBeta)
            .click({ force: true})
        cy.contains('Continue').click();

        cy.url().should('include', '/Home/SchoolInPrivateBeta');
        cy.get('h1').should('include.text', 'You can use this test service');
        cy.contains('Check your eligibility').click();

        cy.url().should('include', '/Check/Enter_Details');
        cy.get('h1').should('include.text', 'Enter your details');
        cy.get('#FirstName').should('be.visible').type('Tim');
        cy.get('#LastName').should('be.visible').type('TESTER');
        cy.get('#DateOfBirth\\.Day').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Month').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Year').should('be.visible').type('1980');
        cy.get('input[type="radio"][value="false"]').click();
        cy.contains('Save and continue').click();

        cy.get('h1').should('include.text', 'Do you have an asylum support reference number?');
        cy.get('#IsNinoSelected').click();
        cy.get('#NationalAsylumSeekerServiceNumber').type('110111111')
        cy.contains('Save and continue').click();

        cy.get('h1',{ timeout: 60000 }).should('include.text', 'Apply for free school meals for your children');
        const authorizationHeader: string= Cypress.env('AUTHORIZATION_HEADER');
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


        cy.get('[id="ChildList[0].FirstName"]').type('Tim');
        cy.get('[id="ChildList[0].LastName"]').type('TESTER');
        cy.get('[id="ChildList[0].School"]').type(schoolApprovedForPrivateBetaSearchString);
        cy.get('#schoolList0')
            .contains(schoolApprovedForPrivateBeta)
            .click({ force: true})
        cy.get('[id="ChildList[0].DateOfBirth.Day"]').type('01');
        cy.get('[id="ChildList[0].DateOfBirth.Month"]').type('01');
        cy.get('[id="ChildList[0].DateOfBirth.Year"]').type('2007');
        cy.contains('Save and continue').click();

        cy.get('h1',{ timeout: 15000 }).should('contain.text', 'Check your answers before sending');
        cy.CheckValuesInSummaryCard('Parent or guardian details','Name', 'TESTER');
        cy.CheckValuesInSummaryCard('Parent or guardian details','Date of birth', '01/01/1980');
        cy.CheckValuesInSummaryCard('Parent or guardian details','Asylum support reference number', '110111111');
        cy.CheckValuesInSummaryCard('Parent or guardian details','Email address', (Cypress.env('ONEGOV_EMAIL')));

        cy.CheckValuesInSummaryCard('Child 1','Name', 'Tim TESTER');
        cy.CheckValuesInSummaryCard('Child 1','School', schoolApprovedForPrivateBetaSearchString);
        cy.CheckValuesInSummaryCard('Child 1','Date of birth', '01/01/2007');
        cy.contains('Confirm details and send application').click();

        cy.url().should('include', '/Check/Application_Sent');
        cy.get('h1').should('contain.text', 'Application and evidence sent');
        cy.get('.govuk-table__header').should('contain.text', 'TESTER');
        cy.get('.govuk-table__cell').should('contain.text', schoolApprovedForPrivateBetaSearchString);
    });

    it('Parent can make the full journey for two children using correct details', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start now').click()

        cy.get('[id="SelectedSchoolURN"]').type(schoolApprovedForPrivateBetaSearchString);
        cy.get('#schoolListResults', {timeout: 5000})
            .contains(schoolApprovedForPrivateBeta)
            .click({ force: true})
        cy.contains('Continue').click();

        cy.url().should('include', '/Home/SchoolInPrivateBeta');
        cy.get('h1').should('include.text', 'You can use this test service');
        cy.contains('Check your eligibility').click();

        cy.url().should('include', '/Check/Enter_Details');
        cy.get('h1').should('include.text', 'Enter your details');
        cy.get('#FirstName').should('be.visible').type('Tim');
        cy.get('#LastName').should('be.visible').type('TESTER');
        cy.get('#DateOfBirth\\.Day').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Month').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Year').should('be.visible').type('1980');
        cy.get('input[type="radio"][value="false"]').click();
        cy.contains('Save and continue').click();

        cy.get('h1').should('include.text', 'Do you have an asylum support reference number?');
        cy.get('#IsNinoSelected').click();

        cy.get('#NationalAsylumSeekerServiceNumber').type('110111111')
        cy.contains('Save and continue').click();

        cy.get('h1',{ timeout: 60000 }).should('include.text', 'Apply for free school meals for your children');
        const authorizationHeader: string= Cypress.env('AUTHORIZATION_HEADER');
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


        cy.get('[id="ChildList[0].FirstName"]').type('Tim');
        cy.get('[id="ChildList[0].LastName"]').type('TESTER');
        cy.get('[id="ChildList[0].School"]').type(schoolApprovedForPrivateBetaSearchString);

        cy.get('#schoolList0')
            .should('be.visible')
            .contains(schoolApprovedForPrivateBeta)
            .click({ force: true})

        cy.get('[id="ChildList[0].DateOfBirth.Day"]').type('01');
        cy.get('[id="ChildList[0].DateOfBirth.Month"]').type('01');
        cy.get('[id="ChildList[0].DateOfBirth.Year"]').type('2007');
        cy.contains("Add another child").click();

        cy.get('[id="ChildList[1].FirstName"]').type('Tom');
        cy.get('[id="ChildList[1].LastName"]').type('Ljungqvist');
        cy.get('[id="ChildList[1].School"]').type(schoolApprovedForPrivateBetaSearchString);

        cy.get('#schoolList1')
            .should('be.visible')
            .contains(schoolApprovedForPrivateBeta)
            .click({ force: true})

        cy.get('[id="ChildList[1].DateOfBirth.Day"]').type('21');
        cy.get('[id="ChildList[1].DateOfBirth.Month"]').type('08');
        cy.get('[id="ChildList[1].DateOfBirth.Year"]').type('2014');
        cy.contains('Save and continue').click();

        cy.get('h1',{ timeout: 15000 }).should('contain.text', 'Check your answers before sending');
        cy.CheckValuesInSummaryCard('Parent or guardian details','Name', 'TESTER');
        cy.CheckValuesInSummaryCard('Parent or guardian details','Date of birth', '01/01/1980');
        cy.CheckValuesInSummaryCard('Parent or guardian details','Asylum support reference number', '110111111');
        cy.CheckValuesInSummaryCard('Parent or guardian details','Email address', (Cypress.env('ONEGOV_EMAIL')));

        cy.CheckValuesInSummaryCard('Child 1','Name', 'Tim TESTER');
        cy.CheckValuesInSummaryCard('Child 1','School', schoolApprovedForPrivateBetaSearchString);
        cy.CheckValuesInSummaryCard('Child 1','Date of birth', '01/01/2007');
        cy.contains('Confirm details and send application').click();

        cy.url().should('include', '/Check/Application_Sent');
        cy.get('h1').should('contain.text', 'Application and evidence sent');
        cy.get('.govuk-table__header').should('contain.text', 'TESTER');
        cy.get('.govuk-table__cell').should('contain.text', schoolApprovedForPrivateBetaSearchString);
    });
});