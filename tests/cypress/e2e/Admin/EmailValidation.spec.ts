import { v } from "@faker-js/faker/dist/airline-BLb3y-7w";

describe("email address validation", () => {
    const parentFirstName = 'Tim';
    const parentLastName = 'Jones';
    const NIN = 'PN668767B';

    const invalidEmailAddresses = [
        // 'email@123.123.123.123',

        'email@[123.123.123.123]',
        'plainaddress',
        '@no-local-part.com',
        'Outlook Contact <outlook-contact@domain.com>',
        'no-at.domain.com',
        'no-tld@domain',
        ';beginning-semicolon@domain.co.uk',
        'middle-semicolon@domain.co;uk',
        'trailing-semicolon@domain.com;',
        '"email+leading-quotes@domain.com',
        'email+middle"-quotes@domain.com',
        '"quoted-local-part"@domain.com',
        '"quoted@domain.com"',
        'lots-of-dots@domain..gov..uk',
        'multiple@domains@domain.com',
        'spaces in local@domain.com',
        'spaces-in-domain@dom ain.com',
        'underscores-in-domain@dom_ain.com',
        'pipe-in-domain@example.com|gov.uk',
        'comma,in-local@gov.uk',
        'comma-in-domain@domain,gov.uk',
        'pound-sign-in-local£@domain.com',
        'local-with-’-apostrophe@domain.com',
        'local-with-”-quotes@domain.com',
        'domain-starts-with-a-dot@.domain.com',
    ];

    const validEmailAddresses = [
        'email@domain.com',
        'email@domain.COM',
        'firstname.lastname@domain.com',
        'firstname.o\'lastname@domain.com',
        'email@subdomain.domain.com',
        'firstname+lastname@domain.com',
        '1234567890@domain.com',
        'email@domain-one.com',
        '_______@domain.com',
        'email@domain.name',
        'email@domain.superlongtld',
        'email@domain.co.jp',
        'firstname-lastname@domain.com',
        'info@german-financial-services.vermögensberatung',
        'info@german-financial-services.reallylongarbitrarytldthatiswaytoohugejustincase',
        'japanese-info@例え.テスト',
        'technically..valid@domain.com',
    ];

    const BATCH_SIZE = 18;

    beforeEach(() => {
        cy.checkSession('school');
    });

    const visitPrefilledForm = () => {
        cy.visit("/Check/Enter_Details");
        cy.get('#FirstName').should('exist');

        cy.window().then(win => {
            // Get the DOM elements and set their values
            const firstNameEl = win.document.getElementById('FirstName') as HTMLInputElement;
            const lastNameEl = win.document.getElementById('LastName') as HTMLInputElement;
            const dayEl = win.document.getElementById('DateOfBirth.Day') as HTMLInputElement;
            const monthEl = win.document.getElementById('DateOfBirth.Month') as HTMLInputElement;
            const yearEl = win.document.getElementById('DateOfBirth.Year') as HTMLInputElement;
            const ninEl = win.document.getElementById('NationalInsuranceNumber') as HTMLInputElement;

            if (firstNameEl) firstNameEl.value = parentFirstName;
            if (lastNameEl) lastNameEl.value = parentLastName;
            if (dayEl) dayEl.value = '01';
            if (monthEl) monthEl.value = '01';
            if (yearEl) yearEl.value = '1990';
            if (ninEl) ninEl.value = NIN;

            // Check the NIN radio button
            const ninRadioEl = win.document.getElementById('NinAsrSelection') as HTMLInputElement;
            if (ninRadioEl) ninRadioEl.checked = true;
        });
    };

    const fillStandardFormFields = () => {
        cy.get('#FirstName').clear().type(parentFirstName);
        cy.get('#LastName').clear().type(parentLastName);
        cy.get('[id="DateOfBirth.Day"]').clear().type('01');
        cy.get('[id="DateOfBirth.Month"]').clear().type('01');
        cy.get('[id="DateOfBirth.Year"]').clear().type('1990');
        cy.get('#NinAsrSelection').check();
        cy.get('#NationalInsuranceNumber').clear().type(NIN);
    };

    const testInvalidEmail = (email: string) => {
        cy.log(`Testing invalid email: ${email}`);
        // instead of type, set the value directly in the DOM
        cy.window().then(win => {   
            const emailEl = win.document.getElementById('EmailAddress') as HTMLInputElement;
            if (emailEl) emailEl.value = email; // Set the value directly
        });
        cy.contains('Perform check').click();
        cy.get('.govuk-error-message').should('contain', 'Enter an email address in the correct format, like name@example.com');
    };

    const testValidEmail = (email: string) => {
        cy.log(`Testing valid email: ${email}`);
        cy.window().then(win => {
            const emailEl = win.document.getElementById('EmailAddress') as HTMLInputElement;
            if (emailEl) emailEl.value = email;
        });
        cy.contains('Perform check').click();
        // Check that the validation message specifically for email is not present
        cy.get('.govuk-error-message span[data-valmsg-for="EmailAddress"]').should('not.exist');

        // Check that if there's an error summary, it doesn't contain email format error
        cy.document().then(($document) => {
            const errorSummary = $document.querySelector('.govuk-error-summary');
            if (errorSummary) {
                // If error summary exists, it shouldn't mention email format
                expect(errorSummary.textContent).not.to.include('Enter an email address in the correct format');
            }
        });
    };

    const createBatches = (items: any[], batchSize: number) => {
        const batches = [];
        for (let i = 0; i < items.length; i += batchSize) {
            batches.push(items.slice(i, i + batchSize));
        }
        return batches;
    };

    const invalidEmailBatches = createBatches(invalidEmailAddresses, BATCH_SIZE);
    
    invalidEmailBatches.forEach((batch, batchIndex) => {
        it(`should invalidate incorrect email addresses - Batch ${batchIndex + 1} of ${invalidEmailBatches.length}`, () => {
            cy.log(`Testing invalid email batch ${batchIndex + 1} of ${invalidEmailBatches.length}`);
            visitPrefilledForm();
            batch.forEach((email) => {
                testInvalidEmail(email);
            });
        });
    });


    const validEmailBatches = createBatches(validEmailAddresses, BATCH_SIZE);
    validEmailBatches.forEach((batch, batchIndex) => {
        it(`should accept valid email addresses - Batch ${batchIndex + 1} of ${validEmailBatches.length}`, () => {
            batch.forEach((email, index) => {
                visitPrefilledForm();
                testValidEmail(email);
            });
        });
    });

    /* validEmailAddresses.forEach((email, index) => {
        it(`should accept valid email address: ${email}`, () => {
            visitPrefilledForm();
            testValidEmail(email);
        });
    });
    
    invalidEmailAddresses.forEach((email, index) => {
        it(`should invalidate incorrect email address: ${email}`, () => {
            visitPrefilledForm();
            testInvalidEmail(email);
        });
    }); */
});