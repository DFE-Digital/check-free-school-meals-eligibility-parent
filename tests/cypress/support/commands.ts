Cypress.Commands.add('CheckValuesInSummaryCard', (sectionTitle: string, key: string, expectedValue: string) => {
  cy.contains('.govuk-summary-card__title', sectionTitle)
  .parents('.govuk-summary-card')
  .within(() => {
    cy.contains('.govuk-summary-list__key', key)
    .siblings('.govuk-summary-list__value')
    .should('include.text', expectedValue)
  });
});

Cypress.Commands.add('scanPagesForValue', (value: string) => {
  cy.get('body').then((body) => {
    if (body.find(`td a:contains("${value}")`).length > 0) {
      cy.get(`td a:contains("${value}")`).click();
    }
    else {
      cy.contains('.govuk-link', 'Next').click();
      cy.scanPagesForValue(value);
    }
  });
});

Cypress.Commands.add('scanPagesForStatusAndClick', (value: string) => {

  cy.get('body').then(($body) => {
    if ($body.text().includes(value)){
      cy.get('tr').contains('strong', value).parents('tr').within(() =>{
        cy.get('a.govuk-link').click();
      });
    } else {
      cy.get('nav.govuk-pagination').contains('a.govuk-pagination__link', 'Next').click().then(() => {
        cy.wait(2000);
        cy.scanPagesForStatusAndClick(value);
    }
  )};
  });
})




// Cypress.Commands.add('findApplicationFinalise', (value: string) => {
//   let referenceFound = false;
//   function searchOnPage() {
//     cy.get('.govuk-table tbody tr').each(($row) => {
//       cy.wrap($row).find('td').eq(1).invoke('text').then((text) => {
//           if (text.trim() === value) {
//               referenceFound = true;
//               cy.wrap($row).find('td').eq(0).find('input[type="checkbox"]').click();
//               return false;
//           }
//       });
//     }).then(() => {
//       if (!referenceFound){
//         cy.contains('.govuk-link', 'Next').click();
//         searchOnPage();
//         cy.debug();
//         // cy.get('body').then((body) => {
//           // if(body.find('.govuk-pagination__link:contains("Next")}').length > 0) {
//           //   cy.contains('.govuk-pagination__link', 'Next').click();
//           //   cy.findApplicationFinalise(value);
//           // }
//           // else{
//           //   cy.log('Reference number not found')
//           // }
//         // });
//       }
//     });
//   }
//   searchOnPage();
// })

Cypress.Commands.add('findApplicationFinalise', (value: string) => {
  let referenceFound = false;
  function searchOnPage() {
    cy.get('.govuk-table tbody tr').each(($row) => {
      cy.wrap($row).find('td').eq(1).invoke('text').then((text) => {
          if (text.trim() === value) {
              referenceFound = true;
              cy.wrap($row).find('td').eq(0).find('input[type="checkbox"]').click();
              return false;
          }
      });
    }).then(() => {
      if (!referenceFound){
        cy.get('.govuk-link').contains('Next').then(($nextButton) => {
          if($nextButton.length > 0){
            cy.wrap($nextButton).click().then(() => {
              cy.wait(500);
              searchOnPage();
            })
          } else {
            cy.log('Reference number could not be found');
          }
        })
      }
    });
  }
  searchOnPage();
});


Cypress.Commands.add('verifyFieldVisibility', (selector: string, isVisible: boolean) => {
  if (isVisible) {
    cy.get(selector).should('be.visible');
  } else {
    cy.get(selector).should('not.be.visible');
  }
});


Cypress.Commands.add('verifyH1Text', (expectedText: string) => {
  cy.contains('h1', expectedText).should('be.visible');
  cy.get('h1').invoke('text').then((actualText: string) => {
    expect(actualText.trim()).to.eq(expectedText); 
  });
});

Cypress.Commands.add('selectYesNoOption', (baseSelector: string, isYes: boolean) => {
  const finalSelector = isYes ? `${baseSelector}[value="true"]` : `${baseSelector}[value="false"]`;
  cy.log(`selector being used: ${finalSelector}`)
  cy.get(finalSelector).click();
});

Cypress.Commands.add('retainAuthOnRedirect', (initialUrl, authHeader, alias) => {
  let redirectUrl: string;

  cy.intercept(initialUrl, (req) => {
    req.continue((res) => {
      const locationHeader = res.headers['location'];
      if (Array.isArray(locationHeader)) {
        redirectUrl = locationHeader[0];
      } else {
        redirectUrl = locationHeader;
      }
    });
  }).as('initialRequest');

  cy.request({
    url: initialUrl,
    headers: {
      'Authorization': authHeader,
    },
    followRedirect: false,
  }).then(() => {
    expect(redirectUrl).to.exist;

    cy.request({
      url: redirectUrl,
      headers: {
        'Authorization': authHeader,
      }
    }).as(alias);
  });
});

