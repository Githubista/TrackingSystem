using System.ComponentModel.DataAnnotations;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMq.CommunicationContracts;
using StorageService.Settings;

namespace StorageService.Consumers
{
    public class VisitorTrackedEventConsumer : IConsumer<VisitorTrackedEvent>
    {
        private readonly ILogger<VisitorTrackedEventConsumer> _logger;
        private readonly VisitorsFileSettings _fileSettings;
        private readonly IFileWriter _fileWriter;
        private readonly IDateTimeProvider _dateTimeProvider;

        public VisitorTrackedEventConsumer(IOptionsMonitor<VisitorsFileSettings> fileSettings,
            IFileWriter fileWriter,
            IDateTimeProvider dateTimeProvider,
            ILogger<VisitorTrackedEventConsumer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileSettings = fileSettings.CurrentValue ?? throw new ArgumentNullException(nameof(fileSettings));
            _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public async Task Consume(ConsumeContext<VisitorTrackedEvent> context)
        {
            var message = context.Message;

            if (string.IsNullOrWhiteSpace(message.IpAddress))
            {
                throw new ValidationException($"The {typeof(VisitorTrackedEvent)} can't be consumed as it has empty {nameof(VisitorTrackedEvent.IpAddress)}");
            }

            _logger.LogDebug("New {eventType} received with referer: {referer}, user agent: {userAgent} and Ip: {ipAddress}",
                typeof(VisitorTrackedEvent), message.Referer, message.UserAgent, message.IpAddress);

            try
            {
                var referer = string.IsNullOrWhiteSpace(message.Referer) ? "null" : message.Referer;
                var userAgent = string.IsNullOrWhiteSpace(message.UserAgent) ? "null" : message.UserAgent;
                var loggedDateTime = _dateTimeProvider.GetUtcNow();

                var visitorLog = $"{loggedDateTime}|{referer}|{userAgent}|{message.IpAddress}{Environment.NewLine}";
                
                await _fileWriter.AppendToFile(_fileSettings.FilePath, visitorLog);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Writing {eventType} event with referer: {referer}, user agent: {userAgent} and Ip: {ipAddress} failed!",
                    typeof(VisitorTrackedEvent), message.Referer, message.UserAgent, message.IpAddress);

                throw;
            }
        }
    }
}
