namespace PixelService.Tracking
{
    public class TrackingRequestModel
    {
        public string? Referrer { get; init; }
        public string? UserAgent { get; init; }
        public string? IpAddress { get; init; }

        public TrackingRequestModel(string? referrer,
            string? userAgent,
            string? ipAddress)
        {
            Referrer = referrer;
            UserAgent = userAgent;
            IpAddress = ipAddress;
        }
    }
}
