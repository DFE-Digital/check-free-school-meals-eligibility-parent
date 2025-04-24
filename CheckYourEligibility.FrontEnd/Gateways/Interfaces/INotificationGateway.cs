using System;
using CheckYourEligibility.FrontEnd.Boundary.Requests;
using CheckYourEligibility.FrontEnd.Boundary.Responses;

namespace CheckYourEligibility.FrontEnd.Gateways.Interfaces;

public interface INotificationGateway
{
    Task<NotificationItemResponse> SendNotification(NotificationRequest data);
}
