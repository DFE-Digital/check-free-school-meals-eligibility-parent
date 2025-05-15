
import { GOV_UK_ONE_LOGIN_SITE, GOV_UK_ONE_LOGIN_URL } from "../../support/constants";

describe('After errors have been input initially a Parent with valid details can complete full Eligibility check and application', () => {
    it('Parent can make the full journey using correct details after correcting issues in child details', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');

        cy.contains('Start now').click()
        cy.get('input.govuk-radios__input[value="true"]').check();
        cy.contains('Continue').click();
        cy.url().should('include', '/Check/Enter_Details');
        cy.get('h1').should('include.text', 'Run a check for one parent or guardian');

        cy.get('#FirstName').should('be.visible').type('Tim');
        cy.get('#LastName').should('be.visible').type('Smith');
        cy.get('#DateOfBirth\\.Day').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Month').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Year').should('be.visible').type('1990');

        cy.get('#IsNinoSelected').click();

        cy.get('#NationalInsuranceNumber').should('be.visible').type('NN668767B');

        cy.contains('Save and continue').click();
        cy.url().should('include', '/Check/Loader');

        cy.get('h1',{timeout: 60000}).should('include.text', 'Apply for free school meals for your children');



        const authorizationHeader: string= Cypress.env('AUTHORIZATION_HEADER');
        cy.intercept('GET', `${GOV_UK_ONE_LOGIN_SITE}/**`, (req) => {
            req.headers['Authorization'] = authorizationHeader;
        }).as('interceptForGET');

        cy.contains('Continue to GOV.UK One Login',{ timeout: 60000 }).click();

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

            cy.contains('Sign in',{ timeout: 60000 }).click();

            cy.get('input[name=email]').type(Cypress.env('ONEGOV_EMAIL'));
            cy.contains('Continue').click();

            cy.get('input[name=password]').type(Cypress.env('ONEGOV_PASSWORD'));
            cy.contains('Continue').click();

        });

        cy.url().should('include', '/Check/Enter_Child_Details');
        cy.get('h1').should('include.text', 'Add details of your children');

         //Blank fields
        cy.contains('Save and continue').click();

        cy.get('h2').should('contain.text', 'There is a problem');

        cy.get('li').should('contain.text', "Enter a first name for child");
        cy.get('li').should('contain.text', "Enter a last name for child");
        cy.get('li').should('contain.text', 'Select a school for child');
        cy.get('li').should('contain.text', 'Enter a date of birth for child');


        cy.get('[id="ChildList[0].FirstName"]').type('Timmy');
        cy.get('[id="ChildList[0].LastName"]').type('Smith');
        cy.get('[id="ChildList[0].School"]').type('Hinde House 2-16 Academy');

        cy.get('#schoolList0', {timeout: 5000})
            .contains('Hinde House 2-16 Academy, 139856, S5 6AG, Sheffield')
            .click({ force: true})

        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');

        cy.contains('Save and continue').click();

        cy.get('h1',{ timeout: 15000 }).should('contain.text', 'Check your answers before sending');

        cy.get('h2').should('contain.text', 'Parent or guardian details')

        cy.CheckValuesInSummaryCard('Parent or guardian details','Name', 'Tim Smith');
        cy.CheckValuesInSummaryCard('Parent or guardian details','Date of birth', '01/01/1990');
        cy.CheckValuesInSummaryCard('Parent or guardian details','National Insurance number', 'NN668767B');
        cy.CheckValuesInSummaryCard('Parent or guardian details','Email address', (Cypress.env('ONEGOV_EMAIL')));

        cy.CheckValuesInSummaryCard('Child 1','Name', 'Timmy Smith');
        cy.CheckValuesInSummaryCard('Child 1','School', 'Hinde House 2-16 Academy');
        cy.CheckValuesInSummaryCard('Child 1','Date of birth', '01/01/2007');

        cy.contains('Confirm details and send application').click();

        cy.url().should('include', '/Check/Application_Sent');
        cy.get('h1').should('contain.text', 'Application and evidence sent');

        cy.get('.govuk-table__header').should('contain.text', 'Timmy Smith');
        
        cy.get('.govuk-table__cell').should('contain.text', 'Hinde House 2-16 Academy');

    });
});
describe('Parent with valid details can complete full Eligibility check and application', () => {

    

    it('Parent can enter an NI, get an error, then', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');

        cy.contains('Start now').click()
        cy.get('input.govuk-radios__input[value="true"]').check();
        cy.contains('Continue').click();

        cy.url().should('include', '/Check/Enter_Details');

        cy.get('h1').should('include.text', 'Run a check for one parent or guardian');

        cy.get('#FirstName').should('be.visible').type('Tim');
        cy.get('#DateOfBirth\\.Day').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Month').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Year').should('be.visible').type('1990');

        cy.get('#IsNinoSelected').click();

        cy.get('#NationalInsuranceNumber').should('be.visible').type('NN123456C');

        cy.contains('Save and continue').click();

        cy.get('.govuk-error-message').should('contain', 'Enter a last name');

        cy.get('#LastName').should('be.visible').type("Simpson");
        cy.get('input[type="radio"][value="false"]').click();
        cy.contains('Save and continue').click();

        cy.get('h1').should('include.text', 'Do you have an asylum support reference number?');
        cy.get('#IsNinoSelected').filter('[value="true"]').click();
        cy.get('#NationalAsylumSeekerServiceNumber').should('be.visible').type('240712349');
        cy.contains('Save and continue').click();
        cy.get('h1',{timeout: 60000}).should('include.text', 'Apply for free school meals for your children');

    });
});