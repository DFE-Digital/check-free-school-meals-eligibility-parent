using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;

namespace CheckYourEligibility.Admin.Gateways.Interfaces;

public interface INotify
{
    Task<NotificationItemResponse> SendNotification(NotificationRequest data);
}
