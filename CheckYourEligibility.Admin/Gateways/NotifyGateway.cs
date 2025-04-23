using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using System.Net.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CheckYourEligibility.Admin.Gateways
{
    public class NotifyGateway : BaseGateway, INotify
    {
        private readonly string _NotificationSendUrl;
        private readonly ILogger _logger;


        public NotifyGateway(string serviceName, ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration) : base(serviceName, logger, httpClient, configuration)
        {
            _NotificationSendUrl = "Notification";
            _logger = logger.CreateLogger("EcsService");

        }

        public async Task<NotificationItemResponse> SendNotification(NotificationRequest notificationRequest)
        {

            string templateId = _configuration.GetValue<string>($"Notify:Templates:{notificationRequest.Data.Type.ToString()}");
            try
            {
                var response = await ApiDataPostAsynch(_NotificationSendUrl, notificationRequest, new NotificationItemResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Send Notification failed. uri-{_NotificationSendUrl}");
                throw;
            }
        }

        void INotify.SendNotification(NotificationRequest data)

        {
            throw new NotImplementedException();
        }
    }
}
