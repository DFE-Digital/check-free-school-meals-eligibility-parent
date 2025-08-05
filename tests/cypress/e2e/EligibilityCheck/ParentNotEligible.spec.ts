
describe('Parents journey when not eligible', () => {

    it('Will return the correct responses if the Parent is not eligible for free school meals', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start now').click();
        cy.get('input.govuk-radios__input[value="true"]').check();
        cy.contains('Continue').click();

        cy.url().should('include', '/Check/Enter_Details')
        cy.get('h1').should('include.text', 'Enter your details');
        cy.get('#FirstName').type('Tim');
        cy.get('#LastName').type('TESTER');

        cy.get('#DateOfBirth\\.Day').type('01');
        cy.get('#DateOfBirth\\.Month').type('01');
        cy.get('#DateOfBirth\\.Year').type('1990');

        cy.get('#IsNinoSelected').click();
        cy.get('#NationalInsuranceNumber').type('PN668767B');

        cy.contains('Save and continue').click();

        cy.url().should('include', '/Check/Loader');
        cy.wait(2000);
        cy.get('.govuk-notification-banner__heading').should('include.text', 'Your children may not be eligible for free school meals');
    });

    it('Will return the correct response if we cannot find the user', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start now').click();
        cy.get('input.govuk-radios__input[value="true"]').check();
        cy.contains('Continue').click();

        cy.url().should('include', '/Check/Enter_Details')
        cy.get('h1').should('include.text', 'Enter your details');
        cy.get('#FirstName').type('Tim');
        cy.get('#LastName').type('TESTER');

        cy.get('#DateOfBirth\\.Day').type('01');
        cy.get('#DateOfBirth\\.Month').type('01');
        cy.get('#DateOfBirth\\.Year').type('1990');

        cy.get('#IsNinoSelected').click();
        cy.get('#NationalInsuranceNumber').type('PN668767B');

        cy.contains('Save and continue').click();

        cy.wait(2000);
        cy.url().should('include', '/Check/Loader');
        cy.get('.govuk-notification-banner__heading').should('include.text', 'Your children may not be eligible for free school meals');
    });

    it('Will return the correct error response if the user inputs a NI number in the incorrect format', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start now').click();
        cy.get('input.govuk-radios__input[value="true"]').check();
        cy.contains('Continue').click();

        cy.url().should('include', '/Check/Enter_Details')
        cy.get('h1').should('include.text', 'Enter your details');
        cy.get('#FirstName').type('Tim');
        cy.get('#LastName').type('Smith');

        cy.get('#DateOfBirth\\.Day').type('01');
        cy.get('#DateOfBirth\\.Month').type('01');
        cy.get('#DateOfBirth\\.Year').type('1990');

        cy.get('#IsNinoSelected').click();
        cy.get('#NationalInsuranceNumber').type('ABCDE456C');

        cy.contains('Save and continue').click();

        cy.get('h2').should('include.text', 'There is a problem');
        cy.get('a').should('include.text', 'Invalid National Insurance number format');
    });

    it('Will return the correct error response if the user inputs a NI number is too long', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start now').click();
        cy.get('input.govuk-radios__input[value="true"]').check();
        cy.contains('Continue').click();

        cy.url().should('include', '/Check/Enter_Details');
        cy.get('h1').should('include.text', 'Enter your details');
        cy.get('#FirstName').type('Tim');
        cy.get('#LastName').type('Smith');

        cy.get('#DateOfBirth\\.Day').type('01');
        cy.get('#DateOfBirth\\.Month').type('01');
        cy.get('#DateOfBirth\\.Year').type('1990');

        cy.get('#IsNinoSelected').click();
        cy.get('#NationalInsuranceNumber').type('0123456789');

        cy.contains('Save and continue').click();

        cy.get('h2').should('include.text', 'There is a problem');
        cy.get('a').should('include.text', 'National Insurance number should contain no more than 9 alphanumeric characters');
    });

    it('Allows a user to enter correct NI number after entering an incorrect one', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start now').click();
        cy.get('input.govuk-radios__input[value="true"]').check();
        cy.contains('Continue').click();

        cy.url().should('include', '/Check/Enter_Details');
        cy.get('h1').should('include.text', 'Enter your details');
        cy.get('#FirstName').type('Tim');
        cy.get('#LastName').type('Smith');

        cy.get('#DateOfBirth\\.Day').type('01');
        cy.get('#DateOfBirth\\.Month').type('01');
        cy.get('#DateOfBirth\\.Year').type('1990');

        cy.get('#IsNinoSelected').click();
        cy.get('#NationalInsuranceNumber').type('0123456789');

        cy.contains('Save and continue').click();

        cy.get('h2').should('include.text', 'There is a problem');
        cy.get('li').should('include.text', 'National Insurance number should contain no more than 9 alphanumeric characters');

        cy.get('#NationalInsuranceNumber').clear().type('NN668767B');
        cy.contains('Save and continue').click();
        cy.url().should('include','/Check/Loader');

    })

});