
describe('Keyword search validation', () => {
  const parentFirstName = 'Sinkler';
  const parentLastName = Cypress.env('lastName');
  const parentEmailAddress = 'TimJones@Example.com';
  const NIN = 'PN668797B'
  const childFirstName = 'Timmy';
  const childLastName = 'Smith';
  let referenceNumber: string;
  let found = false;
  let index = 0;

  beforeEach(() => {
    cy.checkSession('school');
    cy.visit(Cypress.config().baseUrl ?? "/");
  });

  it('Will allow a school user to create an application that may not be elligible and send it for appeal', () => {
    //single check
    cy.contains('Run a check for one parent or guardian').click();
    cy.get('#consent').check();
    cy.get('#submitButton').click();

    //enter parent details
    cy.url().should('include', '/Check/Enter_Details');
    cy.get('#FirstName').type(parentFirstName);
    cy.get('#LastName').type(parentLastName);
    cy.get('#EmailAddress').type(parentEmailAddress);
    cy.get('[id="DateOfBirth.Day"]').type('01');
    cy.get('[id="DateOfBirth.Month"]').type('01');
    cy.get('[id="DateOfBirth.Year"]').type('1990');

    cy.get('#NinAsrSelection').click();
    cy.get('#NationalInsuranceNumber').type(NIN);

    cy.contains('button', 'Perform check').click();

    //loader page and result
    cy.url().should('include', 'Check/Loader');
    cy.get('p.govuk-notification-banner__heading', { timeout: 80000 }).should('include.text', 'The children of this parent or guardian may not be eligible for free school meals');
    cy.contains('a.govuk-button', 'Appeal now').click();

    //enter child details
    cy.url().should('include', '/Enter_Child_Details');
    cy.get('[id="ChildList[0].FirstName"]').type(childFirstName);
    cy.get('[id="ChildList[0].LastName"]').type(childLastName);
    cy.get('[id="ChildList[0].DateOfBirth.Day"]').type('01');
    cy.get('[id="ChildList[0].DateOfBirth.Month"]').type('01');
    cy.get('[id="ChildList[0].DateOfBirth.Year"]').type('2007');
    cy.contains('button', 'Save and continue').click();

    //skip uploading evidence
    cy.get('h1').should('include.text', 'Send supporting evidence');
    cy.contains('a', 'Send by email later').click();

    //verify the details match expected values
    cy.get('h1').should('include.text', 'Check your answers before submitting');
    cy.CheckValuesInSummaryCard('Parent or guardian details', 'Name', `${parentFirstName} ${parentLastName}`);
    cy.CheckValuesInSummaryCard('Parent or guardian details', 'Date of birth', '1 January 1990');
    cy.CheckValuesInSummaryCard('Parent or guardian details', 'National Insurance number', NIN);
    cy.CheckValuesInSummaryCard('Parent or guardian details', 'Email address', parentEmailAddress);
    cy.CheckValuesInSummaryCard('Child 1 details', "Name", childFirstName + " " + childLastName);
    cy.contains('button', 'Add details').click();

    //verify add details completed successfully 
    cy.url().should('include', '/Check/AppealsRegistered');
    cy.get('.govuk-table')
      .find('tbody tr')
      .eq(0)
      .find('td')
      .eq(1)
      .invoke('text')
      .then((text) => {
        referenceNumber = text.trim().toString();
        cy.wrap('referenceNumber').as(referenceNumber);
      });
  });

  it('Returns the correct record when searching Parent First Name ', () => {
    cy.contains('Search all records').click();
    cy.get("#Keyword").type(parentFirstName);
    cy.contains(".govuk-button", "Apply filters").click();
    cy.wait(100);
    cy.get('.govuk-table').find('tbody tr').each(($tr, i) => {
      if (!found) {
        cy.wrap($tr).find('td').each(($td) => {
          if ($td.text().includes(referenceNumber)) {
            found = true;
            index = i;
            return false;
          }
        });
      }
    })
  });

  it('Returns the correct record when searching Parent Last Name ', () => {
    cy.contains('Search all records').click();
    cy.get("#Keyword").type(parentLastName);
    cy.contains(".govuk-button", "Apply filters").click();
    cy.wait(100);
    cy.get('.govuk-table').find('tbody tr').each(($tr, i) => {
      if (!found) {
        cy.wrap($tr).find('td').each(($td) => {
          if ($td.text().includes(referenceNumber)) {
            found = true;
            index = i;
            return false;
          }
        });
      }
    })
  });
  it('Returns the correct record when searching Child First Name ', () => {
    cy.contains('Search all records').click();
    cy.get("#Keyword").type(childFirstName);
    cy.contains(".govuk-button", "Apply filters").click();
    cy.wait(100);
    cy.get('.govuk-table').find('tbody tr').each(($tr, i) => {
      if (!found) {
        cy.wrap($tr).find('td').each(($td) => {
          if ($td.text().includes(referenceNumber)) {
            found = true;
            index = i;
            return false;
          }
        });
      }
    })
  });

  it('Returns the correct record when searching Child Last Name ', () => {
    cy.contains('Search all records').click();
    cy.get("#Keyword").type(childLastName);
    cy.contains(".govuk-button", "Apply filters").click();
    cy.wait(100);
    cy.get('.govuk-table').find('tbody tr').each(($tr, i) => {
      if (!found) {
        cy.wrap($tr).find('td').each(($td) => {
          if ($td.text().includes(referenceNumber)) {
            found = true;
            index = i;
            return false;
          }
        });
      }
    })
  });

  it('Returns the correct record when searching National Insurance number ', () => {
    cy.contains('Search all records').click();
    cy.get("#Keyword").type(NIN);
    cy.contains(".govuk-button", "Apply filters").click();
    cy.wait(100);
    cy.get('.govuk-table').find('tbody tr').each(($tr, i) => {
      if (!found) {
        cy.wrap($tr).find('td').each(($td) => {
          if ($td.text().includes(referenceNumber)) {
            found = true;
            index = i;
            return false;
          }
        });
      }
    })
  });

  it('Returns date filtered results when a radio is selected and filter can be removed', () => {
    cy.contains('Search all records').click();
    cy.get("#DateRangeNow").click();
    cy.contains(".govuk-button", "Apply filters").click();
    cy.wait(100);
    cy.contains('td.govuk-table__cell', 'No results found.').should('not.exist');
    cy.contains(".moj-filter__tag", "Current month to date").click();
    cy.contains(".moj-filter__tag").should('not.exist');
  });

  it('Returns status filtered results when a Status is selected and filter can be removed', () => {
    cy.contains('Search all records').click();
    cy.get("#Status_ReviewedEntitled").click();
    cy.contains(".govuk-button", "Apply filters").click();
    cy.wait(100);
    cy.contains('td.govuk-table__cell', 'No results found.').should('not.exist');
    cy.contains(".moj-filter__tag", "Reviewed Entitled").click();
    cy.contains(".moj-filter__tag").should('not.exist');
  });

  it('Returns the record when First and Last name of Parent are searched', () => {
    cy.contains('Search all records').click();
    cy.get("#Keyword").type(parentFirstName + " " + parentLastName);
    cy.contains(".govuk-button", "Apply filters").click();
    cy.wait(100);
    cy.get('.govuk-table').find('tbody tr').each(($tr, i) => {
      if (!found) {
        cy.wrap($tr).find('td').each(($td) => {
          if ($td.text().includes(referenceNumber)) {
            found = true;
            index = i;
            return false;
          }
        });
      }
    })
  });
});
