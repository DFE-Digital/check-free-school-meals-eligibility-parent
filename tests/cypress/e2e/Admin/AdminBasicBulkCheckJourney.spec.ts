import { waitForStatusCompleted } from "../../support/BulkCheckHelper";

const bulkBasicUploadAttemptLimit = Number(
  Cypress.env("BULK_UPLOAD_ATTEMPT_LIMIT") ?? 10,
);
const bulkBasicOverLimitRowCount = Number(
  Cypress.env("BULK_OVER_LIMIT_ROW_COUNT") ?? 6001,
);

const createBasicBulkCsv = (rowCount: number): string => {
  const header =
    "Parent Last Name,Parent Date of Birth,Parent National Insurance number";
  const rows = Array.from({ length: rowCount }, (_, index) => {
    const day = ((index % 28) + 1).toString().padStart(2, "0");
    return `Tester,${day}/01/2000,AB123456C`;
  });
  return [header, ...rows].join("\n");
};

describe("BasicLAHappyPath", () => {
  let skipSetupBasic = false;
  beforeEach(() => {
    if (!skipSetupBasic) {
      cy.checkSession("basic");
      cy.visit((Cypress.config().baseUrl ?? "") + "/home");
      cy.wait(1);
      cy.get(".govuk-caption-l").should(
        "include.text",
        "Manchester City Council",
      );
      cy.contains("Run a batch check").click();
      cy.url().should("include", "/BulkCheck/Bulk_Check");
    }
  });

  it("will return an error message if the bulk file contains header content that doesn't match the template", () => {
    cy.fixture(
      "BulkCheckFileValidation/BASIC-bulkchecktemplate_invalid_HeadersContent.csv",
    ).then((fileContent1) => {
      cy.get('input[type="file"]').attachFile([
        {
          fileContent: fileContent1,
          fileName: "BASIC-bulkchecktemplate_invalid_HeadersContent.csv",
          mimeType: "text/csv",
        },
      ]);
    });
    cy.contains("button", "Run a batch check").click();
    cy.get("#file-upload-1-error").as("errorMessage");
    cy.get("@errorMessage").should(($p) => {
      expect($p.first()).to.contain(
        "Invalid CSV format. Missing required header: 'Parent Date of Birth'",
      );
    });
  });

  it("will return an error message if the bulk file contains wrong number of headers or out of sequence headers", () => {
    cy.fixture(
      "BulkCheckFileValidation/BASIC-bulkchecktemplate_invalid_HeadersSequenceOrCount.csv",
    ).then((fileContent1) => {
      cy.get('input[type="file"]').attachFile([
        {
          fileContent: fileContent1,
          fileName:
            "BASIC-bulkchecktemplate_invalid_HeadersSequenceOrCount.csv",
          mimeType: "text/csv",
        },
      ]);
    });
    cy.contains("button", "Run a batch check").click();
    cy.get("#file-upload-1-error").as("errorMessage");
    cy.get("@errorMessage").should(($p) => {
      expect($p.first()).to.contain(
        "The column headers in the selected file must exactly match the template",
      );
    });
  });

  it("will return an error message if the bulk file contains more than the configured row limit", () => {
    const overLimitCsv = createBasicBulkCsv(bulkBasicOverLimitRowCount + 1);
    cy.get('input[type="file"]').attachFile([
      {
        fileContent: overLimitCsv,
        fileName: "bulkcheck_over_limit.csv",
        mimeType: "text/csv",
      },
    ]);
    cy.contains("button", "Run a batch check").click();
    cy.get("#file-upload-1-error").as("errorMessage");
    cy.get("@errorMessage").should(($p) => {
      expect($p.first()).to.contain(
        "CSV file cannot contain more than 250 records",
      );
    });
  });

it("will run a successful batch check", () => {
  cy.fixture("BulkCheckFileValidation/BASIC-bulkchecktemplate_complete.csv")
    .then((fileContent1) => {
      cy.get('input[type="file"]').attachFile([
        {
          fileContent: fileContent1,
          fileName: "BASIC-bulkchecktemplate_complete.csv",
          mimeType: "text/csv",
        },
      ]);
    });

  cy.contains("button", "Run a batch check").click();

  cy.get("h1").should("include.text", "Batch checks history");

  const today = new Date()
    .toLocaleDateString("en-GB", {
      day: "2-digit",
      month: "short",
      year: "numeric",
    })
    .replace(",", "");

  cy.contains("table tbody tr", "BASIC-bulkchecktemplate_complete.csv")
    .should("exist")
    .then(($row) => {
      cy.wrap($row).find("td").eq(0).invoke("text").should("match", /\.csv$/i);
      cy.wrap($row).find("td").eq(1).should("have.text", "15");
      cy.wrap($row).find("td").eq(2).should("have.text", "Tester");
      cy.wrap($row).find("td").eq(3).should("contain.text", today);
      cy.wrap($row).find("td").eq(4).invoke("text").should("not.be.empty");
      cy.wrap($row).find("td").eq(5).find("strong").should("have.class", "govuk-tag");
    });

  waitForStatusCompleted("BASIC-bulkchecktemplate_complete.csv");

  cy.contains("table tbody tr", "BASIC-bulkchecktemplate_complete.csv")
    .should("exist")
    .then(($row) => {
      cy.wrap($row).find("td").eq(5).should("contain.text", "Completed");

      cy.wrap($row)
        .find("td")
        .eq(6)
        .should("contain.text", "Download results")
        .and("contain.text", "Delete");
    });
});

  it("will run a successful batch check when last name contains a curly apostrophe", () => {
    cy.fixture(
      "BulkCheckFileValidation/BASIC-bulkchecktemplate_curly_apostrophe.csv",
    ).then((fileContent1) => {
      cy.get('input[type="file"]').attachFile([
        {
          fileContent: fileContent1,
          fileName: "BASIC-bulkchecktemplate_curly_apostrophe.csv",
          mimeType: "text/csv",
        },
      ]);
    });
    cy.get('input[type="file"]').attachFile(
      "BulkCheckFileValidation/BASIC-bulkchecktemplate_curly_apostrophe.csv",
    );

    cy.get('input[type="file"]').should(($input) => {
      expect(($input[0] as HTMLInputElement).files?.length).to.eq(1);
    });

    cy.contains("button", "Run a batch check").click();

    cy.get("h1", { timeout: 80000 }).should(
      "include.text",
      "Batch checks history",
    );

    cy.contains(
      "table tbody tr",
      "BASIC-bulkchecktemplate_curly_apostrophe.csv",
      { timeout: 80000 },
    ).should("exist");
  });

  it("Navigate to Batch checks history and delete a batch check if one exists", () => {
    cy.contains("a", "Batch checks history").click();
    cy.get("h1", { timeout: 80000 }).should(
      "include.text",
      "Batch checks history",
    );

    cy.get("table tbody tr td:nth-child(7) a") // Get all delete links
      .filter((_, el) => /delete/i.test(el.innerText))
      .then(($links) => {
        if ($links.length === 0) {
          cy.log(
            "No delete links found - probably In Progress or no completed checks.",
          );
          return;
        }

        cy.wrap($links[0]).click();

        cy.get("h3.govuk-notification-banner__heading").should(
          "contain.text",
          "Batch check deleted successfully.",
        );
      });
  });

  it("will return an error message if more than 10 batches are attempted within an hour", () => {
    for (let i = 0; i < 11; i++) {
      cy.fixture(
        "BulkCheckFileValidation/BASIC-bulkchecktemplate_invalid_HeadersContent.csv",
      ).then((fileContent1) => {
        cy.get('input[type="file"]').attachFile([
          {
            fileContent: fileContent1,
            fileName: "BASIC-bulkchecktemplate_invalid_HeadersContent.csv",
            mimeType: "text/csv",
          },
        ]);
      });
      cy.contains("button", "Run a batch check").click();
    }
    cy.get("#file-upload-1-error").as("errorMessage");
    cy.get("@errorMessage").should(($p) => {
      expect($p.first()).to.contain(
        "You have exceeded the maximum number of bulk upload attempts. Please try again later.",
      );
    });
  });
});
