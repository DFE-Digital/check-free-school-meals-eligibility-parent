import { GOV_UK_ONE_LOGIN_SITE, GOV_UK_ONE_LOGIN_URL } from "../../support/constants";
import 'cypress-file-upload';

describe('Parent with not eligible result can add evidence and submit application', () => {
    const parentFirstName = 'Tim';
    const parentLastName = Cypress.env('lastName');
    const NIN = 'PN668767B'
    const childFirstName = 'Timmy';
    const childLastName = 'Smith';
    const schoolApprovedForPrivateBeta = "Kilmorie Primary School, 100718, SE23 2SP, Lewisham";
    const schoolApprovedForPrivateBetaSearchString = "Kilmorie Primary";
    beforeEach(() => {

        // Complete private beta school check before accessing Enter_Details
        cy.completePrivateBetaSchoolCheck();

        //Sign in OneGov
        const authorizationHeader: string = Cypress.env('AUTHORIZATION_HEADER');
        cy.intercept('GET', `${GOV_UK_ONE_LOGIN_SITE}/**`, (req) => {
            req.headers['Authorization'] = authorizationHeader;
        }).as('interceptForGET');
    });

    it('Confirms the application and evidence were sent after uploading files', () => {
        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('#DateOfBirth\\.Day').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Month').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Year').should('be.visible').type('1990');

        cy.get('#IsNinoSelected').click();
        cy.get('#NationalInsuranceNumber').should('be.visible').type(NIN);
        cy.contains('Save and continue').click();

        cy.url().should('include', 'Check/Loader');
        cy.get('p.govuk-notification-banner__heading', { timeout: 80000 }).should('include.text', 'Your children may not be eligible for free school meals.');
        cy.contains('a.govuk-button', 'Apply with evidence now').click();

        cy.wait(500);
        cy.origin(GOV_UK_ONE_LOGIN_URL, () => {
            let currentUrl = "";
            cy.url().then((url) => {
                currentUrl = url;
            });
            cy.wait(2000);

            cy.visit(currentUrl, {
                auth: {
                    username: Cypress.env('AUTH_USERNAME'),
                    password: Cypress.env('AUTH_PASSWORD')
                },
            });

            cy.wait(2000);
            cy.contains('Sign in').click();
            cy.get('input[name=email]').type(Cypress.env('ONEGOV_EMAIL'));
            cy.contains('Continue').click();
            cy.get('input[name=password]').type(Cypress.env('ONEGOV_PASSWORD'));
            cy.contains('Continue').click();
        });

        cy.wait(2000);

        //Enter Child Details
        cy.url().should('include', '/Enter_Child_Details');
        cy.get('[id="ChildList[0].FirstName"]').type(childFirstName);
        cy.get('[id="ChildList[0].LastName"]').type(childLastName);

        cy.get('[id="ChildList[0].School"]').type(schoolApprovedForPrivateBetaSearchString);
        cy.get('#schoolList0')
            .should('be.visible')
            .contains(schoolApprovedForPrivateBeta)
            .click({ force: true })

        cy.get('[id="ChildList[0].DateOfBirth.Day"]').type('01');
        cy.get('[id="ChildList[0].DateOfBirth.Month"]').type('01');
        cy.get('[id="ChildList[0].DateOfBirth.Year"]').type('2007');
        cy.contains('button', 'Save and continue').click();

        //Select evidence type or none
        cy.url().should('include', '/Upload_Evidence_Type');
        cy.get('[type="radio"]').check('digital');
        cy.contains('button', 'Continue').click();

        //evidence type user information page
        cy.url().should('include', '/Upload_Guidance_Digital');
        cy.contains('button', 'Continue to upload').click();

        // Load files from fixtures folder

        //Example of how to add a single file
        // cy.url().should('include', '/UploadEvidence');
        // cy.fixture('TestFile1.txt').then(fileContent => {
        //     cy.get('input[type="file"]').attachFile({
        //         fileContent,
        //         fileName: 'TestFile1.txt',
        //         mimeType: 'text/plain'
        //     });
        // });

        cy.url().should('include', '/UploadEvidence');
        
        cy.fixture('testImage1.png').then(fileContent1 => {
            cy.fixture('testImage2.png').then(fileContent2 => {
                cy.get('input[type="file"]').attachFile([
                    {
                        fileContent: fileContent1,
                        fileName: 'testImage1.png',
                        mimeType: 'image/png'
                    },
                    {
                        fileContent: fileContent2,
                        fileName: 'testImage2.png',
                        mimeType: 'image/png'
                    }
                ]);
            });
        });

        cy.contains('button', 'Upload and continue').click();

        //Check answers
        cy.get('h1').should('include.text', 'Check your answers before sending the application');

        cy.CheckValuesInSummaryCard('Evidence', "testImage1.png", "Uploaded");
        cy.CheckValuesInSummaryCard('Evidence', "testImage2.png", "Uploaded");

        cy.get('#finishedConfirmation').check();
        cy.contains('button', 'Confirm details and send application').click();

        cy.url().should('include', '/Check/Application_Sent');
        cy.get('h1').should('have.text', 'Application and evidence sent');
        cy.title().should('match', /^Application and evidence sent - /);
        cy.contains('caption', 'We have sent the following evidence:').closest('table')
            .should('contain.text', 'testImage1.png').and('contain.text', 'testImage2.png');
    });

    it('Confirms the application was sent when the parent takes evidence into school', () => {
        cy.get('#FirstName').type(parentFirstName);
        cy.get('#LastName').type(parentLastName);
        cy.get('#DateOfBirth\\.Day').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Month').should('be.visible').type('01');
        cy.get('#DateOfBirth\\.Year').should('be.visible').type('1990');

        cy.get('#IsNinoSelected').click();
        cy.get('#NationalInsuranceNumber').should('be.visible').type(NIN);
        cy.contains('Save and continue').click();

        cy.url().should('include', 'Check/Loader');
        cy.get('p.govuk-notification-banner__heading', { timeout: 80000 }).should('include.text', 'Your children may not be eligible for free school meals.');
        cy.contains('a.govuk-button', 'Apply with evidence now').click();

        cy.wait(500);
        cy.origin(GOV_UK_ONE_LOGIN_URL, () => {
            let currentUrl = "";
            cy.url().then((url) => {
                currentUrl = url;
            });
            cy.wait(2000);

            cy.visit(currentUrl, {
                auth: {
                    username: Cypress.env('AUTH_USERNAME'),
                    password: Cypress.env('AUTH_PASSWORD')
                },
            });

            cy.wait(2000);
            cy.contains('Sign in').click();
            cy.get('input[name=email]').type(Cypress.env('ONEGOV_EMAIL'));
            cy.contains('Continue').click();
            cy.get('input[name=password]').type(Cypress.env('ONEGOV_PASSWORD'));
            cy.contains('Continue').click();
        });

        cy.wait(2000);

        //Enter Child Details
        cy.url().should('include', '/Enter_Child_Details');
        cy.get('[id="ChildList[0].FirstName"]').type(childFirstName);
        cy.get('[id="ChildList[0].LastName"]').type(childLastName);

        cy.get('[id="ChildList[0].School"]').type(schoolApprovedForPrivateBetaSearchString);
        cy.get('#schoolList0')
            .should('be.visible')
            .contains(schoolApprovedForPrivateBeta)
            .click({ force: true })

        cy.get('[id="ChildList[0].DateOfBirth.Day"]').type('01');
        cy.get('[id="ChildList[0].DateOfBirth.Month"]').type('01');
        cy.get('[id="ChildList[0].DateOfBirth.Year"]').type('2007');
        cy.contains('button', 'Save and continue').click();

        //Select evidence type or none
        cy.url().should('include', '/Upload_Evidence_Type');
        cy.get('[type="radio"]').check('none');
        cy.contains('button', 'Continue').click();

        //Check answers
        cy.get('h1').should('include.text', 'Check your answers before sending the application');
        cy.contains('h2', 'Evidence').should('not.exist');

        cy.get('#finishedConfirmation').check();
        cy.contains('button', 'Confirm details and send application').click();

        cy.url().should('include', '/Check/Application_Sent');
        cy.get('h1').should('have.text', 'Application sent');
        cy.title().should('match', /^Application sent - /);
        cy.contains('caption', 'We have sent the following evidence:').should('not.exist');
    });
});
