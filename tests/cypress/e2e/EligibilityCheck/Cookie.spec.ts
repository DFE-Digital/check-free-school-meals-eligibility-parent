interface CustomWindow extends Window {
    clarity: Function;
}

describe('Cookie consent banner functionality', () => {
    it('Should show the cookie banner on first visit when no choice has been made', () => {
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start now').click()
        cy.get('.govuk-cookie-banner')
            .first()
            .should('be.visible')
            .within(() => {
                cy.contains('h2', 'Cookies on Check if your children can get free school meals');
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

    it('Should hide banner and set cookie when accepting analytics from the cookies page option', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start now').click()
        cy.get('.govuk-footer__link[href="/Home/Cookies"]').click();

        // Select yes radio button and submit using Continue button then verify display state
        cy.get('#cookies-analytics-yes').click();
        cy.get('button.govuk-button').contains('Save cookie settings').click();

        cy.get('#cookie-banner').should('have.css', 'display', 'none');

        // Verify banner stays hidden on next visit
        cy.reload();
        cy.get('#cookie-banner').should('have.css', 'display', 'none');
    });

    it('Should hide banner and set cookie when rejecting analytics from the cookies page option', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start now').click()
        cy.get('.govuk-footer__link[href="/Home/Cookies"]').click();

        // Select no radio button and submit using Continue button then verify display state
        cy.get('#cookies-analytics-no').click();
        cy.get('button.govuk-button').contains('Save cookie settings').click();

        cy.get('#cookie-banner').should('have.css', 'display', 'none');

        // Verify banner stays hidden on next visit
        cy.reload();
        cy.get('#cookie-banner').should('have.css', 'display', 'none');
    });

    it('Should initialize Clarity when analytics are accepted', () => {
        cy.visit(Cypress.config().baseUrl ?? "");

        // Accept cookies
        cy.get('#accept-cookies').click();
        cy.wait(1000);

        // Verify Clarity script is added
        cy.get('body')
            .invoke('attr', 'data-clarity')
            .then(($clarity) => {
                if ($clarity) {
                    cy.get('head script[src*="clarity"]');
                }
            }
            );
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