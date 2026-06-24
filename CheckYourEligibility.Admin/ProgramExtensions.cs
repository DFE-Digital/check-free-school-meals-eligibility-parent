using CheckYourEligibility.Admin.Gateways;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Usecases;
using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Domain.Validation;
using FluentValidation;

namespace CheckYourEligibility.Admin;

public static class ProgramExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllersWithViews();
        services.AddHttpContextAccessor();

        services.AddHttpClient<IParentGateway, ParentGateway>(client =>
        {
            client.BaseAddress = new Uri(configuration["Api:Host"]);
        });

        services.AddHttpClient<IAdminGateway, AdminGateway>(client =>
        {
            client.BaseAddress = new Uri(configuration["Api:Host"]);
        });

        services.AddHttpClient<ICheckGateway, CheckGateway>(client =>
        {
            client.BaseAddress = new Uri(configuration["Api:Host"]);
        });

        services.AddHttpClient<INotificationGateway, NotificationGateway>(client =>
        {
            client.BaseAddress = new Uri(configuration["Api:Host"]);
        });

        // ELIG-2661B: used to read LA settings that control school dashboard tiles
        services.AddHttpClient<ILocalAuthoritySettingsGateway, LocalAuthoritySettingsGateway>(client =>
        {
            client.BaseAddress = new Uri(configuration["Api:Host"]);
        });

        services.AddHttpClient<IEligibilityCheckReportingGateway, EligibilityCheckReportingGateway>(client =>
        {
            client.BaseAddress = new Uri(configuration["Api:Host"]);
        });

        services.AddScoped<ISchoolMenuContextResolver, SchoolMenuContextResolver>();

        services.AddScoped<IBlobStorageGateway, BlobStorageGateway>();

    
        services.AddScoped<IParseBulkCheckFileUseCase, ParseBulkCheckFileUseCase>();
        services.AddScoped<IGetBulkChecks, GetBulkChecks>();
        services.AddScoped<IDeleteBulkCheckFileUseCase, DeleteBulkCheckFileUseCase>();

        services.AddScoped<IValidator<CheckEligibilityRequestDataBase>, CheckEligibilityRequestDataValidator>();
        services.AddScoped<IValidator<CheckEligibilityRequestData_Enhanced>, CheckEligibilityRequestDataValidator>();

        return services;
    }
}