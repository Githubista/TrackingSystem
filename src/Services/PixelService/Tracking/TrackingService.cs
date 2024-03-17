using PixelService.Services;

namespace PixelService.Tracking
{
    public class TrackingService : IService<TrackingRequestModel, TrackingResponseModel>
    {
        public async Task<TrackingResponseModel> GetAsync(TrackingRequestModel request, HttpContext context)
        {
            // publish data to other service;
            //get image bytes and return them
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "example.png");
            var imageBytes = await File.ReadAllBytesAsync(imagePath);

            return new TrackingResponseModel(imageBytes);
        }
    }
}
