using Communication.Contracts;
using MassTransit;
using PixelService.Resources;

namespace PixelService.Tracking
{
    public class TrackingService : IService<TrackingRequestModel, TrackingResponseModel>
    {
        private readonly IBusControl _busControl;
        private readonly ILogger<TrackingService> _logger;

        public TrackingService(IBusControl busControl,
            ILogger<TrackingService> logger)
        {
            _busControl = busControl ?? throw new ArgumentNullException(nameof(busControl));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TrackingResponseModel> GetAsync(TrackingRequestModel request, HttpContext context, CancellationToken cancellationToken)
        {
            _ = PublishTrackingInformation(request, cancellationToken);
            return new TrackingResponseModel(ImageLoader.TrackingImage);
        }

        public async Task PublishTrackingInformation(TrackingRequestModel request, CancellationToken cancellationToken)
        {
            try
            {
                var visitorTrackedEvent = new VisitorTrackedEvent(request.Referer, request.UserAgent, request.IpAddress);

                await _busControl.Publish(visitorTrackedEvent, cancellationToken);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Failed to publish {event} event", typeof(VisitorTrackedEvent));
            }
        }
    }
}
