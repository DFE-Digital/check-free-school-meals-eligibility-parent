describe('Basic LA Happy Path', () => {

    const baseUrl = Cypress.config().baseUrl ?? "";

    let skipSetupBasic = false;

    const fixtures = {
        invalidHeaderContent: "BulkCheckFileValidaiton/BASIC-bulkchecktemplate_invalid_HeadersContent.csv",
        invalidHeaderStructure: "BulkCheckFileValidaiton/BASIC-bulkchecktemplate_invalid_HeadersSequenceOrCount.csv",
        tooManyRecords: "BulkCheckFileValidaiton/BASIC-bulkchecktemplate_too_many_records.csv",
        valid: "BulkCheckFileValidaiton/BASIC-bulkchecktemplate_complete.csv",
        curlyApostrophe: "BulkCheckFileValidaiton/BASIC-bulkchecktemplate_curly_apostrophe.csv"
    };

    const uploadFixture = (fixturePath: string) => {
        cy.fixture(fixturePath).then((fileContent) => {
            cy.get('input[type="file"]').attachFile({
                fileContent,
                fileName: fixturePath.split('/').pop(),
                mimeType: "text/csv",
            });
        });
    };

    const submitBatch = () => {
        cy.contains('button', 'Run a batch check').click();
    };

    beforeEach(() => {
        if (!skipSetupBasic) {
            cy.checkSession('basic');
            cy.visit(`${baseUrl}/home`);

            cy.get('.govuk-caption-l')
                .should('include.text', 'Manchester City Council');

            cy.contains('Run a batch check').click();
            cy.url().should('include', '/BulkCheck/Bulk_Check');
        }
    });

    it("validates missing required header content", () => {
        uploadFixture(fixtures.invalidHeaderContent);
        submitBatch();

        cy.get("#file-upload-1-error")
            .should('contain',
                "Invalid CSV format. Missing required header: 'Parent Date of Birth'"
            );
    });

    it("validates header structure and sequence", () => {
        uploadFixture(fixtures.invalidHeaderStructure);
        submitBatch();

        cy.get("#file-upload-1-error")
            .should('contain',
                "The column headers in the selected file must exactly match the template"
            );
    });

    it("validates max row limit", () => {
        uploadFixture(fixtures.tooManyRecords);
        submitBatch();

        cy.get("#file-upload-1-error")
            .should('contain',
                "CSV file cannot contain more than 250 records"
            );
    });

    it("runs a successful batch check", () => {
        uploadFixture(fixtures.valid);
        submitBatch();

        cy.get('h1', { timeout: 80000 })
            .should('include.text', 'Batch checks history');

        const today = new Date().toLocaleDateString('en-GB', {
            day: '2-digit',
            month: 'short',
            year: 'numeric'
        }).replace(',', '');

        cy.contains('table tbody tr', 'BASIC-bulkchecktemplate_complete.csv', { timeout: 80000 })
            .should('exist')
            .within(() => {
                cy.get('td').eq(0).invoke('text').should('match', /\.csv$/i);
                cy.get('td').eq(1).should('have.text', '15');
                cy.get('td').eq(2).should('have.text', 'Tester');
                cy.get('td').eq(3).should('contain.text', today);
                cy.get('td').eq(4).invoke('text').should('not.be.empty');
                cy.get('td').eq(5).find('strong').should('have.class', 'govuk-tag');
                cy.get('td').eq(6).should('contain.text', 'Delete');
            });
    });

    it("handles curly apostrophe names", () => {
        uploadFixture(fixtures.curlyApostrophe);

        cy.get('input[type="file"]')
            .should(($input) => {
                expect(($input[0] as HTMLInputElement).files?.length).to.eq(1);
            });

        submitBatch();

        cy.get('h1', { timeout: 80000 })
            .should('include.text', 'Batch checks history');

        cy.contains(
            'table tbody tr',
            'BASIC-bulkchecktemplate_curly_apostrophe.csv',
            { timeout: 80000 }
        ).should('exist');
    });

    it("deletes a batch check if available", () => {
        cy.contains('a', 'Batch checks history').click();

        cy.get('h1')
            .should('include.text', 'Batch checks history');

        cy.get('table tbody tr td:nth-child(7) a')
            .filter((_, el) => /delete/i.test(el.innerText))
            .then($links => {
                if ($links.length === 0) {
                    cy.log("No deletable records found.");
                    return;
                }

                cy.wrap($links[0]).click();

                cy.get('h3.govuk-notification-banner__heading')
                    .should('contain.text', 'Batch check deleted successfully.');
            });
    });

    it("enforces batch attempt limit", () => {
        for (let i = 0; i <= 10; i++) {
            uploadFixture(fixtures.tooManyRecords);
            submitBatch();
        }

        cy.get("#file-upload-1-error")
            .should('contain',
                "You have exceeded the maximum number of bulk upload attempts. Please try again later."
            );
    });

});