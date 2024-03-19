using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StorageService.Consumers;
using StorageService.Settings;

namespace StorageService
{
    internal class ReceiveEndpointsService : BackgroundService
    {
        private readonly IReceiveEndpointConnector _connector;
        private readonly QueueSettings _queueSettings;
        private readonly ILogger<ReceiveEndpointsService> _logger;
        private readonly List<HostReceiveEndpointHandle> _endpointHandlers = new();

        public ReceiveEndpointsService(
            IReceiveEndpointConnector connector,
            IOptions<QueueSettings> queueSettings,
            ILogger<ReceiveEndpointsService> logger)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _queueSettings = queueSettings?.Value ?? throw new ArgumentNullException(nameof(queueSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            await ConnectReceiveEndpoints();
            _logger.LogInformation("Bus endpoints connected at: {time}", DateTimeOffset.UtcNow);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await DisconnectReceiveEndpoints();

            _logger.LogInformation("Bus endpoints disconnected at: {time}", DateTimeOffset.UtcNow);
            await base.StopAsync(cancellationToken);
        }

        private async Task ConnectReceiveEndpoints()
        {
            _endpointHandlers.Add(_connector.ConnectReceiveEndpoint(_queueSettings.VisitorTrackedQueueName, (context, cfg) =>
            {
                cfg.ConfigureConsumer<VisitorTrackedEventConsumer>(context);
            }));

            await Task.WhenAll(_endpointHandlers.Select(x => x.Ready));
        }

        private async Task DisconnectReceiveEndpoints()
        {
            if (_endpointHandlers.Any())
            {
                await Task.WhenAll(_endpointHandlers.Select(x => x.StopAsync()));
                _logger.LogInformation("Bus endpoints disconnected at: {time}", DateTimeOffset.UtcNow);
            }

            _endpointHandlers.Clear();
        }
    }
}
