describe('Eligible and Not Eligible responses in LA and School portal will route to unique pages', () => {
    const parentFirstName = 'Tim';
    const parentLastName = Cypress.env('lastName');
    const parentEmailAddress = 'TimJones@Example.com';
    const NINE = 'NN668767B'
    const NINNE = 'PN668767B'

    it('Will route to the School variant of outcome pages when logged in as School', () => {
        cy.checkSession('school');
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.get('h1').should('include.text', 'The Telford Park School');

        cy.contains('Run a check for one parent or guardian').click();
        cy.get('#consent').check();
        cy.get('#submitButton').click();
        cy.url().should('include', '/Check/Enter_Details');
        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('#EmailAddress').type(parentEmailAddress);
        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');
        cy.get('#NinAsrSelection').click();
        cy.get('#NationalInsuranceNumber').type(NINE);
        cy.contains('button', 'Perform check').click();
        cy.url().should('include', 'Check/Loader');
        cy.get('a.govuk-button', { timeout: 80000 }).should('contain.text', "Add children's details");

        cy.visit(Cypress.config().baseUrl ?? "");
        cy.get('h1').should('include.text', 'The Telford Park School');

        cy.contains('Run a check for one parent or guardian').click();
        cy.get('#consent').check();
        cy.get('#submitButton').click();
        cy.url().should('include', '/Check/Enter_Details');
        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('#EmailAddress').type(parentEmailAddress);
        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');
        cy.get('#NinAsrSelection').click();
        cy.get('#NationalInsuranceNumber').type(NINNE);
        cy.contains('button', 'Perform check').click();
        cy.url().should('include', 'Check/Loader');
        cy.get('a.govuk-button', { timeout: 80000 }).should('contain.text', "Appeal now");
    });

    it('Will route to the LA varient of outcome pages when logged in as LA', () => {
        cy.checkSession('LA');
        cy.visit(Cypress.config().baseUrl ?? "");

        cy.get('h1').should('include.text', 'Telford and Wrekin Council');

        cy.contains('Run a check for one parent or guardian').click();

        cy.url().should('include', '/Check/Enter_Details');
        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('#EmailAddress').type(parentEmailAddress);
        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');
        cy.get('#NinAsrSelection').click();
        cy.get('#NationalInsuranceNumber').type(NINE);
        cy.contains('button', 'Perform check').click();

        cy.url().should('include', 'Check/Loader');
        cy.get('a.govuk-button', { timeout: 80000 }).should('contain.text', "Do another check");

        cy.visit(Cypress.config().baseUrl ?? "");
        cy.wait(1);
        cy.get('h1').should('include.text', 'Telford and Wrekin Council');
        cy.contains('Run a check for one parent or guardian').click();
        cy.url().should('include', '/Check/Enter_Details');
        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('#EmailAddress').type(parentEmailAddress);
        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');
        cy.get('#NinAsrSelection').click();
        cy.get('#NationalInsuranceNumber').type(NINNE);
        cy.contains('button', 'Perform check').click();

        cy.url().should('include', 'Check/Loader');
        cy.get('h1.govuk-heading-l', { timeout: 80000 }).should('contain.text', "Review supporting evidence");
    });
});