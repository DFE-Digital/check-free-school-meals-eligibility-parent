describe('Full journey of creating an application through school portal through to approving in LA portal', () => {
    const parentFirstName = 'Tim';
    const parentLastName = Cypress.env('lastName');
    const parentEmailAddress = 'TimJones@Example.com';
    const NIN = 'PN668767B'
    const childFirstName = 'Timmy';
    const childLastName = 'Smith';
    let referenceNumber: string;

    beforeEach(() => {
        cy.checkSession('school');
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.wait(1);
        cy.get('h1').should('include.text', 'The Telford Park School');
    });

    it('Will allow a school user to create an application and add evidence files and those files are shown on Check_Answers page', () => {
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

        cy.url().should('include', 'Check/Loader');
        cy.get('p.govuk-notification-banner__heading', { timeout: 80000 }).should('include.text', 'The children of this parent or guardian may not be eligible for free school meals');
        cy.contains('a.govuk-button', 'Appeal now').click();

        cy.url().should('include', '/Enter_Child_Details');
        cy.get('[id="ChildList[0].FirstName"]').type(childFirstName);
        cy.get('[id="ChildList[0].LastName"]').type(childLastName);
        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');
        cy.contains('button', 'Save and continue').click();

        //Example of how to add a single file
        // cy.url().should('include', '/UploadEvidence');
        // cy.fixture('TestImage.png').then(fileContent => {
        //     cy.get('input[type="file"]').attachFile({
        //         fileContent,
        //         fileName: 'TestImage.png',
        //         mimeType: 'image/png'
        //     });
        // });

        // Load files from fixtures folder
        cy.url().should('include', '/UploadEvidence');
        cy.fixture('TestImage.png').then(fileContent1 => {
            cy.fixture('TestImage2.png').then(fileContent2 => {
                cy.get('input[type="file"]').attachFile([
                    {
                        fileContent: fileContent1,
                        fileName: 'TestImage.png',
                        mimeType: 'image/png'
                    },
                    {
                        fileContent: fileContent2,
                        fileName: 'TestImage2.png',
                        mimeType: 'image/png'
                    }
                ]);
            });
        });

        cy.contains('button', 'Attach evidence').click();

        cy.get('h1').should('include.text', 'Check your answers before submitting');

        cy.CheckValuesInSummaryCard('Evidence', "TestImage.png", "Uploaded");
        cy.CheckValuesInSummaryCard('Evidence', "TestImage2.png", "Uploaded");
    });

    it('Will allow a school user to create an application and add reach Check_Answers page without uploading any evidence files', () => {
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

        cy.url().should('include', 'Check/Loader');
        cy.get('p.govuk-notification-banner__heading', { timeout: 80000 }).should('include.text', 'The children of this parent or guardian may not be eligible for free school meals');
        cy.contains('a.govuk-button', 'Appeal now').click();

        cy.url().should('include', '/Enter_Child_Details');
        cy.get('[id="ChildList[0].FirstName"]').type(childFirstName);
        cy.get('[id="ChildList[0].LastName"]').type(childLastName);
        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');
        cy.contains('button', 'Save and continue').click();

        cy.get('h1').should('include.text', 'Send supporting evidence');
        cy.contains('button', 'Send by email later').click();

        cy.get('h1').should('include.text', 'Check your answers before submitting');
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Name', `${parentFirstName} ${parentLastName}`);
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Date of birth', '1 January 1990');
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'National Insurance number', NIN);
        cy.CheckValuesInSummaryCard('Parent or guardian details', 'Email address', parentEmailAddress);
        cy.CheckValuesInSummaryCard('Child 1 details', "Name", childFirstName + " " + childLastName);

        cy.contains('h2', 'Evidence').should('not.exist');
    });

    it('Will reject files that are not pngs', () => {
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

        cy.url().should('include', 'Check/Loader');
        cy.get('p.govuk-notification-banner__heading', { timeout: 80000 }).should('include.text', 'The children of this parent or guardian may not be eligible for free school meals');
        cy.contains('a.govuk-button', 'Appeal now').click();

        cy.url().should('include', '/Enter_Child_Details');
        cy.get('[id="ChildList[0].FirstName"]').type(childFirstName);
        cy.get('[id="ChildList[0].LastName"]').type(childLastName);
        cy.get('[id="ChildList[0].Day"]').type('01');
        cy.get('[id="ChildList[0].Month"]').type('01');
        cy.get('[id="ChildList[0].Year"]').type('2007');
        cy.contains('button', 'Save and continue').click();

        // Load files from fixtures folder
        cy.url().should('include', '/UploadEvidence');
        cy.fixture('TestFile1.txt').then(fileContent1 => {
            cy.fixture('TestFile2.txt').then(fileContent2 => {
                cy.get('input[type="file"]').attachFile([
                    {
                        fileContent: fileContent1,
                        fileName: 'TestFile1.txt',
                        mimeType: 'text/plain'
                    },
                    {
                        fileContent: fileContent2,
                        fileName: 'TestFile2.txt',
                        mimeType: 'text/plain'
                    }
                ]);
            });
        });

        cy.contains('button', 'Attach evidence').click();

        cy.contains('The selected file must be a JPG, JPEG, HEIC, HEIF, BMP, PNG, TIF, or PDF');
    });
});