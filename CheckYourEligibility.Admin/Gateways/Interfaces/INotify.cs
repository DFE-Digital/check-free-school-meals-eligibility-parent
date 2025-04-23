using CheckYourEligibility.Admin.Boundary.Requests;

namespace CheckYourEligibility.Admin.Gateways.Interfaces;

public interface INotify
{
    void SendNotification(NotificationRequest data);
}
