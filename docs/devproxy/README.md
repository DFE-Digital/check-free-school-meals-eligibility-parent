# FSM Basic Bulk Check - Dev Proxy Mocks

## Setup

1. Copy both files to your Dev Proxy folder (e.g., `C:\Test Projects\DevProxy\ece\`):
   - `fsm-basic-bulk-check-config.json`
   - `fsm-basic-bulk-check-mocks.json`

2. Run Dev Proxy with this config:
   ```powershell
   devproxy --config-file fsm-basic-bulk-check-config.json
   ```

## Mocked Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/bulk-check/free-school-meals` | Submit a bulk check (returns GUID) |
| GET | `/bulk-check/{guid}/progress` | Get processing progress |
| GET | `/bulk-check/{guid}/` | Get results |
| GET | `/bulk-check/search?organisationId=*` | Get bulk check history |
| DELETE | `/bulk-check/{guid}` | Delete a bulk check |

## Test Data

### Bulk Check History (4 records)
The `/bulk-check/search` endpoint returns 4 bulk checks:

| ID | Filename | Status | Type | Submitted By |
|----|----------|--------|------|--------------|
| `a1b2c3d4-e5f6-7890-abcd-ef1234567890` | january_batch_check.csv | Completed | FreeSchoolMeals | john.smith@school.gov.uk |
| `b2c3d4e5-f6a7-8901-bcde-f23456789012` | weekly_check_27jan.csv | Completed | FreeSchoolMeals | jane.doe@school.gov.uk |
| `c3d4e5f6-a7b8-9012-cdef-345678901234` | new_pupils_jan.csv | InProgress | FreeSchoolMeals | admin@school.gov.uk |
| `d4e5f6a7-b8c9-0123-defa-456789012345` | 2yo_batch.csv | Completed | TwoYearOffer | john.smith@school.gov.uk |

> **Note:** The `TwoYearOffer` record is included to verify that filtering by `EligibilityType == "FreeSchoolMeals"` works correctly.

### Bulk Check Results

**Batch 1** (`a1b2c3d4...`) - 5 records:
| Last Name | DOB | NI | Status |
|-----------|-----|-----|--------|
| Smith | 1985-03-15 | AB123456C | eligible |
| Jones | 1990-07-22 | CD789012D | notEligible |
| Williams | 1988-11-08 | EF345678E | eligible |
| Brown | 1992-01-30 | GH901234F | parentNotFound |
| Taylor | 1987-09-12 | IJ567890G | eligible |

**Batch 2** (`b2c3d4e5...`) - 3 records:
| Last Name | DOB | NI | Status |
|-----------|-----|-----|--------|
| Johnson | 1982-05-20 | KL123456H | eligible |
| Davis | 1995-12-03 | MN789012I | eligible |
| Wilson | 1979-08-17 | OP345678J | notEligible |

### Progress Responses

| Batch | Total | Complete | Status |
|-------|-------|----------|--------|
| `a1b2c3d4...` | 5 | 5 | Done |
| `b2c3d4e5...` | 3 | 3 | Done |
| `c3d4e5f6...` | 10 | 4 | In Progress |

## Combining with Existing Mocks

To use alongside your existing application mocks, you can either:

1. **Merge the mocks** - Add the mocks from `fsm-basic-bulk-check-mocks.json` into your `applicationresponse.json`

2. **Update urlsToWatch** - Add bulk-check URLs to your existing config:
   ```json
   "urlsToWatch": [
       "https://dev.eligibility-checking-engine.education.gov.uk/*",
       "https://localhost:7117/application*",
       "https://localhost:7117/check*",
       "https://localhost:7117/bulk-check*"
   ]
   ```

## Status Values

The API returns these status values which the admin app maps to user-friendly text:

| API Status | Display Text |
|------------|--------------|
| `eligible` | Entitled |
| `notEligible` | Not Entitled |
| `parentNotFound` | Information does not match records |
| `error` | Try again |
