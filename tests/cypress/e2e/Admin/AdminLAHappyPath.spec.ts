describe('Full journey of creating an application through school portal through to approving in LA portal', () => {
    const parentFirstName = 'Tim';
    let referenceNumber: string;
    const parentLastName = Cypress.env('lastName');
    const parentEmailAddress = 'TimJones@Example.com';
    const NIN = 'PN668767B'
    const childFirstName = 'Timmy';
    const childLastName = 'Smith';

it('Will allow an LA user to create an application is eligible and submit an application', () => {
        cy.checkSession('LA');
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.get('h1').should('include.text', 'Telford and Wrekin Council');
        //Add parent details
        cy.contains('Run a check for one parent or guardian').click();
        cy.get('#submitButton').click();
        cy.url().should('include', '/Check/Enter_Details');
        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('#EmailAddress').type(parentEmailAddress);
        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');
        cy.get('#NinAsrSelection').click();
        cy.get('#NationalInsuranceNumber').type("nn123456c");
        cy.contains('button', 'Perform check').click();

        //Loader page
        cy.url().should('include', 'Check/Loader');

        //Eligible outcome page
        cy.get('.govuk-body', { timeout: 80000 }).should('include.text', 'This information should be passed on to their school.');
        cy.get('a.govuk-link').contains('Continue to add child details').click();
        //Enter child details
        cy.url().should('include', '/Enter_Child_Details');
        cy.get('[id="ChildList[0].FirstName"]').type(childFirstName);
        cy.get('[id="ChildList[0].LastName"]').type(childLastName);
        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');
        cy.get('[id="ChildList[0].School"]').type('Hinde House 2-16 Academy');

        cy.get('#schoolList0')
            .should('be.visible')
            .contains('Hinde House 2-16 Academy, 139856, S5 6AG, Sheffield')
            .click({ force: true });
             cy.contains('button', 'Save and continue').click();

        //Check answers page
        cy.get('h1').should('include.text', 'Check your answers before submitting');
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Name', `${parentFirstName} ${parentLastName}`);
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Date of birth', '1 January 1990');
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'National Insurance number', "NN123456C");
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Email address', parentEmailAddress);
        cy.CheckValuesInSummaryCard('Child 1 details', "Name", childFirstName + " " + childLastName);
        cy.contains('button', 'Add details').click();

        //Applications Registered confirmation page
        cy.url().should('include', '/Check/ApplicationsRegistered');
    });
})