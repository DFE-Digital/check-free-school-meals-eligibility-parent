describe('Admin Bulk Check Loader', () => {
    const loaderHtml = (currentCounter: number, totalCounter: number) => `
        <!doctype html>
        <html lang="en">
          <head>
            <meta charset="utf-8" />
            <title>Checking</title>
          </head>
          <body>
            <div class="govuk-grid-row" id="content" data-url="/BulkCheck/Bulk_Loader" data-type="counter-${currentCounter}">
              <span id="currentCounter">${currentCounter}</span>/
              <span id="totalCounter">${totalCounter} records processed.</span>
            </div>
            <script src="/js/bulk-eligibility-loader.js"></script>
          </body>
        </html>`;

    it('keeps polling after the bulk progress counter updates', () => {
        cy.clock();

        let requestCount = 0;

        cy.intercept('GET', '**/BulkCheck/Bulk_Loader', (req) => {
            requestCount += 1;

            if (requestCount === 1) {
                req.reply({
                    statusCode: 200,
                    body: loaderHtml(10, 250),
                });
                return;
            }

            if (requestCount === 2) {
                req.reply({
                    statusCode: 200,
                    body: loaderHtml(20, 250),
                });
                return;
            }

            req.reply({
                statusCode: 200,
                body: loaderHtml(30, 250),
            });
        }).as('bulkLoaderPage');

        cy.visit((Cypress.config().baseUrl ?? '') + '/BulkCheck/Bulk_Loader');

        cy.get('#currentCounter').should('have.text', '10');
        cy.get('#totalCounter').should('have.text', '250 records processed.');

        cy.tick(5000);
        cy.wait('@bulkLoaderPage');
        cy.get('#currentCounter').should('have.text', '20');

        cy.tick(5000);
        cy.wait('@bulkLoaderPage');
        cy.get('#currentCounter').should('have.text', '30');
    });
});