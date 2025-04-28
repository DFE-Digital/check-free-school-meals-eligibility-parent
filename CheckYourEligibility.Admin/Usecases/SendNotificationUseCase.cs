using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;

namespace CheckYourEligibility.Admin.UseCases;

public interface ISendNotificationUseCase
{
    Task<NotificationItemResponse> Execute(NotificationRequest notificationRequest);
}

public class SendNotificationUseCase : ISendNotificationUseCase
{
    private readonly ILogger<SendNotificationUseCase> _logger;
    private readonly INotificationGateway _notificationGateway;

    public SendNotificationUseCase(
        ILogger<SendNotificationUseCase> logger,
        INotificationGateway notificationGateway)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationGateway = notificationGateway ?? throw new ArgumentNullException(nameof(notificationGateway));
    }

    public async Task<NotificationItemResponse> Execute(NotificationRequest notificationRequest)
    {
        try
        {
            _logger.LogInformation("Sending notification request");
            
            var response = await _notificationGateway.SendNotification(notificationRequest);
            
            _logger.LogInformation("Notification sent successfully");
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification");
            throw;
        }
    }
}