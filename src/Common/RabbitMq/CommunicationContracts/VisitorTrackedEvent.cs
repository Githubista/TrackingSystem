﻿namespace Configuration.RabbitMq.CommunicationContracts
{
    public class VisitorTrackedEvent
    {
        public string? Referer { get; init; }
        public string? UserAgent { get; init; }
        public string IpAddress { get; init; }
    }
}
