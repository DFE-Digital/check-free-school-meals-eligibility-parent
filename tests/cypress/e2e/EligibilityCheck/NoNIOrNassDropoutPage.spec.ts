describe('Parent or Guardian without an NI or NASS will be redirected to correct page', () => {

    const schoolApprovedForPrivateBeta = "Kilmorie Primary School, 100718, SE23 2SP, Lewisham";
    const schoolApprovedForPrivateBetaSearchString = "Kilmorie Primary";

    it('Will redirect the parent or guardian to the correct dropout page if no NI or NASS is given', () => {

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