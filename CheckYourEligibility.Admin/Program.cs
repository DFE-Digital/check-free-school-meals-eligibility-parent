using System.Globalization;
using CheckYourEligibility.Admin;
using Azure.Identity;
using CheckYourEligibility.Admin.Infrastructure;
using System.Text;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using CheckYourEligibility.Admin.UseCases;

var builder = WebApplication.CreateBuilder(args);

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-GB");

builder.Services.AddApplicationInsightsTelemetry();
if (Environment.GetEnvironmentVariable("KEY_VAULT_NAME")!=null)
{
    var keyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
    var kvUri = $"https://{keyVaultName}.vault.azure.net";

    builder.Configuration.AddAzureKeyVault(
        new Uri(kvUri), 
        new DefaultAzureCredential(),
        new AzureKeyVaultConfigurationOptions()
        {
            ReloadInterval = TimeSpan.FromSeconds(60*10)
        }
    );
}

// Add services to the container.
builder.Services.AddServices(builder.Configuration);
builder.Services.AddSession();

builder.Services.AddScoped<IAddChildUseCase, AddChildUseCase>();
builder.Services.AddScoped<IChangeChildDetailsUseCase, ChangeChildDetailsUseCase>();
builder.Services.AddScoped<IEnterChildDetailsUseCase, EnterChildDetailsUseCase>();
builder.Services.AddScoped<ILoadParentDetailsUseCase, LoadParentDetailsUseCase>();
builder.Services.AddScoped<IProcessChildDetailsUseCase, ProcessChildDetailsUseCase>();
builder.Services.AddScoped<IPerformEligibilityCheckUseCase, PerformEligibilityCheckUseCase>();
builder.Services.AddScoped<IGetCheckStatusUseCase, GetCheckStatusUseCase>();
builder.Services.AddScoped<IRegistrationResponseUseCase, RegistrationResponseUseCase>();
builder.Services.AddScoped<IRegistrationUseCase, RegistrationUseCase>();
builder.Services.AddScoped<IRemoveChildUseCase, RemoveChildUseCase>();
builder.Services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();
builder.Services.AddScoped<ISubmitApplicationUseCase, SubmitApplicationUseCase>();
builder.Services.AddScoped<IValidateParentDetailsUseCase, ValidateParentDetailsUseCase>();
builder.Services.AddScoped<IInitializeCheckAnswersUseCase, InitializeCheckAnswersUseCase>();
builder.Services.AddSession();

var dfeSignInConfiguration = new DfeSignInConfiguration();
builder.Configuration.GetSection("DfeSignIn").Bind(dfeSignInConfiguration);
builder.Services.AddDfeSignInAuthentication(dfeSignInConfiguration);

//builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
//builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.Use(async (ctx, next) =>
{
    await next();
    if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
    {
        //Re-execute the request so the user gets the error page
        ctx.Request.Path = "/Error/NotFound";
        await next();
    }
});

app.MapHealthChecks("/healthcheck");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.Use((context, next) =>
{
    context.Response.Headers["strict-transport-security"] = "max-age=31536000; includeSubDomains";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self' https://*.clarity.ms https://c.bing.com";
    context.Response.Headers["X-Frame-Options"] = "sameorigin";
    context.Response.Headers["Cache-Control"] = "Private";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    return next.Invoke();
});
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
