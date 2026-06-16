const bulkUploadAttemptLimit = Number(Cypress.env('BULK_UPLOAD_ATTEMPT_LIMIT') ?? 10);
const bulkOverLimitRowCount = Number(Cypress.env('BULK_OVER_LIMIT_ROW_COUNT') ?? 6001);

//session configuration
const sessionConfigs = {
    school: {
        fixtureValid: "BulkCheckFileValidaiton/bulkchecktemplate_complete.csv",
        fixtureInvalid: "BulkCheckFileValidaiton/bulkchecktemplate_invalid_headers.csv",
        includeSchoolURN: false
    },
    LA: {
        fixtureValid: "BulkCheckFileValidaiton/bulkchecktemplate_complete_LA.csv",
        fixtureInvalid: "BulkCheckFileValidaiton/bulkchecktemplate_invalid_headers_LA.csv",
        includeSchoolURN: true
    }
};

//dynamic CSV generator
const createBulkCsv = (rowCount: number, includeSchoolURN: boolean): string => {
    const baseHeader =
        'Parent Last Name,Parent Date of Birth,Parent National Insurance number,Child First Name,Child Last Name,Child Date of Birth';

    const header = includeSchoolURN
        ? `${baseHeader},Child School URN`
        : baseHeader;

    const rows = Array.from({ length: rowCount }, (_, index) => {
        const day = ((index % 28) + 1).toString().padStart(2, '0');

        const baseRow = [
            `SURNAME${index}`,
            `${day}/01/2000`,
            `AB123456E`,
            `CHILD${index}`,
            `SURNAME${index}`,
            `${day}/01/2010`
        ];

        if (includeSchoolURN) {
            baseRow.push('123417');
        }

        return baseRow.join(",");
    });

    return [header, ...rows].join("\n");
};

//helper upload
const uploadFile = (fileContent: any, fileName: string) => {
    cy.get('input[type="file"]').attachFile([
        {
            fileContent,
            fileName,
            mimeType: 'text/csv',
        },
    ]);
};

// loop sessions
Object.entries(sessionConfigs).forEach(([sessionType, config]) => {

    describe(`Admin Bulk Check Journey (${sessionType})`, () => {

        beforeEach(() => {
            cy.checkSession(sessionType);
            cy.visit((Cypress.config().baseUrl ?? "") + "/home");
            cy.contains('Run a batch check').click();
            cy.url().should('include', 'Bulk_Check');
        });

        it("returns error for invalid headers", () => {
            cy.fixture(config.fixtureInvalid).then((fileContent) => {
                uploadFile(fileContent, "invalid_headers.csv");
            });

            cy.contains('button', 'Run a batch check').click();

            cy.get("#file-upload-1-error")
                .should('contain',
                    "The column headers in the selected file must exactly match the template"
                );
        });

        it("returns error when row limit exceeded", () => {
            const csv = createBulkCsv(bulkOverLimitRowCount, config.includeSchoolURN);

            uploadFile(csv, "over_limit.csv");

            cy.contains('button', 'Run a batch check').click();

            cy.get("#file-upload-1-error")
                .should('contain',
                    "CSV file cannot contain more than 250 records"
                );
        });

        it("runs a successful batch check", () => {
            cy.fixture(config.fixtureValid).then((fileContent) => {
                uploadFile(fileContent, "valid.csv");
            });

             cy.contains('button', 'Run a batch check').click();

           cy.get('h1', { timeout: 80000 })
            .should('include.text', 'Batch checks history');

        const today = new Date().toLocaleDateString('en-GB', {
            day: '2-digit',
            month: 'short',
            year: 'numeric'
        }).replace(',', '');

        cy.contains('table tbody tr', 'valid.csv', { timeout: 80000 })
            .should('exist')
            .within(() => {
                cy.get('td').eq(0).invoke('text').should('match', /\.csv$/i);
                cy.get('td').eq(1).should('have.text', '4');
                cy.get('td').eq(2).should('have.text', 'Tester');
                cy.get('td').eq(3).should('contain.text', today);
                cy.get('td').eq(4).invoke('text').should('not.be.empty');
                cy.get('td').eq(5).find('strong').should('have.class', 'govuk-tag');
                cy.get('td').eq(6).should('contain.text', 'Delete');
            });
        });

        it("returns error after exceeding attempt limit", () => {
            for (let i = 0; i <= bulkUploadAttemptLimit; i++) {
                cy.fixture(config.fixtureInvalid).then((fileContent) => {
                    uploadFile(fileContent, "invalid_headers.csv");
                });

                cy.contains('button', 'Run a batch check').click();
            }

            cy.get("#file-upload-1-error")
                .should('contain',
                    "You have exceeded the maximum number of bulk upload attempts. Please try again later."
                );
        });

    });
});