using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace urban_dukan_checkout_service.Services
{

    public class ServiceBusPublisher : IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;

        public ServiceBusPublisher(IConfiguration config)
        {
            var connectionString = config["ServiceBus:ConnectionString"];
            var queueName = config["ServiceBus:QueueName"];

            _client = new ServiceBusClient(connectionString);
            _sender = _client.CreateSender(queueName);
        }

        public async Task SendOrderPlacedEventAsync(object orderEvent)
        {
            string messageBody = JsonSerializer.Serialize(orderEvent);

            var message = new ServiceBusMessage(messageBody);

            await _sender.SendMessageAsync(message);
        }

        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}
