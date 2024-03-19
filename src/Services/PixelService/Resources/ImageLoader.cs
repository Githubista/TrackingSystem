namespace PixelService.Resources
{
    public static class ImageLoader
    {
        public static byte[] TrackingImage { get; private set; } = Array.Empty<byte>();

        public static async Task LoadTrackingImage()
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), nameof(Resources), "1-pixel.gif");
            var imageBytes = await File.ReadAllBytesAsync(imagePath);
            TrackingImage = imageBytes;
        }
    }
}
