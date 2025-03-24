using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CheckYourEligibility.FrontEnd.Gateways;
using CheckYourEligibility.FrontEnd.Gateways.Interfaces;

namespace CheckYourEligibility.FrontEnd
{
    public static class ProgramExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllersWithViews();

            services.AddHttpClient<IParentGateway, ParentGateway>(client =>
            {
                client.BaseAddress = new Uri(configuration["Api:Host"]);
            });

            services.AddHttpClient<ICheckGateway, CheckGateway>(client =>
            {
                client.BaseAddress = new Uri(configuration["Api:Host"]);
            });
            
            return services;
        }
    }
}
