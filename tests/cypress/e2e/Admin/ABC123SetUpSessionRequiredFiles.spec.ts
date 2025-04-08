describe('Ensure file location for cookies exists', () => {
    it("will return an error message if the bulk file contains more than 250 rows of data", () => {
        cy.storeCookies('school');
        cy.storeCookies('LA');
    });
});
