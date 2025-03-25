Cypress Setup User Guide
Introduction
This guide will walk you through the steps required to install and run Cypress with TypeScript locally.
Prerequisites
Before you begin, ensure that you have the following installed:

Git Bash :
Node.js (version 12 or higher) : https://nodejs.org/en/download/prebuilt-installer
• npm (version 10 or higher ) - This is downloaded as part of Node.js

Check the Node.js is correctly installed by running the following commands

node -v
npm -v

Setup Instructions

1. Open Project in Visual Studio Code
   Open your project repository in Visual Studio Code and navigate to the test folder.

2. Install Cypress
   Inside a Bash Terminal

CD into the tests folder

Install Cypress as a development dependency by running the following command in your terminal:
npm install cypress@13.8.1 --save-dev

Certain machines may struggle to install Cypress and receive a certificate error. This can be overcome by getting the
portal URL for your VPN and adding it to this command:

- HTTP_PROXY=https{url} npm i

3. Configuring Environment envariables

Inside the tests directory create a file named "Cypress.env.json" and input the following, Populating the relevant
information. The OneGov email and password will need to be yours that you can create.

```
{
    "AUTHORIZATION_HEADER": "",
    "AUTH_USERNAME": "",
    "AUTH_PASSWORD": "",
    "ONEGOV_EMAIL": "",
    "ONEGOV_PASSWORD": ""
}
```

4. Open Cypress
   Launch Cypress using the following command:

- CYPRESS_BASE_URL={Test environment url} npx cypress open
- Cypress UI will be launched. Run the tests via electron.
- Click on "E2E Testing" to see all the tests available.

5. Running Tests
   You can run your Cypress tests in two ways:
   Interactive Mode
   To run tests in interactive mode, use:
   CYPRESS_BASE_URL="https://localhost:7228/" npx cypress open
   Headless Mode
   To run tests in headless mode, use:
   CYPRESS_BASE_URL={Test environment url} npx cypress run

Cypress testing standards:

Our goal is to make sure that our tests are easy to read and sturdy, to help keep to these standards please:

1. Make sure that under no circumstances are the environmental variables committed to the repository.
2. Make sure that test file names clearly express which areas are being tested.
3. Name each test clearly so that anyone viewing/running them may understand what is currently being tested.
4. When possible use built in Cypress features instead of creating unnecessary helper functions which will only abstract
   from the
   purpose of the test, when this is unavoidable please again make sure that the helper function is clearly named and
   will allow a user to understand its purpose.





