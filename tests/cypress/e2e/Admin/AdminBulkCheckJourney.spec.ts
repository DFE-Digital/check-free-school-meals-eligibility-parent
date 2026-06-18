const bulkUploadAttemptLimit = Number(Cypress.env('BULK_UPLOAD_ATTEMPT_LIMIT') ?? 10);
const bulkOverLimitRowCount = Number(Cypress.env('BULK_OVER_LIMIT_ROW_COUNT') ?? 6001);

const createBulkCsv = (rowCount: number): string => {
    const header = 'Parent National Insurance number,Parent asylum support reference number,Parent Date of Birth,Parent Last Name';
    const rows = Array.from({ length: rowCount }, (_, index) => {
        const day = ((index % 28) + 1).toString().padStart(2, '0');
        return `AB123456E,,${day}/01/2000,SURNAME${index}`;
    });

    return [header, ...rows].join('\n');
};

describe('Admin Bulk Check Journey', () => {
    beforeEach(() => {
        cy.checkSession('school');
        cy.visit((Cypress.config().baseUrl ?? "") + "/home");
        cy.contains('Run a batch check').click();
        cy.url().should('include', 'Bulk_Check');
    });

    it("will return an error message if the bulk file contains headers that don't match the template", () => {
        cy.fixture("BulkCheckFileValidaiton/bulkchecktemplate_invalid_headers.csv").then(
            (fileContent1) => {
                cy.get('input[type="file"]').attachFile([
                    {
                        fileContent: fileContent1,
                        fileName: "bulkchecktemplate_invalid_headers.csv",
                        mimeType: "text/csv",
                    },
                ]);
            }
        );
        cy.contains('button', 'Run check').click();
        cy.get("#file-upload-1-error").as("errorMessage");
        cy.get("@errorMessage").should(($p) => {
            expect($p.first()).to.contain(
                "The column headings in the selected file must exactly match the template"
            );
        });
    });

    it("will return an error message if the bulk file contains more than the configured row limit", () => {
        const overLimitCsv = createBulkCsv(bulkOverLimitRowCount);
        cy.get('input[type="file"]').attachFile([
            {
                fileContent: overLimitCsv,
                fileName: "bulkcheck_over_limit.csv",
                mimeType: "text/csv",
            },
        ]);
        cy.contains('button', 'Run check').click();
        cy.get("#file-upload-1-error").as("errorMessage");
        cy.get("@errorMessage").should(($p) => {
            expect($p.first().text()).to.match(/CSV File cannot contain more than\s+\d+\s+records/);
        });
    });

    it("will run a successful batch check", () => {
        cy.fixture("BulkCheckFileValidaiton/bulkchecktemplate_complete.csv").then(
            (fileContent1) => {
                cy.get('input[type="file"]').attachFile([
                    {
                        fileContent: fileContent1,
                        fileName: "bulkchecktemplate_complete.csv",
                        mimeType: "text/csv",
                    },
                ]);
            }
        );
        cy.contains('Run check').click();
        cy.get('h1', { timeout: 80000 }).should('include.text', 'Checks completed');
        cy.contains("Download").click();
    });

    it("will return an error message if more than 10 batches are attempted within an hour", () => {
        for (let i = 0; i < 11; i++) {
            cy.fixture("BulkCheckFileValidaiton/bulkchecktemplate_invalid_headers.csv").then(
                (fileContent1) => {
                    cy.get('input[type="file"]').attachFile([
                        {
                            fileContent: fileContent1,
                            fileName: "bulkchecktemplate_invalid_headers.csv",
                            mimeType: "text/csv",
                        },
                    ]);
                }
            );
            cy.contains('button', 'Run check').click();
            cy.url().then(url => cy.log(`After submit ${i}: ${url}`));
        }
        cy.get("#file-upload-1-error").as("errorMessage");
        cy.get("@errorMessage").should(($p) => {
            expect($p.first()).to.contain(
                "No more than 10 batch check requests can be made per hour"
            );
        });
    });
});
