// cypress/support/commands.ts
declare namespace Cypress {
    interface Chainable {
        clickButtonByText(buttonText: string): Chainable<Element>;
        typeTextByLabel(labelText: string, text: string): Chainable<Element>


        typeIntoInput(selector: string, text: string): Chainable<void>;
        verifyFieldVisibility(selector: string, isVisible: boolean): Chainable<void>;
        enterDate(daySelector: string, monthSelector: string, yearSelector: string, day: string, month: string, year: string): Chainable<void>;
        clickButtonByRole(role: string): Chainable<void>;
        clickButton(text: string): Chainable<void>;
        verifyH1Text(expectedText: string): Chainable<void>;
        selectYesNoOption(selector: string, isYes: boolean): Chainable<Element>;
    }
}

Cypress.Commands.add('typeTextByLabel', (labelText: string, text: string) => {
    cy.contains('label', labelText)
        .parent() // Move to the parent element of the label
        .find('input') // Find the input element within that parent
        //.clear() // Clear any existing text
        .type(text); // Type the new text
});


Cypress.Commands.add('typeIntoInput', (selector: string, text: string) => {
    cy.get(selector).type(text);
});

Cypress.Commands.add('verifyFieldVisibility', (selector: string, isVisible: boolean) => {
    if (isVisible) {
        cy.get(selector).should('be.visible');
    } else {
        cy.get(selector).should('not.be.visible');
    }
});

Cypress.Commands.add('enterDate', (daySelector: string, monthSelector: string, yearSelector: string, day: string, month: string, year: string) => {
    cy.get(daySelector).clear().type(day);
    cy.get(monthSelector).clear().type(month);
    cy.get(yearSelector).clear().type(year);
});


Cypress.Commands.add('clickButtonByRole', (role: string) => {
    cy.contains(role).click();
});

Cypress.Commands.add('clickButton', (text: string) => {
    cy.contains('button', text).click();
});

Cypress.Commands.add('verifyH1Text', (expectedText: string) => {
    cy.get('h1').should('have.text', expectedText);
});

Cypress.Commands.add('selectYesNoOption', (baseSelector: string, isYes: boolean) => {
    const finalSelector = isYes ? `${baseSelector}[value="false"]` : `${baseSelector}[value="true"]`;
    cy.get(finalSelector).click();
});