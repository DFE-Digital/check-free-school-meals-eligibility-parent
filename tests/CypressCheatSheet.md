# Cypress Testing Cheat Sheet

## Running tests

### Open Cypress UI

```bash
npx cypress open
```

### Run a specific spec

```bash
npx cypress run --spec "cypress/e2e/Admin/AdminReviewEvidenceVisibility.spec.ts" --browser electron
```

### Local environment variables

```bash
CYPRESS_BASE_URL=https://localhost:7228
CYPRESS_API_HOST=https://localhost:7117
```

### Windows CMD

```cmd
set CYPRESS_BASE_URL=https://localhost:7228&& set CYPRESS_API_HOST=https://localhost:7117&& npx cypress run --spec "cypress/e2e/Admin/AdminReviewEvidenceVisibility.spec.ts" --browser electron
```

### PowerShell

```powershell
$env:CYPRESS_BASE_URL="https://localhost:7228"
$env:CYPRESS_API_HOST="https://localhost:7117"
npx cypress run --spec "cypress/e2e/Admin/AdminReviewEvidenceVisibility.spec.ts" --browser electron
```

## Sessions and login

We rely heavily on session reuse via cookies.

```ts
beforeEach(() => {
  cy.checkSession('school');
  cy.visit('/home');
});
```

Common user types in our codebase:

```ts
cy.checkSession('school');
cy.checkSession('schoolCanReviewEvidenceDisabled');
cy.checkSession('matSchoolWithLaFlagDisabled');
cy.checkSession('MAT');
cy.checkSession('LA');
cy.checkSession('basic');
```

Cookie files:

```text
cypress/fixtures/SchoolUserCookies.json
cypress/fixtures/SchoolUserFlagOffCookies.json
cypress/fixtures/MatSchoolFlagOffCookies.json
cypress/fixtures/MATUserCookies.json
cypress/fixtures/LAUserCookies.json
```

If tests behave inconsistently, I delete the relevant cookie file and force a fresh login.

## Navigation

```ts
cy.visit('/home');
```

If I see a `404` on `/home`, I first check that the frontend is running on:

```text
https://localhost:7228
```

## Assertions

```ts
cy.get('h1')
  .should('contain.text', 'Manage eligibility for free school meals');

cy.get('.govuk-caption-l')
  .should('contain.text', 'The Telford Park School');

cy.url().should('include', '/Check/Enter_Details');

cy.contains('Run a check for one parent or guardian')
  .should('be.visible');

cy.contains('Run a check for one parent or guardian').click();
```

## Dashboard tiles

Tiles that should exist:

```ts
cy.contains('Pending applications').should('be.visible');
cy.contains('Guidance for reviewing evidence').should('be.visible');
```

Tiles that should not exist:

```ts
cy.contains('Pending applications').should('not.exist');
cy.contains('Guidance for reviewing evidence').should('not.exist');
```

Other common tiles:

```ts
cy.contains('Search all records').should('be.visible');
cy.contains('Finalise applications').should('be.visible');
cy.contains('Download PDF form').should('be.visible');
```

## Form filling

```ts
cy.get('#FirstName').type('Tim');
cy.get('#LastName').type('Smith');
cy.get('#Nino').type('PN668767B');

cy.get('#consent').check();
cy.get('#submitButton').click();
```

Date of Birth fields need escaping because the IDs contain dots:

```ts
cy.get('#DateOfBirth\\.Day').type('01');
cy.get('#DateOfBirth\\.Month').type('01');
cy.get('#DateOfBirth\\.Year').type('2000');
```

## File upload

```ts
import 'cypress-file-upload';
```

```ts
cy.get('input[type=file]').attachFile('bulk-check-template.csv');
cy.get('#submitButton').click();
```

## Downloads and CSV checks

```ts
cy.contains('Download template').click();
```

```ts
cy.readFile('cypress/downloads/report.csv')
  .should('contain', 'Outcome');
```

## Debugging failures

Run in UI mode first:

```bash
npx cypress open
```

Check:

```text
cypress/screenshots/
cypress/videos/
results/*.xml
```

## Common failure causes in our codebase

### 1. Wrong ports

```text
Frontend: https://localhost:7228
API:      https://localhost:7117
```

### 2. Stale cookies

If login/session behaviour looks odd, I delete the relevant cookie fixture and re-login.

### 3. Menu or tile caching

We cache menu/tile decisions using values such as:

```text
role
LA code
establishment ID
MAT ID
```

If dashboard tiles randomly appear or disappear, I suspect cache key issues first.

### 4. Wrong user role

Different users see different dashboards:

```text
School
MAT
LA
Basic
```

I never assume tiles are universal.

### 5. Brittle text assertions

Bad:

```ts
cy.contains('Very long exact sentence...');
```

Better:

```ts
cy.contains('Pending applications');
```

### 6. Accessibility hidden text

Some accessibility fixes are intentionally not visible on screen.

```ts
cy.get('.govuk-visually-hidden')
  .should('contain.text', 'View generated reports');
```

## Best practices

Avoid fixed waits:

```ts
cy.wait(5000);
```

Prefer waiting for a real page element:

```ts
cy.get('h1').should('be.visible');
```

Useful assertion differences:

```ts
.should('exist')          // in the DOM
.should('be.visible')     // visible to the user
.should('not.exist')      // not in the DOM
.should('not.be.visible') // in the DOM but hidden
```

## QA comment templates

### Pass

```text
QA passed.

Tested locally against:
- FE: https://localhost:7228
- API: https://localhost:7117

Confirmed:
- [feature works]
- [UI correct]
- [CSV/template correct]

Evidence:
- Screenshot attached
```

### Partial or blocked

```text
QA partially passed.

Confirmed:
- [what works]

Could not confirm:
- [what is blocked]

Reason:
- [why]

Recommendation:
- [next step]
```

## Debugging order

When something fails, I check in this order:

```text
1. Environment: ports and app running
2. Session or cookies
3. Secrets or config
4. User role
5. Feature flags or caching
6. Selector or content change
7. Actual bug
```

## 🧰 Useful Scripts

These are the commands we actually use day-to-day to avoid setup friction.

---

### Run Cypress with correct local environment (Windows CMD)

```cmd
set CYPRESS_BASE_URL=https://localhost:7228&& set CYPRESS_API_HOST=https://localhost:7117&& npx cypress run --browser electron
```

---

### Run a specific spec (Windows CMD)

```cmd
set CYPRESS_BASE_URL=https://localhost:7228&& set CYPRESS_API_HOST=https://localhost:7117&& npx cypress run --spec "cypress/e2e/Admin/AdminReviewEvidenceVisibility.spec.ts" --browser electron
```

---

### PowerShell equivalent

```powershell
$env:CYPRESS_BASE_URL="https://localhost:7228"
$env:CYPRESS_API_HOST="https://localhost:7117"
npx cypress run --browser electron
```

---

### Open Cypress with env set (PowerShell)

```powershell
$env:CYPRESS_BASE_URL="https://localhost:7228"
$env:CYPRESS_API_HOST="https://localhost:7117"
npx cypress open
```

---

### Clear cached session (force fresh login)

Delete cookie files manually:

```text
cypress/fixtures/SchoolUserCookies.json
cypress/fixtures/SchoolUserFlagOffCookies.json
cypress/fixtures/MatSchoolFlagOffCookies.json
cypress/fixtures/MATUserCookies.json
cypress/fixtures/LAUserCookies.json
```

Or just delete all:

```cmd
del cypress\fixtures\*Cookies.json
```

---

### Run ALL tests (CI-like)

```cmd
set CYPRESS_BASE_URL=https://localhost:7228&& set CYPRESS_API_HOST=https://localhost:7117&& npx cypress run
```

---

### Debug a failing spec quickly

```cmd
set CYPRESS_BASE_URL=https://localhost:7228&& set CYPRESS_API_HOST=https://localhost:7117&& npx cypress open
```

Then run the spec interactively.

---

### (Optional) Add to package.json for convenience

```json
"scripts": {
  "cy:open": "set CYPRESS_BASE_URL=https://localhost:7228&& set CYPRESS_API_HOST=https://localhost:7117&& cypress open",
  "cy:run": "set CYPRESS_BASE_URL=https://localhost:7228&& set CYPRESS_API_HOST=https://localhost:7117&& cypress run",
  "cy:admin": "set CYPRESS_BASE_URL=https://localhost:7228&& set CYPRESS_API_HOST=https://localhost:7117&& cypress run --spec \"cypress/e2e/Admin/**/*.ts\""
}
```

Then we can just run:

```bash
npm run cy:open
npm run cy:run
npm run cy:admin
```

---

### Quick sanity check before running tests

Before blaming Cypress, I check:

```text
✔ Frontend running on https://localhost:7228
✔ API running on https://localhost:7117
✔ Logged in manually at least once (if cookies needed)
✔ Correct user type selected
```
Yiannos Georgantas 30/04/2026
