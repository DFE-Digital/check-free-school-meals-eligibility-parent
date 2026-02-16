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

        services.AddScoped<IBlobStorageGateway, BlobStorageGateway>();

        // FSM Basic Bulk Check Services
        services.AddScoped<IParseBulkCheckFileUseCase_FsmBasic, ParseBulkCheckFileUseCase_FsmBasic>();
        services.AddScoped<IGetBulkCheckStatusesUseCase_FsmBasic, GetBulkCheckStatusesUseCase_FsmBasic>();
        services.AddScoped<IDeleteBulkCheckFileUseCase_FsmBasic, DeleteBulkCheckFileUseCase_FsmBasic>();

        // FSM Basic Validator
        services.AddScoped<IValidator<CheckEligibilityRequestData_FsmBasic>, CheckEligibilityRequestDataValidator_FsmBasic>();

        return services;
    }
}