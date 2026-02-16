# FSM Basic Bulk Check - Frontend Implementation Guide

This document provides instructions for the frontend developer on implementing the views for the FSM Basic Bulk Check functionality.

## Overview

The FSM Basic Bulk Check feature allows users to upload CSV files containing parent/guardian information and check their Free School Meals eligibility. Unlike the current bulk check for LA, this version shows a status page with a table of checks instead of directly showing results.

## Controller: BulkCheckFsmBasicController

Location: `CheckYourEligibility.Admin/Controllers/BulkCheckFsmBasicController.cs`

## Views to Create

All views should be created in: `CheckYourEligibility.Admin/Views/BulkCheckFsmBasic/`

### 1. **Bulk_Check.cshtml** (Upload Page)
- **Route**: `/BulkCheckFsmBasic/Bulk_Check`
- **HTTP Method**: GET and POST
- **View Model**: `BulkCheckFsmBasicViewModel`
- **Purpose**: Initial page where users upload their CSV file

#### View Model Properties:
```csharp
public class BulkCheckFsmBasicViewModel
{
    public string DocumentTemplatePath { get; set; }
    public List<string> FieldDescriptions { get; set; }
}
```

#### Required Elements:
- Link to download CSV template (`Model.DocumentTemplatePath`)
- Instructions displaying field descriptions (`Model.FieldDescriptions`)
- File upload form (accepts CSV only, max 10MB)
- "Run check" submit button
- Display error messages from `TempData["ErrorMessage"]`

#### Form Requirements:
```html
<form method="post" enctype="multipart/form-data">
    <input type="file" name="fileUpload" accept=".csv" />
    <button type="submit">Run a batch check</button>
</form>
```

---

### 2. **Bulk_Check_Errors.cshtml** (Validation Errors Page)
- **Route**: Redirected from POST when CSV has errors
- **View Model**: `BulkCheckFsmBasicErrorsViewModel`
- **Purpose**: Display validation errors found in the uploaded CSV

#### View Model Properties:
```csharp
public class BulkCheckFsmBasicErrorsViewModel
{
    public string Response { get; set; }
    public string ErrorMessage { get; set; }
    public IEnumerable<CheckRowErrorFsmBasic> Errors { get; set; }
    public int TotalErrorCount { get; set; }
}

public class CheckRowErrorFsmBasic
{
    public int LineNumber { get; set; }
    public string Message { get; set; }
}
```

#### Required Elements:
- Display `Model.ErrorMessage` as main error message
- Table showing errors with columns:
  - Line Number (`Error.LineNumber`)
  - Error Message (`Error.Message`)
- Show first 20 errors only
- Display total error count (`Model.TotalErrorCount`)
- Link back to upload page to fix and resubmit

---

### 3. **Bulk_Check_Submitted.cshtml** (File Submitted Confirmation)
- **Route**: Redirected after successful upload
- **View Model**: `BulkCheckFsmBasicFileSubmittedViewModel`
- **Purpose**: Confirm file was submitted and is being processed

#### View Model Properties:
```csharp
public class BulkCheckFsmBasicFileSubmittedViewModel
{
    public string Filename { get; set; }
    public int NumberOfRecords { get; set; }
}
```

#### Required Elements:
- Success message banner (blue/info style)
- Display submitted filename (`Model.Filename`)
- Display number of records (`Model.NumberOfRecords`)
- Message: "Your file {filename} was submitted and is being checked."
- Note about processing time depending on file size
- Link to "View the latest status in the batch check history"
- Auto-redirect to Bulk_Check_Status after 3 seconds (optional)

---

### 4. **Bulk_Check_Processing.cshtml** (Processing Status)
- **Route**: `/BulkCheckFsmBasic/Bulk_Check_Status`
- **HTTP Method**: GET
- **View Data**: Uses ViewBag
- **Purpose**: Show progress while checks are being processed

#### ViewBag Properties:
```csharp
ViewBag.Total // int - total records
ViewBag.Complete // int - completed records
ViewBag.BulkCheckUrl // string - status check URL
```

#### Required Elements:
- Progress indicator showing `ViewBag.Complete` of `ViewBag.Total`
- Progress bar (percentage = Complete/Total * 100)
- Message: "Processing time depends on file size and how many checks are currently running."
- Auto-refresh page every 5 seconds using JavaScript or meta refresh
- Link to batch check history

#### Auto-refresh Example:
```html
<meta http-equiv="refresh" content="5">
```

---

### 5. **Bulk_Check_Complete.cshtml** (Processing Complete)
- **Route**: `/BulkCheckFsmBasic/Bulk_Check_Complete?bulkCheckId={id}`
- **HTTP Method**: GET
- **View Data**: `ViewBag.BulkCheckId`
- **Purpose**: Notify user that processing is complete

#### Required Elements:
- Success message (green banner): "Checks completed"
- Message: "Now download the checked file to see which parents can claim free school meals for their children."
- Download button linking to: `/BulkCheckFsmBasic/Bulk_Check_Download?bulkCheckId={ViewBag.BulkCheckId}`
- Link to "Return to dashboard" or batch history

---

### 6. **Bulk_Check_History.cshtml** (Batch Checks History Table)
- **Route**: `/BulkCheckFsmBasic/Bulk_Check_History?page={page}&pageSize={pageSize}`
- **HTTP Method**: GET
- **View Model**: `BulkCheckFsmBasicStatusesViewModel`
- **Purpose**: Display table of all batch checks with pagination

#### View Model Properties:
```csharp
public class BulkCheckFsmBasicStatusesViewModel
{
    public List<BulkCheckFsmBasicStatusViewModel> Checks { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
}

public class BulkCheckFsmBasicStatusViewModel
{
    public string BulkCheckId { get; set; }
    public string Filename { get; set; }
    public int NumberOfRecords { get; set; }
    public string FinalNameInCheck { get; set; }
    public DateTime DateSubmitted { get; set; }
    public string SubmittedBy { get; set; }
    public string Status { get; set; }
}
```

#### Required Elements:
- Page title: "Batch checks history"
- Tab/button to navigate to "Run a manual batch check" (Bulk_Check page)
- Message: "Showing {count} batch files uploaded in the last 7 days"
- Table with columns:
  - **Filename** - `Check.Filename`
  - **Number of records** - `Check.NumberOfRecords`
  - **Final name in check** - `Check.FinalNameInCheck`
  - **Date submitted** - `Check.DateSubmitted` (format: "dd MMMM yyyy h:mmtt")
  - **Submitted by** - `Check.SubmittedBy`
  - **Status** - `Check.Status` with appropriate badge styling:
    - "Not started" - grey
    - "In progress" - blue
    - "Completed" - green
    - "Failed" - red
  - **Actions**:
    - "View results" link (if Status = "Completed"): `/BulkCheckFsmBasic/Bulk_Check_View_Results?bulkCheckId={Check.BulkCheckId}`
    - "Delete" button (POST form): `/BulkCheckFsmBasic/Bulk_Check_Delete` with `bulkCheckId={Check.BulkCheckId}`

- **Pagination controls**:
  - Previous/Next buttons
  - Page numbers (1, 2, 3...)
  - Highlight current page (`Model.CurrentPage`)
  - Calculate using `Model.TotalPages`

- Display success/error messages from TempData:
  - `TempData["SuccessMessage"]` - green banner
  - `TempData["ErrorMessage"]` - red banner

---

### 7. **Bulk_Check_View_Results.cshtml** (Optional - View Results in Browser)
- **Route**: `/BulkCheckFsmBasic/Bulk_Check_View_Results?bulkCheckId={id}`
- **HTTP Method**: GET
- **View Model**: `CheckEligibilityBulkResponse`
- **Purpose**: Display results in browser before downloading (optional feature)

#### View Model Properties:
```csharp
public class CheckEligibilityBulkResponse
{
    public IEnumerable<CheckEligibilityItem> Data { get; set; }
}

public class CheckEligibilityItem
{
    public string LastName { get; set; }
    public string DateOfBirth { get; set; }
    public string NationalInsuranceNumber { get; set; }
    public string Status { get; set; }
}
```

#### Required Elements:
- Results table with columns:
  - Last Name
  - Date of Birth
  - National Insurance Number
  - Outcome (mapped status with descriptions)
- Download button to get CSV version
- Status descriptions:
  - "parentNotFound" → "Information does not match records"
  - "eligible" → "Entitled"
  - "notEligible" → "Not Entitled"
  - "error" → "Try again"

---

## Controller Actions Summary

| Action | HTTP Method | Purpose | View/Redirect |
|--------|-------------|---------|---------------|
| `Bulk_Check()` | GET | Show upload page | View: Bulk_Check.cshtml |
| `Bulk_Check(IFormFile)` | POST | Handle file upload | View: Bulk_Check_Errors.cshtml OR View: Bulk_Check_Submitted.cshtml |
| `Bulk_Check_Status(string)` | GET | Check progress | View: Bulk_Check_Processing.cshtml OR Redirect: Bulk_Check_Complete |
| `Bulk_Check_Complete(string)` | GET | Show completion | View: Bulk_Check_Complete.cshtml |
| `Bulk_Check_History(int, int)` | GET | Show history table | View: Bulk_Check_History.cshtml |
| `Bulk_Check_View_Results(string)` | GET | View results | View: Bulk_Check_View_Results.cshtml |
| `Bulk_Check_Download(string)` | GET | Download CSV | File download |
| `Bulk_Check_Delete(string)` | POST | Delete bulk check | Redirect: Bulk_Check_History |

---

## Styling Notes

1. **Status Badges**: Use appropriate colors based on status:
   - Not started: Grey/Neutral
   - In progress: Blue/Info
   - Completed: Green/Success
   - Failed: Red/Danger

2. **Message Banners**: 
   - Success messages: Green background
   - Error messages: Red background
   - Info messages: Blue background

3. **Tables**: Should be responsive and match existing admin panel styling

4. **Forms**: Use standard GOV.UK Design System components if applicable

5. **Progress Bar**: Visual indicator showing completion percentage

---

## Navigation

Add a link in the main navigation menu:
- Text: "FSM Basic Bulk Check" or "Batch Check (Basic)"
- Route: `/BulkCheckFsmBasic/Bulk_Check_History`

This should be separate from the existing LA bulk check functionality.

---

## API Endpoints Called (for reference)

The controller calls these API endpoints (configured in CheckGateway):
- `POST bulk-check/free-school-meals-basic` - Submit bulk check
- `GET bulk-check/{id}/status` - Check progress
- `GET bulk-check/{id}/results` - Get results
- `GET bulk-checks/statuses?organisationId={id}` - Get all checks for organisation
- `DELETE bulk-check/{id}` - Delete a bulk check

---

## Testing Checklist

- [ ] Upload valid CSV file
- [ ] Upload CSV with errors (shows error page)
- [ ] Upload non-CSV file (shows error)
- [ ] Upload file > 10MB (shows error)
- [ ] Watch processing status update
- [ ] View completed check in history table
- [ ] Download results as CSV
- [ ] Delete a bulk check
- [ ] Pagination works correctly
- [ ] All links and buttons work
- [ ] Error messages display correctly
- [ ] Success messages display correctly

---

## Additional Notes

- The FSM Basic check only requires: Last Name, Date of Birth, and National Insurance Number
- Unlike the full FSM check, it does NOT support NASS (National Asylum Seekers Service Number)
- Results show simpler outcomes: Entitled, Not Entitled, Information does not match records, Try again
- Session management handles rate limiting (max 5 attempts per hour)
- Use existing styling from the LA bulk check where appropriate but keep functionality separate

---

## Questions or Issues

Contact the backend developer if you need:
- Clarification on any view model properties
- Additional controller actions
- Changes to the API responses
- Help with routing or configuration
