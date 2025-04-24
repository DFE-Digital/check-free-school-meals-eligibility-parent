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
    private readonly INotify _notifyGateway;

    public SendNotificationUseCase(
        ILogger<SendNotificationUseCase> logger,
        INotify notifyGateway)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notifyGateway = notifyGateway ?? throw new ArgumentNullException(nameof(notifyGateway));
    }

    public async Task<NotificationItemResponse> Execute(NotificationRequest notificationRequest)
    {
        try
        {
            _logger.LogInformation("Sending notification request");
            
            var response = await _notifyGateway.SendNotification(notificationRequest);
            
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