# Check Your Eligibility Frontend

This repo contains the front-end for the Free School Meals (FSM) Parent Portal service.

## Pre-requisites

The projects target .NET 10. Install the .NET 10 SDK to build and run them locally.

### Local development setup guide

See the [FSM Parent local setup guide](https://dfedigital.atlassian.net/wiki/spaces/ECE/pages/6144294917) for prerequisites, local configuration, Azure Key Vault secret mapping and troubleshooting common setup issues.

## How to run tests

We have two test-suites - one .NET NUnit for unit tests and one Cypress for integration and e2e tests. Cypress needs a running application responding to http calls.

### Unit tests

`
cd CheckYourEligibility.FrontEnd.Tests
dotnet test
`

### Cypress Testing

There is a full breakdown of how to set up and run the Cypress tests along with our current standards for Cypress testing within the tests folder.

## Ways of working

### Releasing code

Submit pull requests to `main` with functioning code. Reviewers check that the automated tests pass and review the changes. Once approved, the author merges the pull request and deletes the branch.

### Secrets

Do not commit active secrets to this repository. If a secret is committed, notify the appropriate DfE contacts, rewritethe Git history, and follow DfE processes.
