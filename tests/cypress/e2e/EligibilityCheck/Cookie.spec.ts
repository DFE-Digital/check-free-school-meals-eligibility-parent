interface CustomWindow extends Window {
    clarity: Function;
}

describe('Cookie consent banner functionality', () => {
    it('Should show the cookie banner on first visit when no choice has been made', () => {
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.get('.govuk-cookie-banner')
            .should('be.visible')
            .within(() => {
                cy.contains('h2', 'Cookies on check a family\'s eligibility');
                cy.contains('button', 'Accept analytics cookies');
                cy.contains('button', 'Reject analytics cookies');
            });
    });

    it('Should hide banner and set cookie when accepting analytics', () => {
        cy.visit(Cypress.config().baseUrl ?? "");
        
        // Click accept and verify display state
        cy.get('#accept-cookies').click();
        cy.wait(1000);
        cy.get('#cookie-banner').should('have.css', 'display', 'none');
        
        // Verify banner stays hidden on next visit
        cy.reload();
        cy.get('#cookie-banner').should('have.css', 'display', 'none');
    });

    it('Should hide banner and set cookie when rejecting analytics', () => {
        cy.visit(Cypress.config().baseUrl ?? "");
        
        // Click reject and verify display state
        cy.get('#reject-cookies').click();
        cy.wait(1000);
        cy.get('#cookie-banner').should('have.css', 'display', 'none');
        
        // Verify banner stays hidden on next visit
        cy.reload();
        cy.get('#cookie-banner').should('have.css', 'display', 'none');
    });

    it('Should initialize Clarity when analytics are accepted', () => {
        cy.visit(Cypress.config().baseUrl ?? "");
    
        // Accept cookies
        cy.get('#accept-cookies').click();
    });

    it('Should remove Clarity cookies when analytics are rejected', () => {
        // First accept to set cookies
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.get('#accept-cookies').click();
        cy.wait(1000);
        
        // Then reject to remove them
        cy.clearCookies();
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.get('#reject-cookies').click();
        cy.wait(1000);
        
        // Verify Clarity script is not added
        cy.window().then((win) => {
            const clarityId = win.document.body.getAttribute('data-clarity');
            cy.get(`script[src*="clarity.ms/tag/${clarityId}"]`)
                .should('not.exist');
        });
    });
});