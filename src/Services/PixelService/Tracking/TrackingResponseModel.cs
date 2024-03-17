namespace PixelService.Tracking
{
    public class TrackingResponseModel
    {
        public byte[] ImageBytes { get; init; }

        public TrackingResponseModel(byte[] imageBytes)
        {
            ImageBytes = imageBytes;
        }
    }
}
