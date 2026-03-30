describe('Admin Bulk Check Journey', () => {
    beforeEach(() => {
        cy.checkSession('school');
        cy.visit((Cypress.config().baseUrl ?? "") + "/home");
        cy.contains('Run a batch check').click();
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

    it("will return an error message if the bulk file contains more than 250 rows of data", () => {
        cy.fixture("BulkCheckFileValidaiton/bulkchecktemplate_too_many_records.csv").then(
            (fileContent1) => {
                cy.get('input[type="file"]').attachFile([
                    {
                        fileContent: fileContent1,
                        fileName: "bulkchecktemplate_too_many_records.csv",
                        mimeType: "text/csv",
                    },
                ]);
            }
        );
        cy.contains('button', 'Run check').click();
        cy.get("#file-upload-1-error").as("errorMessage");
        cy.get("@errorMessage").should(($p) => {
            expect($p.first()).to.contain(
                "CSV File cannot contain more than 250 records"
            );
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
            cy.fixture("BulkCheckFileValidaiton/bulkchecktemplate_too_many_records.csv").then(
                (fileContent1) => {
                    cy.get('input[type="file"]').attachFile([
                        {
                            fileContent: fileContent1,
                            fileName: "bulkchecktemplate_too_many_records.csv",
                            mimeType: "text/csv",
                        },
                    ]);
                }
            );
            cy.contains('button', 'Run check').click();
        }
        cy.get("#file-upload-1-error").as("errorMessage");
        cy.get("@errorMessage").should(($p) => {
            expect($p.first()).to.contain(
                "No more than 10 batch check requests can be made per hour"
            );
        });
    });
});
