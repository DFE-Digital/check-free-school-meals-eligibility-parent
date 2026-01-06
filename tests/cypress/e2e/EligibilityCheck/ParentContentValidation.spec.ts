describe('Parent Date of Birth Validation Tests', () => {
    const parentFirstName = 'Tim';
    const parentLastName = Cypress.env('lastName');
    const parentEmailAddress = 'TimJones@Example.com';
    const NIN = 'PN668767B';
    const schoolApprovedForPrivateBeta = "Kilmorie Primary School, 100718, SE23 2SP, Lewisham";
    const schoolApprovedForPrivateBetaSearchString = "Kilmorie Primary";

    beforeEach(() => {
        // Ensure the test starts on the correct page
        cy.visit('/Check/Enter_Details');
    });

    it('displays error messages for missing date fields', () => {
        // Scenario 1: All fields are empty
        cy.get('#DateOfBirth\\.Day').clear();
        cy.get('#DateOfBirth\\.Month').clear();
        cy.get('#DateOfBirth\\.Year').clear();
        cy.contains('Save and continue').click();

        // Assert that the error message for missing date of birth is shown
        cy.get('.govuk-error-message').should('contain', 'Enter a date of birth');
        cy.get('#DateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Month').should('have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Year').should('have.class', 'govuk-input--error');
    });

    it('displays error messages for non-numeric inputs', () => {
        // Scenario 2: Non-numeric inputs
        cy.get('#DateOfBirth\\.Day').clear().type('abc'); // Invalid #DateOfBirth\\.Day
        cy.get('#DateOfBirth\\.Month').clear().type('xyz'); // Invalid month
        cy.get('#DateOfBirth\\.Year').clear().type('abcd'); // Invalid year
        cy.contains('Save and continue').click();

        // Assert error messages
        cy.get('.govuk-error-message').should('contain', 'Enter a date of birth using numbers only');
        cy.get('#DateOfBirth\\.Day').should('have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Month').should('have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Year').should('have.class', 'govuk-input--error');
    });

    it('displays error messages for out-of-range inputs', () => {
        // Scenario 3: Invalid #DateOfBirth\\.Day, month, and year ranges
        cy.get('#DateOfBirth\\.Day').clear().type('50'); // Invalid 
        cy.get('#DateOfBirth\\.Month').clear().type('13'); // Invalid month
        cy.get('#DateOfBirth\\.Year').clear().type('1800'); // Invalid year
        cy.contains('Save and continue').click();

        // Assert error messages for each out-of-range field
        cy.get('.govuk-error-message').should('contain', 'Enter a valid date');
        cy.get('.govuk-error-message').should('contain', 'Enter a valid date');
        cy.get('.govuk-error-message').should('contain', 'Enter a valid date');
    });

    it('displays error messages for future dates', () => {
        // Scenario 4: Date in the future
        cy.get('#DateOfBirth\\.Day').clear().type('01');
        cy.get('#DateOfBirth\\.Month').clear().type('01');
        cy.get('#DateOfBirth\\.Year').clear().type((new Date().getFullYear() + 1).toString()); // Next year
        cy.contains('Save and continue').click();

        // Assert that the error message for a future date is shown
        cy.get('.govuk-error-message').should('contain', 'Enter a date in the past');
    });

    it('displays error messages for invalid combinations (e.g., 31st February)', () => {
        // Scenario 5: Invalid date combination
        cy.get('#DateOfBirth\\.Day').clear().type('31');
        cy.get('#DateOfBirth\\.Month').clear().type('02'); // February
        cy.get('#DateOfBirth\\.Year').clear().type('2020'); // Leap year
        cy.contains('Save and continue').click();

        // Assert error message for invalid day in month
        cy.get('.govuk-error-message').should('contain', 'Enter a valid date');
    });

    it('allows valid date of birth submission', () => {
        // Scenario 6: Valid date
        cy.get('#DateOfBirth\\.Day').clear().type('15');
        cy.get('#DateOfBirth\\.Month').clear().type('06');
        cy.get('#DateOfBirth\\.Year').clear().type('2005');
        cy.contains('Save and continue').click();
    
        // Assert no DOB-specific error messages are shown
        cy.get('#DateOfBirth\\.Day + .govuk-error-message').should('not.exist'); // Scoped to Day error message
        cy.get('#Month + .govuk-error-message').should('not.exist'); // Scoped to Month error message
        cy.get('#Year + .govuk-error-message').should('not.exist'); // Scoped to Year error message
    
        // Assert no error classes are applied to DOB fields
        cy.get('#DateOfBirth\\.Day').should('not.have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Month').should('not.have.class', 'govuk-input--error');
        cy.get('#DateOfBirth\\.Year').should('not.have.class', 'govuk-input--error');
    });
    
});
