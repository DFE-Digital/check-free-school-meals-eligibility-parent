describe('Admin Bulk Check Journey', () => {
    beforeEach(() => {
        cy.checkSession('school');
        cy.visit(Cypress.config().baseUrl ?? "");
        cy.contains('Run a batch check').click();
    });

    it("will return an error message if the bulk file contains more than 250 rows of data", () => {
        cy.get('input[type=file]').selectFile('bulkchecktemplate_too_many_records.csv');
        cy.contains('Run check').click();
        cy.get('#file-upload-1-error').as('errorMessage');
        cy.get('@errorMessage').should(($p) => {
            expect($p.first()).to.contain('CSV File cannot contain more than 250 records');
        });
    });

    it("will return an error message if more than 10 batches are attempted within an hour", () => {
        for (let i = 0; i < 11; i++) {
            cy.get('input[type=file]').selectFile('bulkchecktemplate_too_many_records.csv');
            cy.contains('Run check').click();
        }
        cy.get('#file-upload-1-error').as('errorMessage');
        cy.get('@errorMessage').should(($p) => {
            expect($p.first()).to.contain('No more than 10 batch check requests can be made per hour');
        });
    });

    it("will run a successful batch check", () => {
        cy.get('input[type=file]').selectFile('bulkchecktemplate_complete.csv');
        cy.contains('Run check').click();
        cy.get('h1', { timeout: 80000 }).should('include.text', 'Checks completed');
        cy.contains("Download").click();
    });
});
