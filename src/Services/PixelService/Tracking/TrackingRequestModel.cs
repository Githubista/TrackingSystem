namespace PixelService.Tracking
{
    public class TrackingRequestModel
    {
        public string? Referer { get; init; }
        public string? UserAgent { get; init; }
        public string? IpAddress { get; init; }

        public TrackingRequestModel(string? referer,
            string? userAgent,
            string? ipAddress)
        {
            Referer = referer;
            UserAgent = userAgent;
            IpAddress = ipAddress;
        }
    }
}
