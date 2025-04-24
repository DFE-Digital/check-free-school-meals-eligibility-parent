using System;
using CheckYourEligibility.FrontEnd.Domain.Enums;

namespace CheckYourEligibility.FrontEnd.Boundary.Requests;

public class NotificationRequest
    {
        public NotificationRequestData Data { get; set; }
    }

    public class NotificationRequestData
    {
        public string Email { get; set; }
        public NotificationType Type { get; set; }
        public Dictionary<string, object>? Personalisation { get; set; }
    }
