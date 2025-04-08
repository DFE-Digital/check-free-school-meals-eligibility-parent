describe("Admin Portal pages contain recommended network security related headers", () => {
    it("Pages respond with recommended network security headers", () => {

        cy.checkSession('school');
        
        // check for network headers on a regular page
        cy.request({
            method: 'GET',
            url: '/'
        }).then((response) => {
            console.log(response.headers)
            expect(response.headers).to.have.property('content-security-policy', `default-src 'self' https://*.clarity.ms https://c.bing.com`);
            expect(response.headers).to.have.property('strict-transport-security', 'max-age=31536000; includeSubDomains');
            expect(response.headers).to.have.property('x-xss-protection', '1; mode=block');
            expect(response.headers).to.have.property('x-content-type-options', 'nosniff');
        })
    
        // check for network headers on a error page
        cy.request({
            method: 'GET',
            url: '/error'
        }).then((response) => {
            console.log(response.headers)
            expect(response.headers).to.have.property('content-security-policy', `default-src 'self' https://*.clarity.ms https://c.bing.com`);
            expect(response.headers).to.have.property('strict-transport-security', 'max-age=31536000; includeSubDomains');
            expect(response.headers).to.have.property('x-xss-protection', '1; mode=block');
            expect(response.headers).to.have.property('x-content-type-options', 'nosniff');
        })
        })
    }
);

describe('Clarity', () => {
    var schoolClarityId: string
    var LAClarityId: string

    it('Loads Clarity when it is enabled', () => {
        cy.checkSession('school');

        cy.get('#accept-cookies').click();

        cy.wait(1000);

        cy.get('body')
            .invoke('attr', 'data-clarity')
            .then(($clarity) => {
                    if($clarity)
                        schoolClarityId = $clarity;
                    cy.get('head script[src*="clarity"]');
                }
            );
    });

    it('Does not Clarity when it is disabled', () => {
        cy.checkSession('LA');

        cy.get('#accept-cookies').click();

        cy.wait(1000);

        cy.get('body')
            .invoke('attr', 'data-clarity')
            .then(($clarity) => {
                    if(!$clarity) {
                        cy.get('head script[src*="clarity"]').should('not.exist');
                    }

                    else {
                        LAClarityId = $clarity;
                    }
                }
            );
    });

    it('Loads different Clarity for School and LA', () => {
        if(schoolClarityId) {
            expect(schoolClarityId).not.equal(LAClarityId)
        }
    });
});