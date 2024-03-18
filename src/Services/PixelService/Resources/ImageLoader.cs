namespace PixelService.Resources
{
    public static class ImageLoader
    {
        public static byte[] TrackingImage { get; private set; } = Array.Empty<byte>();

        /// <summary>
        /// This is a very simple method of loading some information necessary during the service run.
        /// In production, depending on the requirements, the resources can be loaded/read from different sources
        /// like an external storage.
        /// </summary>
        public static async Task LoadTrackingImage()
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), nameof(Resources), "1-pixel.gif");
            var imageBytes = await File.ReadAllBytesAsync(imagePath);
            TrackingImage = imageBytes;
        }
    }
}
