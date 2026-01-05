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

    it('Will allow a Parent to create an application and add evidence files and those files are shown on Check_Answers page', () => {
        cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start now').click();
        
        cy.get('[id="SelectedSchoolURN"]').type(schoolApprovedForPrivateBetaSearchString);
        cy.get('#schoolListResults', {timeout: 5000})
            .contains(schoolApprovedForPrivateBeta)
            .click({ force: true})
        cy.contains('Continue').click();

        cy.url().should('include', '/Home/SchoolInPrivateBeta');
        cy.get('h1').should('include.text', 'You can use this test service');
        cy.contains('Check your eligibility').click();

        cy.url().should('include', '/Check/Enter_Details')
        cy.contains('Enter your details').click();
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


        //Sign in OneGov
        const authorizationHeader: string = Cypress.env('AUTHORIZATION_HEADER');
        cy.intercept('GET', `${GOV_UK_ONE_LOGIN_SITE}/**`, (req) => {
            req.headers['Authorization'] = authorizationHeader;
        }).as('interceptForGET');

        cy.wait(3);
        let currentUrl = "";

        cy.url().then((url) => {
            currentUrl = url;
        });
        cy.visit(currentUrl, {
            auth: {
                username: Cypress.env('AUTH_USERNAME'),
                password: Cypress.env('AUTH_PASSWORD')
            },
        });

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
    });

    it('Will allow a school user to create an application and add reach Check_Answers page without uploading any evidence files', () => {
                cy.visit('/');
        cy.get('h1').should('include.text', 'Check if your children can get free school meals');
        cy.contains('Start now').click();
        
        cy.get('[id="SelectedSchoolURN"]').type(schoolApprovedForPrivateBetaSearchString);
        cy.get('#schoolListResults', {timeout: 5000})
            .contains(schoolApprovedForPrivateBeta)
            .click({ force: true})
        cy.contains('Continue').click();

        cy.url().should('include', '/Home/SchoolInPrivateBeta');
        cy.get('h1').should('include.text', 'You can use this test service');
        cy.contains('Check your eligibility').click();

        cy.url().should('include', '/Check/Enter_Details')
        cy.contains('Enter your details').click();
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


        //Sign in OneGov
        const authorizationHeader: string = Cypress.env('AUTHORIZATION_HEADER');
        cy.intercept('GET', `${GOV_UK_ONE_LOGIN_SITE}/**`, (req) => {
            req.headers['Authorization'] = authorizationHeader;
        }).as('interceptForGET');

        cy.wait(3);
        let currentUrl = "";

        cy.url().then((url) => {
            currentUrl = url;
        });
        cy.visit(currentUrl, {
            auth: {
                username: Cypress.env('AUTH_USERNAME'),
                password: Cypress.env('AUTH_PASSWORD')
            },
        });

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
    });
});