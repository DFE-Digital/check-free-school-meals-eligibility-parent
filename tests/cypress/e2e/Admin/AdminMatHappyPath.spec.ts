describe('Full journey of creating an application through school portal through to approving in MAT portal', () => {
    const parentFirstName = 'Tim';
    let referenceNumber: string;
    const parentLastName = Cypress.env('lastName');
    const parentEmailAddress = 'TimJones@Example.com';
    const NIN = 'PN668767B'
    const childFirstName = 'Timmy';
    const childLastName = 'Smith';

it('Will allow an MAT user to create an application is eligible and submit an application', () => {
        cy.checkSession('MAT');
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.get('h1').should('include.text', 'THOMAS TELFORD MULTI ACADEMY TRUST');
        
        //Assert that MAT dashboard has same actions as LA dashboard
        cy.contains('Run a check for one parent or guardian');
        cy.contains('Run a batch check');
        cy.contains('Pending applications');
        cy.contains('Search all records');
        cy.contains('Guidance for reviewing evidence');

        //Future work should extend to perform check

    });
})