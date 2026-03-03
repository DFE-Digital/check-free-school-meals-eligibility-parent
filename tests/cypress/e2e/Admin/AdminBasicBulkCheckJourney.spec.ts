describe('BasicLAHappyPath', () => {
    let skipSetupBasic = false
    beforeEach(() => {
        if (!skipSetupBasic) {
            cy.checkSession('basic');
            cy.visit(Cypress.config().baseUrl ?? "");
            cy.wait(1);
            cy.get('h1').should('include.text', 'MANCHESTER CITY COUNCIL');
            cy.contains('Run a batch check').click();
            cy.url().should('include', '/BulkCheckFsmBasic/Bulk_Check_FSMB');
        }
    });

    it("will return an error message if the bulk file contains header content that doesn't match the template", () => {
        cy.fixture("BulkcheckFileValidaiton/BASIC-bulkchecktemplate_invalid_HeadersContent.csv").then(
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
        cy.get("#file-upload-1-error").as("errorMessage");
        cy.get("@errorMessage").should(($p) => {
            expect($p.first()).to.contain(
                "Invalid CSV format. Missing required header: 'Parent Date of Birth'"
            );
        });
    });

    it("will return an error message if the bulk file contains wrong number of headers or out of sequence headers", () => {
        cy.fixture("BulkcheckFileValidaiton/BASIC-bulkchecktemplate_invalid_HeadersSequenceOrCount.csv").then(
            (fileContent1) => {
                cy.get('input[type="file"]').attachFile([
                    {
                        fileContent: fileContent1,
                        fileName: "BASIC-bulkchecktemplate_invalid_HeadersSequenceOrCount.csv",
                        mimeType: "text/csv",
                    },
                ]);
            }
        );
        cy.contains('button', 'Run a batch check').click();
        cy.get("#file-upload-1-error").as("errorMessage");
        cy.get("@errorMessage").should(($p) => {
            expect($p.first()).to.contain(
                "The column headers in the selected file must exactly match the template"
            );
        });
    });

    it("will return an error message if the bulk file contains more than 250 rows of data", () => {
        cy.fixture("BulkcheckFileValidaiton/BASIC-bulkchecktemplate_too_many_records.csv").then(
            (fileContent1) => {
                cy.get('input[type="file"]').attachFile([
                    {
                        fileContent: fileContent1,
                        fileName: "BASIC-bulkchecktemplate_too_many_records.csv",
                        mimeType: "text/csv",
                    },
                ]);
            }
        );
        cy.contains('button', 'Run a batch check').click();
        cy.get("#file-upload-1-error").as("errorMessage");
        cy.get("@errorMessage").should(($p) => {
            expect($p.first()).to.contain(
                "CSV file cannot contain more than 250 records"
            );
        });
    });

    it("will return an error message if more than 10 batches are attempted within an hour", () => {
        for (let i = 0; i < 11; i++) {
            cy.fixture("BulkcheckFileValidaiton/BASIC-bulkchecktemplate_too_many_records.csv").then(
                (fileContent1) => {
                    cy.get('input[type="file"]').attachFile([
                        {
                            fileContent: fileContent1,
                            fileName: "BASIC-bulkchecktemplate_too_many_records.csv",
                            mimeType: "text/csv",
                        },
                    ]);
                }
            );
            cy.contains('button', 'Run a batch check').click();
        }
        cy.get("#file-upload-1-error").as("errorMessage");
        cy.get("@errorMessage").should(($p) => {
            expect($p.first()).to.contain(
                "You have exceeded the maximum number of bulk upload attempts. Please try again later."
            );
        });
    });

    it("will run a successful batch check", () => {
        cy.fixture("BulkcheckFileValidaiton/BASIC-bulkchecktemplate_complete.csv").then(
            (fileContent1) => {
                cy.get('input[type="file"]').attachFile([
                    {
                        fileContent: fileContent1,
                        fileName: "BASIC-bulkchecktemplate_complete.csv",
                        mimeType: "text/csv",
                    },
                ]);
            }
        );
        cy.contains('button', 'Run a batch check').click();
        cy.get('h1', { timeout: 80000 }).should('include.text', 'Batch checks history');

        const today = new Date().toLocaleDateString('en-GB', {
            day: '2-digit',
            month: 'short',
            year: 'numeric'
        }).replace(',', ''); // e.g. "16 Feb 2026"

        cy.get('table tbody tr').first().within(() => {
            cy.get('td').eq(0).should('have.text', 'BASIC-bulkchecktemplate_complete.csv');
            cy.get('td').eq(1).should('have.text', '15');
            cy.get('td').eq(2).should('have.text', 'Tester');
            cy.get('td').eq(3).should('contain.text', today);
            cy.get('td').eq(4).invoke('text').then(t => t.trim()).should('not.be.empty');
            cy.get('td').eq(5).find('strong').should('have.class', 'govuk-tag');
            cy.get('td').eq(6).should('contain.text', 'Delete');
        });
    });

    it("Navigate to Batch checks history and delete a batch check if one exists", () => {
        cy.contains('a', 'Batch checks history').click();
        cy.get('h1', { timeout: 80000 }).should('include.text', 'Batch checks history');


        cy.get('table tbody tr td:nth-child(7) a') // Get all delete links
            .filter((_, el) => /delete/i.test(el.innerText))
            .then($links => {

                if ($links.length === 0) {
                    cy.log("No delete links found - probably In Progress or no completed checks.");
                    return;
                }

                cy.wrap($links[0]).click();

                cy.get('h3.govuk-notification-banner__heading')
                    .should('contain.text', 'Batch check deleted successfully.');
            });
    });
});