using System.ComponentModel.DataAnnotations;
using System.Text;
using Configuration.RabbitMq.CommunicationContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StorageService.Settings;

namespace StorageService.Consumers
{
    public class VisitorTrackedEventConsumer : IConsumer<VisitorTrackedEvent>
    {
        private readonly ILogger<VisitorTrackedEventConsumer> _logger;
        private readonly VisitorsFileSettings _fileSettings;

        public VisitorTrackedEventConsumer(IOptionsMonitor<VisitorsFileSettings> fileSettings,
            ILogger<VisitorTrackedEventConsumer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileSettings = fileSettings.CurrentValue ?? throw new ArgumentNullException(nameof(fileSettings));
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
                var loggedDateTime = DateTime.UtcNow.ToString("O");

                var visitorLog = $"{loggedDateTime}|{referer}|{userAgent}|{message.IpAddress}{Environment.NewLine}";
                
                
                byte[] jsonBytes = Encoding.UTF8.GetBytes(visitorLog);

                var directoryPath = Path.GetDirectoryName(_fileSettings.FilePath);

                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var fileStreamOptions = new FileStreamOptions
                {
                    Access = FileAccess.Write,
                    Mode = FileMode.Append,
                    Share = FileShare.Write,
                    Options = FileOptions.Asynchronous
                };

                await using var sourceStream = File.Open(_fileSettings.FilePath, fileStreamOptions);
                await sourceStream.WriteAsync(jsonBytes);
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
