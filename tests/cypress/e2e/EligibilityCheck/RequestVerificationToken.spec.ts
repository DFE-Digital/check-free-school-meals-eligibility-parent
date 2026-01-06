describe('CSRF token manipulation', () => {

    const schoolApprovedForPrivateBeta = "Kilmorie Primary School, 100718, SE23 2SP, Lewisham";
    const schoolApprovedForPrivateBetaSearchString = "Kilmorie Primary";

    it('Will return Bad Request if request verification token is modified', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start now').click();

        cy.get('[id="SelectedSchoolURN"]').type(schoolApprovedForPrivateBetaSearchString);
        cy.get('#schoolListResults', {timeout: 5000})
            .contains(schoolApprovedForPrivateBeta)
            .click({ force: true})
        cy.contains('Continue').click();

        cy.url().should('include', '/Home/SchoolInPrivateBeta');
        cy.get('h1').should('include.text', 'You can use this test service');
        cy.contains('Check your eligibility').click();

        cy.url().should('include', '/Check/Enter_Details')
        cy.intercept('POST', Cypress.config().baseUrl + '/Check/Enter_Details').as('post')

        cy.get('h1').should('include.text', 'Enter your details');
        cy.get('#FirstName').type('Tim');
        cy.get('#LastName').type('TESTER');
        cy.get('#DateOfBirth\\.Day').type('01');
        cy.get('#DateOfBirth\\.Month').type('01');
        cy.get('#DateOfBirth\\.Year').type('1990');
        cy.get('#IsNinoSelected').click();

        cy.get('#NationalInsuranceNumber').type('PN668767B');
        cy.get('form').within(() => {
            cy.get('input[name="__RequestVerificationToken"]').then(elem => {
                elem.val('invalid token');
            });;
        });
        cy.contains('Save and continue').click();
        
        cy.wait('@post').then(interception => {
            expect(interception.response?.statusCode).to.eq(400);
        })
    });
});