# Dependency Injection Setup for FSM Basic Bulk Check

Add these registrations to your DI container (typically in `Program.cs` or `ProgramExtensions.cs`):

## Required Service Registrations

```csharp
using CheckYourEligibility.Admin.Usecases;
using CheckYourEligibility.Admin.Domain.Validation;
using CheckYourEligibility.Admin.Boundary.Requests;
using FluentValidation;

// In your ConfigureServices method or builder.Services section:

// FSM Basic Use Cases
services.AddScoped<IParseBulkCheckFileUseCase_FsmBasic, ParseBulkCheckFileUseCase_FsmBasic>();
services.AddScoped<IGetBulkCheckStatusesUseCase_FsmBasic, GetBulkCheckStatusesUseCase_FsmBasic>();
services.AddScoped<IDeleteBulkCheckFileUseCase_FsmBasic, DeleteBulkCheckFileUseCase_FsmBasic>();

// FSM Basic Validator
services.AddScoped<IValidator<CheckEligibilityRequestData_FsmBasic>, CheckEligibilityRequestDataValidator_FsmBasic>();
```

## Configuration Settings

Add to `appsettings.json`:

```json
{
  "BulkEligibilityCheckLimit": "1000",
  "BulkUploadAttemptLimit": "5"
}
```

## Existing Dependencies Required

These should already be registered (verify they exist):

```csharp
// Already registered (no action needed)
services.AddScoped<ICheckGateway, CheckGateway>();
services.AddHttpClient<ICheckGateway, CheckGateway>();
```

## Session Configuration

Ensure session is configured (should already exist):

```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// In the middleware pipeline:
app.UseSession();
```

## Complete Registration Example

If you're adding to an existing `ProgramExtensions.cs` file:

```csharp
public static class ProgramExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // ... existing registrations ...

        // FSM Basic Bulk Check Services
        services.AddScoped<IParseBulkCheckFileUseCase_FsmBasic, ParseBulkCheckFileUseCase_FsmBasic>();
        services.AddScoped<IGetBulkCheckStatusesUseCase_FsmBasic, GetBulkCheckStatusesUseCase_FsmBasic>();
        services.AddScoped<IDeleteBulkCheckFileUseCase_FsmBasic, DeleteBulkCheckFileUseCase_FsmBasic>();

        // FSM Basic Validator
        services.AddScoped<IValidator<CheckEligibilityRequestData_FsmBasic>, CheckEligibilityRequestDataValidator_FsmBasic>();
    }
}
```

## Required NuGet Packages

Ensure these packages are installed (they should already be present):

- `CsvHelper` - For CSV parsing
- `FluentValidation` - For validation
- `FluentValidation.DependencyInjectionExtensions` - For FluentValidation DI
- `Microsoft.AspNetCore.Http` - For session and file upload
- `Newtonsoft.Json` - For JSON serialization

## Verification Checklist

After adding the registrations:

1. ✅ Build the project successfully
2. ✅ No DI-related errors on startup
3. ✅ Navigate to `/BulkCheckFsmBasic/Bulk_Check` - should load (may show view error if views not created yet)
4. ✅ Check logs for any DI resolution errors
5. ✅ Verify ICheckGateway can be resolved (existing dependency)

## Common Issues

### Issue: "Unable to resolve service for type IParseBulkCheckFileUseCase_FsmBasic"
**Solution**: Ensure you've added the service registration as shown above

### Issue: "Unable to resolve service for type IValidator<CheckEligibilityRequestData_FsmBasic>"
**Solution**: Add the validator registration

### Issue: "Configuration value 'BulkEligibilityCheckLimit' not found"
**Solution**: Add the configuration values to appsettings.json

### Issue: "ICheckGateway methods not found"
**Solution**: Ensure you're using the updated CheckGateway.cs and ICheckGateway.cs files
