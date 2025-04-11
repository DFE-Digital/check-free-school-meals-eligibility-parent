const parentFirstName = 'Tim';
const parentLastName = Cypress.env('lastName');
const parentEmailAddress = 'TimJones@Example.com';
const NIN = 'PN668767B';
const childFirstName = 'Timmy';
const childLastName = 'Smith';

const visitPrefilledForm = (onlyfill?: boolean) => {
    if (!onlyfill) {
        cy.visit("/Check/Enter_Details");
        cy.get('#FirstName').should('exist');
    }

    cy.window().then(win => {
        const firstNameEl = win.document.getElementById('FirstName') as HTMLInputElement;
        const lastNameEl = win.document.getElementById('LastName') as HTMLInputElement;
        const dayEl = win.document.getElementById('Day') as HTMLInputElement;
        const monthEl = win.document.getElementById('Month') as HTMLInputElement;
        const yearEl = win.document.getElementById('Year') as HTMLInputElement;
        const ninEl = win.document.getElementById('NationalInsuranceNumber') as HTMLInputElement;
        const emailEl = win.document.getElementById('EmailAddress') as HTMLInputElement;

        if (firstNameEl) firstNameEl.value = parentFirstName;
        if (lastNameEl) lastNameEl.value = parentLastName;
        if (dayEl) dayEl.value = '01';
        if (monthEl) monthEl.value = '01';
        if (yearEl) yearEl.value = '1990';
        if (ninEl) ninEl.value = NIN;
        if (emailEl) emailEl.value = parentEmailAddress;

        // Check the NIN radio button
        const ninRadioEl = win.document.getElementById('NinAsrSelection') as HTMLInputElement;
        if (ninRadioEl) ninRadioEl.checked = true;
    });
};

describe("Links on not eligible page route to the intended locations", () => {
    beforeEach(() => {
        cy.checkSession('school'); // if no session exists login as given type
        visitPrefilledForm();
        cy.contains('Perform check').click();
    });

    it("Guidance link should route to guidance page", () => {
        cy.contains('a.govuk-link', 'See a complete list of acceptable evidence', { timeout: 8000 }).then(($link) => {
            const url = $link.prop('href');
            cy.visit(url);
            cy.get('h1.govuk-heading-l').should('contain.text', 'Guidance for reviewing evidence');
        });
    });

    it("Support link should route to DfE form", () => {
        cy.contains('a.govuk-link', 'contact the Department for Education support desk', { timeout: 8000 }).then(($link) => {
            const url = $link.prop('href');
            cy.visit(url);
            cy.get('span.text-format-content').should('contain.text', "Check a Family's FSM Eligibility Query Form");
        });
    });
});

describe('Date of Birth Validation Tests', () => {
    beforeEach(() => {
        cy.checkSession('school'); // if no session exists login as given type
        cy.visit('/Check/Enter_Details');
    });

    it('displays error messages for missing date fields', () => {
        cy.get('#Day').clear();
        cy.get('#Month').clear();
        cy.get('#Year').clear();
        cy.contains('Perform check').click();

        cy.get('.govuk-error-message').should('contain', 'Enter a date of birth');
        cy.get('#Day').should('have.class', 'govuk-input--error');
        cy.get('#Month').should('have.class', 'govuk-input--error');
        cy.get('#Year').should('have.class', 'govuk-input--error');
    });

    it('displays error messages for non-numeric inputs', () => {
        cy.get('#Day').clear().type('abc');
        cy.get('#Month').clear().type('xyz');
        cy.get('#Year').clear().type('abcd');
        cy.contains('Perform check').click();

        cy.get('.govuk-error-message').should('contain', 'Date of birth must be a real date');
        cy.get('#Day').should('have.class', 'govuk-input--error');
        cy.get('#Month').should('have.class', 'govuk-input--error');
        cy.get('#Year').should('have.class', 'govuk-input--error');
    });

    it('displays error messages for out-of-range inputs', () => {
        cy.get('#Day').clear().type('50');
        cy.get('#Month').clear().type('13');
        cy.get('#Year').clear().type('1800');
        cy.contains('Perform check').click();

        cy.get('.govuk-error-message').should('contain', 'Date of birth must be a real date');
        cy.get('#Day').should('have.class', 'govuk-input--error');
        cy.get('#Month').should('have.class', 'govuk-input--error');
        cy.get('#Year').should('have.class', 'govuk-input--error');
    });

    it('displays error messages for future dates', () => {
        cy.get('#Day').clear().type('01');
        cy.get('#Month').clear().type('01');
        cy.get('#Year').clear().type((new Date().getFullYear() + 1).toString());
        cy.contains('Perform check').click();

        cy.get('.govuk-error-message').should('contain', 'Enter a date in the past');
    });

    it('displays error messages for invalid combinations', () => {
        cy.get('#Day').clear().type('31');
        cy.get('#Month').clear().type('02');
        cy.get('#Year').clear().type('2020');
        cy.contains('Perform check').click();

        cy.get('.govuk-error-message').should('contain', 'Date of birth must be a real date');
    });

    it('allows valid date of birth submission', () => {
        cy.get('#Day').clear().type('15');
        cy.get('#Month').clear().type('06');
        cy.get('#Year').clear().type('2005');
        cy.contains('Perform check').click();

        cy.get('#Day + .govuk-error-message').should('not.exist');
        cy.get('#Month + .govuk-error-message').should('not.exist');
        cy.get('#Year + .govuk-error-message').should('not.exist');

        cy.get('#Day').should('not.have.class', 'govuk-input--error');
        cy.get('#Month').should('not.have.class', 'govuk-input--error');
        cy.get('#Year').should('not.have.class', 'govuk-input--error');
    });
});

describe("Conditional content on ApplicationDetailAppeal page", () => {
    const parentFirstName = 'Tim';
    const parentLastName = Cypress.env('lastName');
    const parentEmailAddress = 'TimJones@Example.com';
    const NIN = 'PN668767B'
    const childFirstName = 'Timmy';
    const childLastName = 'Smith';

    beforeEach(() => {
        cy.checkSession('school'); // if no session exists login as given type
    });

    it("will show conditional content when status is Evidence Needed and not when status is Sent for Review", () => {
        cy.visit('/');
        cy.contains('Run a check for one parent or guardian').click();
        cy.get('#consent').check();
        cy.get('#submitButton').click();

        //Soft-Check
        cy.url().should('include', '/Check/Enter_Details');
        visitPrefilledForm(true);
        cy.contains('button', 'Perform check').click();
        //Not Eligible, Appeal
        cy.url().should('include', 'Check/Loader');
        cy.get('p.govuk-notification-banner__heading', { timeout: 80000 }).should('include.text', 'The children of this parent or guardian may not be eligible for free school meals');
        cy.contains('.govuk-button', 'Appeal now').click();
        //Enter Child Details
        cy.url().should('include', '/Check/Enter_Child_Details');
        cy.get('[id="ChildList[0].FirstName"]').type(childFirstName);
        cy.get('[id="ChildList[0].LastName"]').type(childLastName);
        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');
        cy.contains('button', 'Save and continue').click();
        //Check and confirm
        cy.get('h1').should('include.text', 'Check your answers before submitting');
        cy.contains('button', 'Add details').click();
        //Find reference on page and save as variable
        cy.get('.govuk-table__cell').eq(1).invoke('text').then((referenceNumber) => {
            const refNumber = referenceNumber.trim();

            cy.visit("/");
            cy.visit('/Application/AppealsApplications?PageNumber=0');
            cy.wait(100);
            cy.scanPagesForNewValue(refNumber);
            cy.contains('p.govuk-heading-s', "Once you've received evidence from this parent or guardian:");
            cy.contains('a.govuk-button', 'Send for review').click();
            cy.get('a.govuk-button--primary').click();
            cy.visit("/Application/AppealsApplications?PageNumber=0");
            cy.wait(1000);
            cy.scanPagesForNewValue(refNumber);
            cy.contains('p.govuk-heading-s', "Once you've received evidence from this parent or guardian:").should('not.exist');
        });
    });
});

describe("Condtional content on ApplicationDetail page", () => {
    const parentFirstName = 'Tim';
    const parentLastName = Cypress.env('lastName');
    const parentEmailAddress = 'TimJones@Example.com';
    const NIN = 'PN668767B'
    const childFirstName = 'Timmy';
    const childLastName = 'Smith';

    beforeEach(() => {
        cy.checkSession('school'); // if no session exists login as given type
    });

    it("will show conditional content when status is Evidence Needed and wont when status is Sent  for Review", () => {
        cy.visit("/");
        cy.contains('Run a check for one parent or guardian').click();
        cy.get('#consent').check();
        cy.get('#submitButton').click();

        //Soft-Check
        cy.url().should('include', '/Check/Enter_Details');
        visitPrefilledForm(true);
        cy.contains('button', 'Perform check').click();
        //Not Eligible, Appeal
        cy.url().should('include', 'Check/Loader');
        cy.get('p.govuk-notification-banner__heading', { timeout: 80000 }).should('include.text', 'The children of this parent or guardian may not be eligible for free school meals');
        cy.contains('.govuk-button', 'Appeal now').click();
        //Enter Child Details
        cy.url().should('include', '/Check/Enter_Child_Details');
        cy.get('[id="ChildList[0].FirstName"]').type(childFirstName);
        cy.get('[id="ChildList[0].LastName"]').type(childLastName);
        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');
        cy.contains('button', 'Save and continue').click();
        //Check and confirm
        cy.get('h1').should('include.text', 'Check your answers before submitting');
        cy.contains('button', 'Add details').click();
        //Find reference on page and save as variable
        cy.get('.govuk-table__cell').eq(1).invoke('text').then((referenceNumber) => {
            const refNumber = referenceNumber.trim();

            cy.visit("/Application/SearchResults");
            cy.wait(1000);
            cy.get('#Status_EvidenceNeeded').check();
            cy.wait(100);
            cy.contains('button.govuk-button', 'Apply filters').click();
            cy.wait(100);
            cy.scanPagesForNewValue(refNumber);
            cy.contains('p.govuk-heading-s', "Once you've received evidence from this parent or guardian:");
            cy.contains('a.govuk-button', 'Send for review').click();
            cy.get('a.govuk-button--primary').click();
            cy.visit("/Application/SearchResults");
            cy.wait(1000);
            cy.get('#Status_SentForReview').check();
            cy.wait(100);
            cy.contains('button.govuk-button', 'Apply filters').click();
            cy.wait(100);
            cy.scanPagesForNewValue(refNumber);
            cy.contains('p.govuk-heading-s', "Once you've received evidence from this parent or guardian:").should('not.exist');
        });
    });
});

describe("Feedback link in header as School", () => {
    beforeEach(() => {
        cy.checkSession('school'); // if no session exists login as given type
    });

    it("Should route a School user to a qualtrics survey", () => {
        cy.visit('/');
        cy.get('span.govuk-phase-banner__text > a.govuk-link')
            .invoke('removeAttr', 'target')
            .click();
        cy.url()
            .should('include', 'https://dferesearch.fra1.qualtrics.com/jfe/form/SV_bjB0MQiSJtvhyZw');
        cy.contains("Thank you for participating in this survey")
    });
});

describe("Feedback link in header as LA", () => {
    beforeEach(() => {
        cy.checkSession('LA'); // if no session exists login as given type
    });

    it("Should route an LA user to a qualtrics survey", () => {
        cy.visit('/');
        cy.get('span.govuk-phase-banner__text > a.govuk-link')
            .invoke('removeAttr', 'target')
            .click();
        cy.url()
            .should('include', 'https://dferesearch.fra1.qualtrics.com/jfe/form/SV_bjB0MQiSJtvhyZw');
        cy.contains("Thank you for participating in this survey")
    });
});

describe("Error Content on FinaliseApplication page", () => {
    beforeEach(() => {
        cy.checkSession('school'); // if no session exists login as given type
    });

    it("Should give an error message if no applications are selected", () => {
        cy.visit('/');
        cy.get('#finalise').click();
        cy.get('#submit').click();
        cy.get('.govuk-error-message').should('contain', 'Select records to finalise');
    });
});