import { defineConfig } from "cypress";
import { faker } from "@faker-js/faker";
import fs from 'fs';

export default defineConfig({
  e2e: {
    setupNodeEvents(on, config) {
      const generatedLastName = faker.person.lastName().toUpperCase();
      config.env.lastName = 'Tester';

      on('task', {
        readFileMaybe(filePath) {
          return new Promise((resolve, reject) => {
            fs.readFile(filePath, 'utf8', (err, data) => {
              if (err) {
                if (err.code === 'ENOENT') {
                  resolve(null); // File not found, resolve with null
                } else {
                  reject(err); // Other errors, reject the promise
                }
              } else {
                try {
                  const parsedData = JSON.parse(data);
                  resolve(parsedData); // File found, resolve with parsed data
                } catch (parseError) {
                  resolve(null); // If parsing fails, resolve with null
                }
              }
            });
          });
        }
      });

      return config;
    },
    baseUrl: process.env.CYPRESS_BASE_URL,
    chromeWebSecurity: false,
    viewportWidth: 1600,
    viewportHeight: 1800,
    specPattern: 'cypress/e2e/**/*.spec.{js,jsx,ts,tsx}',
    experimentalOriginDependencies: true,
    projectId: 'cv64me',
    reporter: "junit",
    reporterOptions: {
      mochaFile: "results/my-test-output-[hash].xml",
    },
    retries: {
      "runMode": 2,
      "openMode": 2
    },
  }
});