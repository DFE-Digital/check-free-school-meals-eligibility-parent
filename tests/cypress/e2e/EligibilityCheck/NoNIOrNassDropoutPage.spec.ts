describe('Parent or Guardian without an NI or NASS will be redirected to correct page', () => {

    const schoolApprovedForPrivateBeta = "Kilmorie Primary School, 100718, SE23 2SP, Lewisham";
    const schoolApprovedForPrivateBetaSearchString = "Kilmorie Primary";

    it('Will redirect the parent or guardian to the correct dropout page if no NI or NASS is given', () => {
        cy.completePrivateBetaSchoolCheck();
        cy.get('#FirstName').should('be.visible').type('Tim');
        cy.get('#LastName').should('be.visible').type('GRIFFIN');
        cy.get('#DateOfBirth\\.Day').should('be.visible').type('31');
        cy.get('#DateOfBirth\\.Month').should('be.visible').type('12');
        cy.get('#DateOfBirth\\.Year').should('be.visible').type('2000');

        cy.get('input[type="radio"][value="false"]').click();
        cy.contains('Save and continue').click();

        cy.get('h1').should('include.text', 'Do you have an asylum support reference number?');
        cy.get('input[type="radio"][value="false"]').click();
        cy.contains('Save and continue').click();

        cy.get('.govuk-grid-column-full').find('h1').should('include.text', "We could not check your children’s entitlement to free school meals")
    })
})