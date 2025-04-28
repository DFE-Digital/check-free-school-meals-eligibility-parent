using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;

namespace CheckYourEligibility.Admin.Gateways.Interfaces;

public interface INotificationGateway
{
    Task<NotificationItemResponse> SendNotification(NotificationRequest data);
}
