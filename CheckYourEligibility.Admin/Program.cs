using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using CheckYourEligibility.Admin;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Mappings;
using CheckYourEligibility.Admin.Usecases;
using CheckYourEligibility.Admin.UseCases;
using Microsoft.FeatureManagement;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-GB");

builder.Services.AddApplicationInsightsTelemetry();
if (Environment.GetEnvironmentVariable("FSM_ADMIN_KEY_VAULT_NAME") != null)
{
    var keyVaultName = Environment.GetEnvironmentVariable("FSM_ADMIN_KEY_VAULT_NAME");
    var kvUri = $"https://{keyVaultName}.vault.azure.net";

    builder.Configuration.AddAzureKeyVault(
        new Uri(kvUri),
        new DefaultAzureCredential(),
        new AzureKeyVaultConfigurationOptions
        {
            ReloadInterval = TimeSpan.FromSeconds(60 * 10)
        }
    );
}

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<BulkExportProfile>();
});

// Add services to the container.
builder.Services.AddServices(builder.Configuration);
builder.Services.AddSession();
builder.Services.AddMemoryCache(); // ELIG-2661B: cache school menu decisions per LA

builder.Services.AddScoped<IAddChildUseCase, AddChildUseCase>();
builder.Services.AddScoped<IChangeChildDetailsUseCase, ChangeChildDetailsUseCase>();
builder.Services.AddScoped<IEnterChildDetailsUseCase, EnterChildDetailsUseCase>();
builder.Services.AddScoped<ILoadParentDetailsUseCase, LoadParentDetailsUseCase>();
builder.Services.AddScoped<IProcessChildDetailsUseCase, ProcessChildDetailsUseCase>();
builder.Services.AddScoped<IPerformEligibilityCheckUseCase, PerformEligibilityCheckUseCase>();
builder.Services.AddScoped<IGetCheckStatusUseCase, GetCheckStatusUseCase>();
builder.Services.AddScoped<IGetCheckUseCase, GetCheckUseCase>();
builder.Services.AddScoped<IRegistrationResponseUseCase, RegistrationResponseUseCase>();
builder.Services.AddScoped<IRegistrationUseCase, RegistrationUseCase>();
builder.Services.AddScoped<IRemoveChildUseCase, RemoveChildUseCase>();
builder.Services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();
builder.Services.AddScoped<ISendNotificationUseCase, SendNotificationUseCase>();
builder.Services.AddScoped<ISubmitApplicationUseCase, SubmitApplicationUseCase>();
builder.Services.AddScoped<ISearchSchoolsUseCase, SearchSchoolsUseCase>();
builder.Services.AddScoped<IValidateParentDetailsUseCase, ValidateParentDetailsUseCase>();
builder.Services.AddScoped<IValidateEvidenceFileUseCase, ValidateEvidenceFileUseCase>();
builder.Services.AddScoped<IInitializeCheckAnswersUseCase, InitializeCheckAnswersUseCase>();
builder.Services.AddScoped<IUploadEvidenceFileUseCase, UploadEvidenceFileUseCase>();
builder.Services.AddScoped<IDownloadEvidenceFileUseCase, DownloadEvidenceFileUseCase>();
builder.Services.AddScoped<IDeleteEvidenceFileUseCase, DeleteEvidenceFileUseCase>();
builder.Services.AddScoped<IGenerateEligibilityCheckReportUseCase, GenerateEligibilityCheckReportUseCase>();
builder.Services.AddScoped<IDeleteEligibilityCheckReportUseCase, DeleteEligibilityCheckReportUseCase>();
builder.Services.AddSession();

var dfeSignInConfiguration = new DfeSignInConfiguration();
builder.Configuration.GetSection("DfeSignIn").Bind(dfeSignInConfiguration);
builder.Services.AddDfeSignInAuthentication(dfeSignInConfiguration);

//builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
//builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();

builder.Services.AddScoped<IMenuProvider, MenuProvider>();

builder.Services.AddFeatureManagement();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");


app.MapHealthChecks("/healthcheck");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseStatusCodePagesWithReExecute("/Error/NotFound");
app.UseAuthorization();
app.Use((context, next) =>
{
    context.Response.Headers["strict-transport-security"] = "max-age=31536000; includeSubDomains";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self' https://*.clarity.ms https://c.bing.com";
    context.Response.Headers["X-Frame-Options"] = "sameorigin";
    context.Response.Headers["Cache-Control"] = "Private";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    if (!builder.Configuration.GetValue<bool>("AllowSearchIndexing"))
    {
        context.Response.Headers["X-Robots-Tag"] = "none";
    }
    return next.Invoke();
});
app.UseSession();
app.MapControllerRoute(
    "default",
    "{controller=Start}/{action=Index}/{id?}");


app.Run();