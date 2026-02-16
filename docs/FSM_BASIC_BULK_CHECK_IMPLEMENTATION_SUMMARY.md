# FSM Basic Bulk Check - Implementation Summary

## Overview
This document summarizes all the backend components created for the FSM Basic Bulk Check functionality.

## Files Created

### 1. Request Objects
**File**: `CheckYourEligibility.Admin/Boundary/Requests/CheckEligibilityRequest.cs` (Updated)
- Added `CheckEligibilityRequestData_FsmBasic` - Data model for individual FSM Basic check
- Added `CheckEligibilityRequest_FsmBasic` - Single check request wrapper
- Added `CheckEligibilityRequestBulk_FsmBasic` - Bulk check request with metadata
- Added `CheckEligibilityRequestBulkMeta` - Metadata (filename, submitted by)

### 2. Response Objects
**File**: `CheckYourEligibility.Admin/Boundary/Responses/CheckEligibilityBulkProgressByLAResponse.cs` (New)
- `CheckEligibilityBulkProgressByLAResponse` - List of all bulk checks for an organisation
- `CheckEligibilityBulkProgressResponse` - Individual bulk check progress/status

**File**: `CheckYourEligibility.Admin/Boundary/Responses/CheckEligiblityBulkDeleteResponse.cs` (New)
- `CheckEligiblityBulkDeleteResponse` - Delete operation response

### 3. Models
**File**: `CheckYourEligibility.Admin/Models/BulkCheck.cs` (New)
- `BulkCheck` - Domain model for bulk check status
- `IBulkExport` - Interface for bulk export results
- `BulkExport` - FSM export result model
- `BulkExportWorkingFamilies` - Working families export result model

### 4. View Models
**File**: `CheckYourEligibility.Admin/ViewModels/BulkCheckFsmBasicViewModel.cs` (New)
- `BulkCheckFsmBasicViewModel` - Upload page view model
- `CheckRowErrorFsmBasic` - CSV row error model
- `BulkCheckFsmBasicErrorsViewModel` - Errors page view model
- `BulkCheckFsmBasicStatusViewModel` - Single check status for table
- `BulkCheckFsmBasicStatusesViewModel` - History table with pagination
- `BulkCheckFsmBasicFileSubmittedViewModel` - File submitted confirmation

### 5. Gateway Updates
**File**: `CheckYourEligibility.Admin/Gateways/CheckGateway.cs` (Updated)
- Added `_FsmBasicCheckBulkUploadUrl` field
- Added `PostBulkCheck_FsmBasic()` - Submit bulk check
- Added `GetBulkCheckProgress_FsmBasic()` - Check progress
- Added `GetBulkCheckResults_FsmBasic()` - Get results
- Added `GetBulkCheckStatuses_FsmBasic()` - Get all checks for organisation
- Added `DeleteBulkChecksFor_FsmBasic()` - Delete a check
- Added `LoadBulkCheckResults_FsmBasic()` - Load and map results for download
- Added `GetFsmBasicStatusDescription()` - Map status codes to descriptions

**File**: `CheckYourEligibility.Admin/Gateways/Interfaces/ICheckGateway.cs` (Updated)
- Added interface methods for all FSM Basic bulk operations

### 6. Use Cases
**File**: `CheckYourEligibility.Admin/Usecases/ParseBulkCheckFileUseCase_FsmBasic.cs` (New)
- `IParseBulkCheckFileUseCase_FsmBasic` interface
- `ParseBulkCheckFileUseCase_FsmBasic` - Parses CSV file, validates rows
- `BulkCheckCsvResultFsmBasic` - Parse result model
- `CsvRowErrorFsmBasic` - CSV error model

**File**: `CheckYourEligibility.Admin/Usecases/GetBulkCheckStatusesUseCase_FsmBasic.cs` (New)
- `IGetBulkCheckStatusesUseCase_FsmBasic` interface
- `GetBulkCheckStatusesUseCase_FsmBasic` - Retrieves all bulk checks for an organisation
- Filters to FSM Basic checks only
- Maps statuses to display-friendly text

**File**: `CheckYourEligibility.Admin/Usecases/DeleteBulkCheckFileUseCase_FsmBasic.cs` (New)
- `IDeleteBulkCheckFileUseCase_FsmBasic` interface
- `DeleteBulkCheckFileUseCase_FsmBasic` - Deletes a bulk check

### 7. Validation
**File**: `CheckYourEligibility.Admin/Domain/Validation/CheckEligibilityRequestDataValidator_FsmBasic.cs` (New)
- `CheckEligibilityRequestDataValidator_FsmBasic` - FluentValidation validator
- Validates: LastName, DateOfBirth, NationalInsuranceNumber
- Does NOT support NASS (unlike full FSM check)

### 8. Controller
**File**: `CheckYourEligibility.Admin/Controllers/BulkCheckFsmBasicController.cs` (New)
- `Bulk_Check()` GET - Upload page
- `Bulk_Check(IFormFile)` POST - Handle upload and submission
- `Bulk_Check_Status(string)` GET - Check progress
- `Bulk_Check_Complete(string)` GET - Show completion
- `Bulk_Check_History(int, int)` GET - Show history table with pagination
- `Bulk_Check_View_Results(string)` GET - View results (optional)
- `Bulk_Check_Download(string)` GET - Download results as CSV
- `Bulk_Check_Delete(string)` POST - Delete a check
- Includes rate limiting (5 attempts per hour)
- File validation (CSV only, max 10MB)
- Error handling and logging

### 9. Documentation
**File**: `CheckYourEligibility.Admin/FSM_BASIC_BULK_CHECK_FRONTEND_GUIDE.md` (New)
- Complete frontend implementation guide
- Lists all views to create
- Documents all view models and their properties
- Provides controller action reference
- Includes styling notes and testing checklist

## Key Differences from LA Bulk Check

1. **Status Page Flow**: Instead of directly showing results, shows a status page with table of all checks
2. **History Table**: Prominent batch check history with pagination
3. **Simpler Data Model**: Only requires Last Name, DOB, and NI (no NASS support)
4. **Separate API Endpoints**: Uses `bulk-check/free-school-meals-basic` endpoint
5. **Independent Functionality**: Completely separate from LA bulk check, no cross-contamination

## API Endpoints Used

Based on the attached files, these endpoints are expected:
- `POST bulk-check/free-school-meals-basic` - Submit bulk check
- `GET bulk-check/{id}/status` - Check progress
- `GET bulk-check/{id}/results` - Get results
- `GET bulk-checks/statuses?organisationId={id}` - Get all checks
- `DELETE bulk-check/{id}` - Delete check

## Configuration Required

Add these to `appsettings.json`:
```json
{
  "BulkEligibilityCheckLimit": "1000",
  "BulkUploadAttemptLimit": "5"
}
```

## Dependency Injection Required

Add these to `Program.cs` or `ProgramExtensions.cs`:
```csharp
// Use Cases
services.AddScoped<IParseBulkCheckFileUseCase_FsmBasic, ParseBulkCheckFileUseCase_FsmBasic>();
services.AddScoped<IGetBulkCheckStatusesUseCase_FsmBasic, GetBulkCheckStatusesUseCase_FsmBasic>();
services.AddScoped<IDeleteBulkCheckFileUseCase_FsmBasic, DeleteBulkCheckFileUseCase_FsmBasic>();

// Validators
services.AddScoped<IValidator<CheckEligibilityRequestData_FsmBasic>, CheckEligibilityRequestDataValidator_FsmBasic>();
```

## Views Required (Frontend Team)

See `FSM_BASIC_BULK_CHECK_FRONTEND_GUIDE.md` for complete details. Views needed:
1. `Bulk_Check.cshtml` - Upload page
2. `Bulk_Check_Errors.cshtml` - Show CSV validation errors
3. `Bulk_Check_Submitted.cshtml` - File submitted confirmation
4. `Bulk_Check_Processing.cshtml` - Processing status with progress bar
5. `Bulk_Check_Complete.cshtml` - Completion message
6. `Bulk_Check_History.cshtml` - History table with pagination
7. `Bulk_Check_View_Results.cshtml` - (Optional) View results in browser

## Testing Points

1. **Upload Flow**:
   - Valid CSV uploads successfully
   - Invalid CSV shows errors
   - Non-CSV files rejected
   - Files > 10MB rejected

2. **Rate Limiting**:
   - Can upload 5 times in an hour
   - 6th attempt blocked

3. **Status Flow**:
   - Processing page shows progress
   - Redirects to complete when done
   - Can view in history

4. **History Table**:
   - Shows all FSM Basic checks
   - Pagination works
   - Can download completed checks
   - Can delete checks
   - Statuses display correctly

5. **Error Handling**:
   - API errors handled gracefully
   - Missing data handled
   - Invalid IDs handled

## Notes for Frontend Developer

1. The controller is fully implemented and ready for views
2. All view models are created with appropriate properties
3. Route convention: `/BulkCheckFsmBasic/{action}`
4. Follow existing admin panel styling patterns
5. Use GOV.UK Design System components if applicable
6. See the frontend guide for detailed specifications

## Next Steps

1. **Backend**: Register dependencies in DI container
2. **Frontend**: Create all required views
3. **Testing**: Test complete flow end-to-end
4. **API**: Ensure API endpoints match expected contracts
5. **Documentation**: Update main README if needed
