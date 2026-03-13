describe('BasicLAHappyPath', () => {
    let skipSetupBasic = false
    const parentFirstName = 'Tim';
    let referenceNumber: string;
    const parentLastName = Cypress.env('lastName');
    const NIN = 'NN668767B'
    beforeEach(() => {
        if (!skipSetupBasic) {
            cy.checkSession('basic');
            cy.visit(Cypress.config().baseUrl ?? "");
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
});
