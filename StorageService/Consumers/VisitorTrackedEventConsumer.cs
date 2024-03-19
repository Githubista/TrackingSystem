using Communication.Contracts;
using MassTransit;

namespace StorageService.Consumers
{
    public class VisitorTrackedEventConsumer : IConsumer<VisitorTrackedEvent>
    {
        public Task Consume(ConsumeContext<VisitorTrackedEvent> context)
        {
            throw new NotImplementedException();
        }
    }
}
