const bulkBasicUploadAttemptLimit = Number(Cypress.env('BULK_UPLOAD_ATTEMPT_LIMIT') ?? 10);
const bulkBasicOverLimitRowCount = Number(Cypress.env('BULK_OVER_LIMIT_ROW_COUNT') ?? 6001);

const createBasicBulkCsv = (rowCount: number): string => {
    const header = 'Parent Last Name,Parent Date of Birth,Parent National Insurance number';
    const rows = Array.from({ length: rowCount }, (_, index) => {
        const day = ((index % 28) + 1).toString().padStart(2, '0');
        return `SURNAME${index},${day}/01/2000,AB123456E`;
    });
    return [header, ...rows].join('\n');
};

describe('BasicLAHappyPath', () => {
    let skipSetupBasic = false
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

    
    it("will return an error message if the bulk file contains more than the configured row limit", () => {
        const overLimitCsv = createBasicBulkCsv(bulkBasicOverLimitRowCount);
        cy.get('input[type="file"]').attachFile([
            {
                fileContent: overLimitCsv,
                fileName: "bulkcheck_over_limit.csv",
                mimeType: "text/csv",
            },
        ]);
        cy.contains('button', 'Run a batch check').click();
        cy.get("#file-upload-1-error").as("errorMessage");
        cy.get("@errorMessage").should(($p) => {
            expect($p.first()).to.contain(
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

    it("will run a successful batch check when last name contains a curly apostrophe", () => {
        cy.fixture("BulkCheckFileValidaiton/BASIC-bulkchecktemplate_curly_apostrophe.csv").then(
            (fileContent1) => {
                cy.get('input[type="file"]').attachFile([
                    {
                        fileContent: fileContent1,
                        fileName: "BASIC-bulkchecktemplate_curly_apostrophe.csv",
                        mimeType: "text/csv",
                    },
                ]);
            }
        );
        cy.get('input[type="file"]').attachFile(
            "BulkCheckFileValidaiton/BASIC-bulkchecktemplate_curly_apostrophe.csv"
        );
    
        cy.get('input[type="file"]').should(($input) => {
            expect(($input[0] as HTMLInputElement).files?.length).to.eq(1);
        });

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

    it("will return an error message if more than 10 batches are attempted within an hour", () => {
        for (let i = 0; i < 11; i++) {
            cy.fixture("BulkCheckFileValidaiton/BASIC-bulkchecktemplate_invalid_HeadersContent.csv").then(
                (fileContent1) => {
                    cy.get('input[type="file"]').attachFile([
                        {
                            fileContent: fileContent1,
                            fileName: "BASIC-bulkchecktemplate_invalid_HeadersContent.csv",
                            mimeType: "text/csv",
                        },
                    ]);
                }
            );
            cy.contains('button', 'Run a batch check').click();
        }

        cy.get("#file-upload-1-error")
            .should('contain',
                "You have exceeded the maximum number of bulk upload attempts. Please try again later."
            );
    });
    
});