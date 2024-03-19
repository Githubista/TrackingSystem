using Communication.Contracts;
using MassTransit;

namespace StorageService.Consumers
{
    public class VisitorTrackedEventConsumer : IConsumer<VisitorTrackedEvent>
    {
        public async Task Consume(ConsumeContext<VisitorTrackedEvent> context)
        {
            throw new NotImplementedException();
        }
    }
}
