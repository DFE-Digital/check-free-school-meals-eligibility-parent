import { includes } from "cypress/types/lodash";

describe('SchoolCanReviewEvidence dashboard tile visibility', () => {

    it('shows review tiles for non-MAT schools whose LA has the flag enabled', () => {
        cy.checkSession('schoolNonMatFlagOn');
        cy.visit((Cypress.config().baseUrl ?? "") + "/home");

        cy.get('.govuk-caption-l').should('include.text', 'The Astley Cooper School');
        cy.get('h1').should('include.text', 'Manage eligibility for free school meals');
        cy.contains('a', 'Pending applications').should('be.visible');
        cy.contains('.dfe-card-container', 'Guidance')
            .should('contain.text', 'Read guidance on using this service, completing checks and reviewing evidence.')
            .find('a')
            .should('have.attr', 'href', '/Home/Guidance');
    });

    it('does not show review tiles for non-MAT schools whose LA flag is disabled', () => {
        cy.checkSession('schoolCanReviewEvidenceDisabled');
        cy.visit((Cypress.config().baseUrl ?? "") + "/home");

        cy.get('.govuk-caption-l').should('include.text', 'The Aldgate School');
        cy.get('h1').should('include.text', 'Manage eligibility for free school meals');
        cy.contains('a', 'Pending applications').should('not.exist');
        cy.contains('.dfe-card-container', 'Guidance')
            .should('contain.text', 'Read guidance on using this service and completing checks.')
            .find('a')
            .should('have.attr', 'href', '/Home/Guidance');
    });

    it('shows review tiles for MAT-linked schools when the MAT flag is enabled even if the LA flag is disabled', () => {
        cy.checkSession('matSchoolWithLaFlagDisabled');
        cy.visit((Cypress.config().baseUrl ?? "") + "/home");

        cy.get('.govuk-caption-l').should('contain.text', 'Altrincham Grammar School');
        cy.get('h1').should('include.text', 'Manage eligibility for free school meals');
        cy.contains('a', 'Pending applications').should('be.visible');
        cy.contains('.dfe-card-container', 'Guidance')
            .should('contain.text', 'Read guidance on using this service, completing checks and reviewing evidence.')
            .find('a')
            .should('have.attr', 'href', '/Home/Guidance');
    });

    it('does not show review tiles for MAT-linked schools when the MAT flag is disabled even if the LA flag is enabled', () => {
        cy.checkSession('matSchoolWithMatFlagDisabled');
        cy.visit((Cypress.config().baseUrl ?? "") + "/home");

        cy.get('.govuk-caption-l').should('contain.text', 'The Telford Park School');
        cy.contains('a', 'Pending applications').should('not.exist');
        cy.contains('.dfe-card-container', 'Guidance')
            .should('contain.text', 'Read guidance on using this service and completing checks.')
            .find('a')
            .should('have.attr', 'href', '/Home/Guidance');
    });

    it('redirects to unauthorized role page when disabled school navigates directly to Pending Applications', () => {
        cy.checkSession('schoolCanReviewEvidenceDisabled');
        cy.visit((Cypress.config().baseUrl ?? "") + '/Application/PendingApplications');

        cy.contains('You do not have access to this service').should('be.visible');
    });
});