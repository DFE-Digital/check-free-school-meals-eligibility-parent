describe('Log in as schools whose LA has set that they can or cannot review evidence', () => {

    it('shows review tiles for schools whose LA has the flag enabled', () => {
        cy.checkSession('school');
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.get('.govuk-caption-l').should('include.text', 'The Telford Park School');
        cy.get('h1').should('include.text', 'Manage eligibility for free school meals');
        cy.contains('a', 'Pending applications').should('be.visible');
        cy.contains('a', 'Finalise applications').should('be.visible');
        cy.contains('a', 'Guidance for reviewing evidence')
            .should('be.visible')
            .and('have.attr', 'href')
            .and('include', '/Home/Guidance');
    });

    it('does not show review tiles for schools whose LA has the flag disabled', () => {
        cy.checkSession('schoolCanReviewEvidenceDisabled');
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.wait(1);
        cy.get('.govuk-caption-l').should('include.text', 'The Astley Cooper School');
        cy.get('h1').should('include.text', 'Manage eligibility for free school meals');
        cy.contains('a', 'Pending applications').should('not.exist');
        cy.contains('a', 'Finalise applications').should('not.exist');
        cy.contains('a', 'Guidance for reviewing evidence').should('not.exist');
    });
});