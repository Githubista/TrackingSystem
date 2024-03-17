namespace PixelService.Resources
{
    public static class ImageLoader
    {
        public static byte[]? TrackingImage { get; private set; }

        public static async Task LoadTrackingImage()
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "1-pixel.gif");
            byte[] gifBytes = await File.ReadAllBytesAsync(imagePath);
            TrackingImage = gifBytes;
        }
    }
}
