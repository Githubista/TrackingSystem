using Configuration.RabbitMq.CommunicationContracts;
using MassTransit;
using PixelService.Resources;

namespace PixelService.Tracking
{
    public class TrackingService : IService<TrackingRequestModel, TrackingResponseModel>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<TrackingService> _logger;

        public TrackingService(IPublishEndpoint publishEndpoint,
            ILogger<TrackingService> logger)
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ValueTask<TrackingResponseModel> GetAsync(TrackingRequestModel request, HttpContext context, CancellationToken cancellationToken)
        {
            //this is a fire and forget method as my assumption is that we need to return very fast a response to this method
            // so we can save time by not awaiting this publish
            _ = PublishTrackingInformation(request, cancellationToken);
            return ValueTask.FromResult(new TrackingResponseModel(ImageLoader.TrackingImage));
        }

        public async Task PublishTrackingInformation(TrackingRequestModel request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(request.IpAddress))
                {
                    return;
                }

                var visitorTrackedEvent = new VisitorTrackedEvent
                {
                    Referer = request.Referer, 
                    UserAgent = request.UserAgent, 
                    IpAddress = request.IpAddress
                };

                await _publishEndpoint.Publish(visitorTrackedEvent, cancellationToken);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Failed to publish {event} event", typeof(VisitorTrackedEvent));
            }
        }
    }
}
