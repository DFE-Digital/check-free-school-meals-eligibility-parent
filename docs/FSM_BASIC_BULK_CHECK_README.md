# FSM Basic Bulk Check - Complete Implementation Guide

## Project Overview

This implementation adds **FSM Basic Bulk Check** functionality to the Check Your Eligibility Admin portal. It allows Local Authorities and schools to upload CSV files with parent/guardian information to batch-check Free School Meals eligibility.

### Key Features
- CSV file upload with validation
- Batch processing with progress tracking
- Status page showing table of all checks
- Batch check history with pagination
- Download results as CSV
- Delete completed checks
- Rate limiting (5 uploads per hour)

### Differences from LA Bulk Check
- Shows status/history table instead of direct results
- Simpler data model (only Last Name, DOB, NI - no NASS)
- Dedicated FSM Basic API endpoints
- Independent functionality, completely separate codebase

---

## Files Created/Modified

### Request Objects
- ✅ **CheckEligibilityRequest.cs** - Added FSM Basic request models
  - `CheckEligibilityRequestData_FsmBasic`
  - `CheckEligibilityRequest_FsmBasic`
  - `CheckEligibilityRequestBulk_FsmBasic`
  - `CheckEligibilityRequestBulkMeta`

### Response Objects
- ✅ **CheckEligibilityBulkProgressByLAResponse.cs** - New
  - `CheckEligibilityBulkProgressByLAResponse`
  - `CheckEligibilityBulkProgressResponse`
- ✅ **CheckEligiblityBulkDeleteResponse.cs** - New
  - `CheckEligiblityBulkDeleteResponse`
- ✅ **CheckEligibilityResponse.cs** - Modified
  - Added `Get_BulkCheck_Status` to `CheckEligibilityResponseBulkLinks`

### Models
- ✅ **BulkCheck.cs** - New
  - `BulkCheck` - Domain model
  - `IBulkExport`, `BulkExport`, `BulkExportWorkingFamilies` - Export models

### View Models
- ✅ **BulkCheckFsmBasicViewModel.cs** - New
  - `BulkCheckFsmBasicViewModel`
  - `CheckRowErrorFsmBasic`
  - `BulkCheckFsmBasicErrorsViewModel`
  - `BulkCheckFsmBasicStatusViewModel`
  - `BulkCheckFsmBasicStatusesViewModel`
  - `BulkCheckFsmBasicFileSubmittedViewModel`

### Gateways
- ✅ **CheckGateway.cs** - Modified
  - Added 6 new FSM Basic methods
- ✅ **ICheckGateway.cs** - Modified
  - Added FSM Basic interface methods

### Use Cases
- ✅ **ParseBulkCheckFileUseCase_FsmBasic.cs** - New
- ✅ **GetBulkCheckStatusesUseCase_FsmBasic.cs** - New
- ✅ **DeleteBulkCheckFileUseCase_FsmBasic.cs** - New

### Validation
- ✅ **CheckEligibilityRequestDataValidator_FsmBasic.cs** - New

### Controllers
- ✅ **BulkCheckFsmBasicController.cs** - New (complete implementation)

### Configuration
- ✅ **ProgramExtensions.cs** - Modified (added DI registrations)

### Documentation
- ✅ **FSM_BASIC_BULK_CHECK_FRONTEND_GUIDE.md** - Complete frontend guide
- ✅ **FSM_BASIC_BULK_CHECK_IMPLEMENTATION_SUMMARY.md** - Implementation summary
- ✅ **DEPENDENCY_INJECTION_SETUP.md** - DI setup guide
- ✅ **README.md** (this file) - Complete guide

---

## Configuration Required

### 1. App Settings
Add to `appsettings.json`:
```json
{
  "BulkEligibilityCheckLimit": "1000",
  "BulkUploadAttemptLimit": "5"
}
```

### 2. Dependency Injection
**Already completed** in `ProgramExtensions.cs`:
```csharp
// FSM Basic Bulk Check Services
services.AddScoped<IParseBulkCheckFileUseCase_FsmBasic, ParseBulkCheckFileUseCase_FsmBasic>();
services.AddScoped<IGetBulkCheckStatusesUseCase_FsmBasic, GetBulkCheckStatusesUseCase_FsmBasic>();
services.AddScoped<IDeleteBulkCheckFileUseCase_FsmBasic, DeleteBulkCheckFileUseCase_FsmBasic>();

// FSM Basic Validator
services.AddScoped<IValidator<CheckEligibilityRequestData_FsmBasic>, CheckEligibilityRequestDataValidator_FsmBasic>();
```

---

## Controller Routes

| URL | Method | Action | Purpose |
|-----|--------|--------|---------|
| `/BulkCheckFsmBasic/Bulk_Check` | GET | Show upload page | Initial upload form |
| `/BulkCheckFsmBasic/Bulk_Check` | POST | Handle upload | Process CSV file |
| `/BulkCheckFsmBasic/Bulk_Check_Status` | GET | Check progress | Show processing status |
| `/BulkCheckFsmBasic/Bulk_Check_Complete` | GET | Show completion | Completion message |
| `/BulkCheckFsmBasic/Bulk_Check_History` | GET | Show history | Table of all checks |
| `/BulkCheckFsmBasic/Bulk_Check_View_Results` | GET | View results | Display results |
| `/BulkCheckFsmBasic/Bulk_Check_Download` | GET | Download CSV | Download results |
| `/BulkCheckFsmBasic/Bulk_Check_Delete` | POST | Delete check | Remove a check |

---

## API Endpoints Called

The backend expects these API endpoints (configured in CheckGateway):
- `POST bulk-check/free-school-meals-basic` - Submit bulk check
- `GET bulk-check/{id}/status` - Check progress
- `GET bulk-check/{id}/results` - Get results
- `GET bulk-checks/statuses?organisationId={id}` - Get all checks
- `DELETE bulk-check/{id}` - Delete check

---

## Views to Create (Frontend)

See **FSM_BASIC_BULK_CHECK_FRONTEND_GUIDE.md** for complete specifications.

**Location**: `CheckYourEligibility.Admin/Views/BulkCheckFsmBasic/`

### Required Views:
1. **Bulk_Check.cshtml** - Upload page
2. **Bulk_Check_Errors.cshtml** - Validation errors
3. **Bulk_Check_Submitted.cshtml** - File submitted confirmation
4. **Bulk_Check_Processing.cshtml** - Processing status with progress bar
5. **Bulk_Check_Complete.cshtml** - Completion message
6. **Bulk_Check_History.cshtml** - History table with pagination
7. **Bulk_Check_View_Results.cshtml** - View results (optional)

---

## CSV File Format

### Required Headers:
- `last name`
- `date of birth` (DD/MM/YYYY or YYYY-MM-DD)
- `national insurance number`

### Example:
```csv
last name,date of birth,national insurance number
Smith,01/01/1980,AB123456C
Jones,15/05/1975,CD789012D
```

### Validation Rules:
- Last name: Required, non-empty
- Date of birth: Required, valid date format
- National Insurance Number: Required, valid UK NI format
- Maximum 1000 records per file
- File size limit: 10MB
- File type: CSV only

---

## Status Descriptions

The system uses these statuses:
- **Not started** - Check queued but not yet processing
- **In progress** - Currently processing
- **Completed** - Processing finished, results available
- **Failed** - Processing encountered an error

### Outcome Mappings:
- `parentNotFound` → "Information does not match records"
- `eligible` → "Entitled"
- `notEligible` → "Not Entitled"
- `error` → "Try again"

---

## Rate Limiting

- Maximum 5 upload attempts per hour per session
- Resets after 1 hour from first submission
- Exceeded attempts show error message

---

## Testing Checklist

### Upload Flow
- [ ] Valid CSV uploads successfully
- [ ] Invalid CSV shows error page with line numbers
- [ ] Non-CSV file rejected
- [ ] File > 10MB rejected
- [ ] Empty file rejected
- [ ] Missing required headers rejected

### Processing Flow
- [ ] Processing status page shows progress bar
- [ ] Auto-refresh works (every 5 seconds)
- [ ] Redirects to complete when done
- [ ] Session URL properly stored and retrieved

### History Table
- [ ] Shows all FSM Basic checks for organisation
- [ ] Pagination works correctly
- [ ] Sorting by date descending
- [ ] Status badges display with correct colors
- [ ] View Results link appears only for completed checks
- [ ] Delete button works

### Download
- [ ] CSV download works for completed checks
- [ ] Downloaded file has correct format
- [ ] Status descriptions properly mapped
- [ ] Filename includes timestamp

### Error Handling
- [ ] API errors handled gracefully
- [ ] Missing organisation ID handled
- [ ] Invalid bulk check ID handled
- [ ] Session expiry handled

### Rate Limiting
- [ ] Can upload 5 times
- [ ] 6th attempt blocked with message
- [ ] Counter resets after 1 hour

---

## Navigation Integration

Add link to main navigation:
```
Text: "FSM Basic Bulk Check" or "Batch Check (Basic)"
Route: /BulkCheckFsmBasic/Bulk_Check_History
```

Keep separate from LA bulk check functionality.

---

## Build and Run

### 1. Verify Dependencies
```bash
# Ensure these packages are installed:
dotnet list package | grep -E "CsvHelper|FluentValidation|Newtonsoft.Json"
```

### 2. Build Project
```bash
dotnet build CheckYourEligibility.Admin.sln
```

### 3. Check for Errors
```bash
# Should show no errors
dotnet build --no-incremental
```

### 4. Run Application
```bash
dotnet run --project CheckYourEligibility.Admin
```

### 5. Verify Routes
Navigate to:
- `https://localhost:{port}/BulkCheckFsmBasic/Bulk_Check`
- Should show 404 if views not created yet
- Should not show 500 error (indicates backend working)

---

## Troubleshooting

### "Unable to resolve service IParseBulkCheckFileUseCase_FsmBasic"
**Solution**: Verify DI registrations in ProgramExtensions.cs

### "Configuration value 'BulkEligibilityCheckLimit' not found"
**Solution**: Add configuration to appsettings.json

### "Cannot find Get_BulkCheck_Status"
**Solution**: Verify CheckEligibilityResponse.cs was updated with the new property

### "Views not found"
**Solution**: Views must be created by frontend developer (see frontend guide)

### Rate limiting not working
**Solution**: Verify session is configured in Program.cs with `app.UseSession()`

---

## Next Steps

1. **Backend Complete** ✅ - All code implemented and compiling
2. **Frontend Required** ⏳ - Create 7 views (see frontend guide)
3. **API Integration** ⏳ - Ensure API endpoints match expected contracts
4. **Testing** ⏳ - End-to-end testing after views created
5. **Deployment** ⏳ - Deploy to test environment

---

## Support

For questions or issues:
1. Review **FSM_BASIC_BULK_CHECK_FRONTEND_GUIDE.md** for view specifications
2. Review **DEPENDENCY_INJECTION_SETUP.md** for configuration help
3. Check **FSM_BASIC_BULK_CHECK_IMPLEMENTATION_SUMMARY.md** for file reference
4. Contact backend developer for API/controller questions

---

## Summary

✅ **Complete backend implementation ready**
- 8 new/modified files in Boundary layer
- 3 new use cases
- 1 new validator
- 1 complete controller with 8 actions
- Updated gateway with 6 new methods
- All DI registrations added
- All compilation errors resolved

⏳ **Frontend work required**
- 7 views to be created
- See FSM_BASIC_BULK_CHECK_FRONTEND_GUIDE.md for complete specifications

🔄 **API coordination needed**
- Verify API endpoints match expected contracts
- Test API responses match response models

---

## License & Attributions

This implementation follows patterns from similar bulk check functionality in the `check-childcare-eligibility-admin` repository while maintaining complete separation and independence.
