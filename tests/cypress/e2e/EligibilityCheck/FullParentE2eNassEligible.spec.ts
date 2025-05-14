import { GOV_UK_ONE_LOGIN_SITE, GOV_UK_ONE_LOGIN_URL } from "../../support/constants";

describe('Parent with valid NASS number can complete full Eligibility check and application', () => {

    let lastName = Cypress.env('lastName');

    it('Parent can make the full journey using correct details', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');

        cy.contains('Start now').click()
        cy.get('input.govuk-radios__input[value="true"]').check();
        cy.contains('Continue').click();
        
        cy.url().should('include', '/Check/Enter_Details');
        cy.get('h1').should('include.text', 'Run a check for one parent or guardian');

        cy.get('#FirstName').should('be.visible').type('Tim');
        cy.get('#LastName').should('be.visible').type('Simpson');
        cy.get('#DateOfBirth\\.Day').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Month').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Year').should('be.visible').type('1990');

        cy.get('input[type="radio"][value="false"]').click();

        cy.contains('Save and continue').click();

        cy.get('h1').should('include.text', 'Do you have an asylum support reference number?');
        cy.get('#IsNinoSelected').click();
        cy.get('#NationalAsylumSeekerServiceNumber').type('240712349')

        cy.contains('Save and continue').click();

        cy.get('h1',{ timeout: 60000 }).should('include.text', 'Apply for free school meals for your children');



        const authorizationHeader: string= Cypress.env('AUTHORIZATION_HEADER');
        cy.intercept('GET', `${GOV_UK_ONE_LOGIN_SITE}/**`, (req) => {
            req.headers['Authorization'] = authorizationHeader;
        }).as('interceptForGET');

        cy.contains('Continue to GOV.UK One Login', { timeout: 60000 }).click();
        
        cy.wait(3);
            let currentUrl = "";

            cy.url().then((url) => {
                currentUrl = url;
            });
            cy.visit(currentUrl, {
                auth: {
                    username: Cypress.env('AUTH_USERNAME'),
                    password: Cypress.env('AUTH_PASSWORD')
                },
            });

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
            });

        cy.wait(2000);
        cy.url().should('include', '/Check/Enter_Child_Details');
        cy.get('h1').should('include.text', 'Add details of your children');


        cy.get('[id="ChildList[0].FirstName"]').type('Tim');
        cy.get('[id="ChildList[0].LastName"]').type('Simpson');

        cy.get('[id="ChildList[0].School"]').type('Hinde House 2-16 Academy');
        
        cy.get('#schoolList0')
            .should('be.visible')
            .contains('Hinde House 2-16 Academy, 139856, S5 6AG, Sheffield')
            .click({ force: true})

        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');

        cy.contains('Save and continue').click();

        cy.get('h1',{ timeout: 15000 }).should('contain.text', 'Check your answers before sending');

        cy.CheckValuesInSummaryCard('Parent or guardian details','Name', 'Simpson');
        cy.CheckValuesInSummaryCard('Parent or guardian details','Date of birth', '01/01/1990');
        cy.CheckValuesInSummaryCard('Parent or guardian details','Asylum support reference number', '240712349');
        cy.CheckValuesInSummaryCard('Parent or guardian details','Email address', (Cypress.env('ONEGOV_EMAIL')));

        cy.CheckValuesInSummaryCard('Child 1','Name', 'Tim Simpson');
        cy.CheckValuesInSummaryCard('Child 1','School', 'Hinde House 2-16 Academy');
        cy.CheckValuesInSummaryCard('Child 1','Date of birth', '01/01/2007');

        cy.get('#finishedConfirmation').check();
        cy.contains('Confirm details and send application').click();

        cy.url().should('include', '/Check/Application_Sent');
        cy.get('h1').should('contain.text', 'Application and evidence sent');

        cy.get('.govuk-table__header').should('contain.text', 'Simpson');
        
        cy.get('.govuk-table__cell').should('contain.text', 'Hinde House 2-16 Academy');

    });

    it('Parent can make the full journey for two children using correct details', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');

        cy.contains('Start now').click()
        cy.get('input.govuk-radios__input[value="true"]').check();
        cy.contains('Continue').click();

        cy.url().should('include', '/Check/Enter_Details');
        cy.get('h1').should('include.text', 'Run a check for one parent or guardian');

        cy.get('#FirstName').should('be.visible').type('Tim');
        cy.get('#LastName').should('be.visible').type('Simpson');
        cy.get('#DateOfBirth\\.Day').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Month').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Year').should('be.visible').type('1990');

        cy.get('input[type="radio"][value="false"]').click();

        cy.contains('Save and continue').click();

        cy.get('h1').should('include.text', 'Do you have an asylum support reference number?');
        cy.get('#IsNinoSelected').click();
        cy.get('#NationalAsylumSeekerServiceNumber').type('240712349')

        cy.contains('Save and continue').click();

        cy.get('h1',{ timeout: 60000 }).should('include.text', 'Apply for free school meals for your children');



        const authorizationHeader: string= Cypress.env('AUTHORIZATION_HEADER');
        cy.intercept('GET', `${GOV_UK_ONE_LOGIN_SITE}/**`, (req) => {
            req.headers['Authorization'] = authorizationHeader;
        }).as('interceptForGET');

        cy.contains('Continue to GOV.UK One Login', { timeout: 60000 }).click();

        cy.wait(3);
        let currentUrl = "";

        cy.url().then((url) => {
            currentUrl = url;
        });
        cy.visit(currentUrl, {
            auth: {
                username: Cypress.env('AUTH_USERNAME'),
                password: Cypress.env('AUTH_PASSWORD')
            },
        });

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
        });

        cy.wait(2000);
        cy.url().should('include', '/Check/Enter_Child_Details');
        cy.get('h1').should('include.text', 'Add details of your children');


        cy.get('[id="ChildList[0].FirstName"]').type('Tim');
        cy.get('[id="ChildList[0].LastName"]').type('Simpson');

        cy.get('[id="ChildList[0].School"]').type('Hinde House 2-16 Academy');

        cy.get('#schoolList0')
            .should('be.visible')
            .contains('Hinde House 2-16 Academy, 139856, S5 6AG, Sheffield')
            .click({ force: true})

        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');
        
        cy.contains("Add another child").click();

        cy.get('[id="ChildList[1].FirstName"]').type('Tom');
        cy.get('[id="ChildList[1].LastName"]').type('Ljungqvist');

        cy.get('[id="ChildList[1].School"]').type('Hinde House 2-16 Academy');

        cy.get('#schoolList1')
            .should('be.visible')
            .contains('Hinde House 2-16 Academy, 139856, S5 6AG, Sheffield')
            .click({ force: true})

        cy.get('[id="ChildList[1].Day"]').type('21');
        cy.get('[id="ChildList[1].Month"]').type('08');
        cy.get('[id="ChildList[1].Year"]').type('2014');

        cy.contains('Save and continue').click();

        cy.get('h1',{ timeout: 15000 }).should('contain.text', 'Check your answers before sending');

        cy.CheckValuesInSummaryCard('Parent or guardian details','Name', 'Simpson');
        cy.CheckValuesInSummaryCard('Parent or guardian details','Date of birth', '01/01/1990');
        cy.CheckValuesInSummaryCard('Parent or guardian details','Asylum support reference number', '240712349');
        cy.CheckValuesInSummaryCard('Parent or guardian details','Email address', (Cypress.env('ONEGOV_EMAIL')));

        cy.CheckValuesInSummaryCard('Child 1','Name', 'Tim Simpson');
        cy.CheckValuesInSummaryCard('Child 1','School', 'Hinde House 2-16 Academy');
        cy.CheckValuesInSummaryCard('Child 1','Date of birth', '01/01/2007');

        cy.get('#finishedConfirmation').check();
        cy.contains('Confirm details and send application').click();

        cy.url().should('include', '/Check/Application_Sent');
        cy.get('h1').should('contain.text', 'Application and evidence sent');

        cy.get('.govuk-table__header').should('contain.text', 'Simpson');

        cy.get('.govuk-table__cell').should('contain.text', 'Hinde House 2-16 Academy');

    });
});