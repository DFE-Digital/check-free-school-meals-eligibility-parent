import { contains } from "cypress/types/jquery";

describe('BasicLAHappyPath', () => {
    let skipSetupBasic = false
    const parentFirstName = 'Tim';
    let referenceNumber: string;
    const parentLastName = Cypress.env('lastName');
    const NIN = 'NN668767B'
    beforeEach(() => {
        if (!skipSetupBasic) {
            cy.checkSession('basic');
            cy.visit((Cypress.config().baseUrl ?? "") + "/home");
            cy.wait(1);
            cy.get('.govuk-caption-l').should('include.text', 'Manchester City Council');
        }
    });

    it('Will allow a basic user to check for eligibility that is eligible', () => {
        //Add parent details
        cy.contains('Run a check for one parent or guardian').click();
        cy.url().should('include', '/Check/Enter_Details_Basic');
        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('[id="DateOfBirth.Day"]').type('01');
        cy.get('[id="DateOfBirth.Month"]').type('01');
        cy.get('[id="DateOfBirth.Year"]').type('1990');
        cy.get('#NationalInsuranceNumber').type(NIN);
        cy.contains('button', 'Perform check').click();

        //Loader page
        cy.url().should('include', 'Check/Loader');

        //eligible outcome
        cy.get('h2.govuk-notification-banner__title', { timeout: 80000 }).should('include.text', 'Children eligible');
        cy.contains('a.govuk-button', 'Do another check');
    });

    it('Will show updated guidance when a basic user check is not eligible', () => {
        cy.checkSession('basic');
    
        cy.visit((Cypress.config().baseUrl ?? "") + "/home");
        cy.get('.govuk-caption-l').should('include.text', 'Manchester City Council');
    
        cy.contains('Run a check for one parent or guardian').click();
    
        cy.url().should('include', '/Check/Enter_Details_Basic');
    
        cy.get('#FirstName').clear().type(parentFirstName);
        cy.get('#LastName').clear().type(parentLastName);
        cy.get('[id="DateOfBirth.Day"]').clear().type('01');
        cy.get('[id="DateOfBirth.Month"]').clear().type('01');
        cy.get('[id="DateOfBirth.Year"]').clear().type('1990');
        cy.get('#NationalInsuranceNumber').clear().type('PN123456A');
    
        cy.contains('button', 'Perform check').click();
    
        cy.url({ timeout: 80000 }).should('include', '/Check/Loader');
    
        cy.get('h2.govuk-notification-banner__title', { timeout: 80000 })
            .should('contain.text', 'May not be eligible');
    
        cy.contains(
            'You can contact the relevant school to ask them to collect it from the parent or guardian.',
            { timeout: 80000 }
        ).should('be.visible');
    
        cy.contains('request a separate check').should('not.exist');
        cy.contains('Department for Education support desk').should('not.exist');
    });
    
    it('Will allow a basic user to generate a report for the last week', () => {
        cy.contains('a.dfe-card-link--header', 'Reports').click();
        cy.get('.govuk-heading-l').should('include.text', 'Report history');
        cy.contains('a.govuk-button', 'Generate report').click();
        cy.get("#StartDate\\.Day").clear().type('2');
        cy.get("#StartDate\\.Month").clear().type('12');
        cy.get("#StartDate\\.Year").clear().type('2025');
        cy.get("#EndDate\\.Day").clear().type('2');
        cy.get("#EndDate\\.Month").clear().type('3');
        cy.get("#EndDate\\.Year").clear().type('2026');
        cy.contains("button", "Generate report").click();
        cy.url({ timeout: 80000 }).should('include', '/Check/Report_Loader');
    });
});
