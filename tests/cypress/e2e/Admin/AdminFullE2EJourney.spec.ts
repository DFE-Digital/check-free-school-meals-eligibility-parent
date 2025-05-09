let referenceNumber: string;
let skipSetup = false

describe('Full journey of creating an application through school portal through to approving in LA portal', () => {
    const parentFirstName = 'Tim';
    const parentLastName = Cypress.env('lastName');
    const parentEmailAddress = 'TimJones@Example.com';
    const NIN = 'PN668767B'
    const childFirstName = 'Timmy';
    const childLastName = 'Smith';

    beforeEach(() => {
        if (!skipSetup) {
            cy.checkSession('school');
            cy.visit(Cypress.config().baseUrl ?? "");
            cy.wait(1);
            cy.get('h1').should('include.text', 'The Telford Park School');
        }
    });

    it('Will allow a school user to create an application that may not be elligible and send it for appeal', () => {
        //Add parent details
        cy.contains('Run a check for one parent or guardian').click();
        cy.get('#consent').check();
        cy.get('#submitButton').click();
        cy.url().should('include', '/Check/Enter_Details');
        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('#EmailAddress').type(parentEmailAddress);
        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');
        cy.get('#NinAsrSelection').click();
        cy.get('#NationalInsuranceNumber').type(NIN);
        cy.contains('button', 'Perform check').click();

        //Loader page
        cy.url().should('include', 'Check/Loader');

        //Not eligible outcome
        cy.get('p.govuk-notification-banner__heading', { timeout: 80000 }).should('include.text', 'The children of this parent or guardian may not be eligible for free school meals');
        cy.contains('a.govuk-button', 'Appeal now').click();

        //Enter child details
        cy.url().should('include', '/Enter_Child_Details');
        cy.get('[id="ChildList[0].FirstName"]').type(childFirstName);
        cy.get('[id="ChildList[0].LastName"]').type(childLastName);
        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');
        cy.contains('button', 'Add another child').click();
        cy.contains('button', 'Remove').click();
        cy.contains('button', 'Save and continue').click();

        //Add supporting evidence or skip
        cy.url().should('include', '/UploadEvidence');
        cy.fixture('TestFile1.txt').then(fileContent => {
            cy.get('input[type="file"]').attachFile({
                fileContent,
                fileName: 'TestFile1.txt',
                mimeType: 'text/plain'
            });
        });
        cy.contains('button', 'Attach evidence').click();

        //Check answers page
        cy.get('h1').should('include.text', 'Check your answers before submitting');
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Name', `${parentFirstName} ${parentLastName}`);
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Date of birth', '1 January 1990');
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'National Insurance number', NIN);
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Email address', parentEmailAddress);
        cy.CheckValuesInSummaryCard('Child 1 details', "Name", childFirstName + " " + childLastName);
        cy.CheckValuesInSummaryCard('Evidence', "TestFile1.txt", "Uploaded");
        cy.contains('button', 'Add details').click();

        //Appeals Registered confirmation page
        cy.url().should('include', '/Check/AppealsRegistered');
        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(1)
            .invoke('text')
            .then((text) => {
                referenceNumber = text.trim().toString();
                cy.wrap('referenceNumber').as(referenceNumber);
            });
    });

    it('Will allow a school user to create an application is eligible and submit an application', () => {
        //Add parent details
        cy.contains('Run a check for one parent or guardian').click();
        cy.get('#consent').check();
        cy.get('#submitButton').click();
        cy.url().should('include', '/Check/Enter_Details');
        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('#EmailAddress').type(parentEmailAddress);
        cy.get('#Day').type('01');
        cy.get('#Month').type('01');
        cy.get('#Year').type('1990');
        cy.get('#NinAsrSelection').click();
        cy.get('#NationalInsuranceNumber').type("nn123456c");
        cy.contains('button', 'Perform check').click();

        //Loader page
        cy.url().should('include', 'Check/Loader');

        //Eligible outcome page
        cy.get('.govuk-notification-banner__heading', { timeout: 80000 }).should('include.text', 'The children of this parent or guardian are eligible for free school meals');
        cy.contains('a.govuk-button', "Add children's details").click();

        //Enter child details
        cy.url().should('include', '/Enter_Child_Details');
        cy.get('[id="ChildList[0].FirstName"]').type(childFirstName);
        cy.get('[id="ChildList[0].LastName"]').type(childLastName);
        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');
        cy.contains('button', 'Save and continue').click();

        //Check answers page
        cy.get('h1').should('include.text', 'Check your answers before submitting');
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Name', `${parentFirstName} ${parentLastName}`);
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Date of birth', '1 January 1990');
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'National Insurance number', "NN123456C");
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Email address', parentEmailAddress);
        cy.CheckValuesInSummaryCard('Child 1 details', "Name", childFirstName + " " + childLastName);
        cy.contains('button', 'Add details').click();

        //Applications Registered confirmation page
        cy.url().should('include', '/Check/ApplicationsRegistered');
    });

    it('Allows a user when logged into the LA portal to approve the application review', () => {
        skipSetup = true; //don't restore school session
        //Log in a LA and navigate to Pending Applications
        cy.checkSession('LA');
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.get('h1').should('include.text', 'Telford and Wrekin Council');
        cy.contains('.govuk-link', 'Pending applications').click();

        //Approve Not Eligible Appeal Application from earlier
        cy.url().should('contain', 'Application/PendingApplications');
        cy.scanPagesForNewValue(referenceNumber);
        cy.contains('.govuk-button', 'Approve application').click();
        cy.contains('.govuk-button', 'Yes, approve now').click();
        
        //Search for approved application
        cy.visit('/');
        cy.contains('Search all records').click();
        cy.url().should('contain', 'Application/SearchResults');
        cy.get('#Keyword').type(referenceNumber);
        cy.contains('button.govuk-button', 'Apply filters').click(); //Apply filters
        cy.url().should('include', 'Application/SearchResults');
        cy.get('h2').should('contain.text', 'Showing 1 results');
        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(0)
            .should('contain.text', referenceNumber);
        cy.get('.govuk-table')
            .find('tbody tr')
            .eq(0)
            .find('td')
            .eq(4)
            .should('contain.text', 'Reviewed Entitled');
        skipSetup = false;
    });

    it('Allows a user when back logged into the School portal to finalise the application', () => {
        cy.contains('Finalise applications').click();
        cy.url().should('contain', 'Application/FinaliseApplications');
        cy.findNewApplicationFinalise(referenceNumber).then(() => {
            cy.contains('.govuk-button', 'Finalise applications').click();
            cy.contains('.govuk-button', 'Yes, finalise now').click();
        });
    });
});