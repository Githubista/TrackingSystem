namespace Communication.Contracts
{
    public class VisitorTrackedEvent
    {
        public string? Referer { get; init; }
        public string? UserAgent { get; init; }
        public string IpAddress { get; init; }

        public VisitorTrackedEvent(string? referer, 
            string? userAgent,
            string ipAddress)
        {
            Referer = referer;
            UserAgent = userAgent;
            IpAddress = ipAddress;
        }
    }
}
