using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MicroRabbit.Infra.Bus
{
    // sealed prevents other clases from inheriting from this class.
    // This is useful when you want to create a class that cannot be extended or modified by other classes.
    // In this case, it ensures that the RabbitMQBus class cannot be subclassed, which can help to maintain the integrity and security of the messaging system.
    public sealed class RabbitMQBus : IEventBus
    {
        private readonly IMediator _mediator;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;
        private readonly ILogger<RabbitMQBus> logger;

        public RabbitMQBus(IMediator mediator)
        {
            _mediator = mediator;
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();

        }

        public Task SendCommand<T>(T command) where T : Command
        {
            return _mediator.Send(command);
        }
        public async Task Publish<T>(T @event) where T : Event
        {
            var eventName = @event.GetType().Name;
            var factory = new ConnectionFactory() { HostName = "localhost" };

            try
            {
                await using var connection = await factory.CreateConnectionAsync();
                await using var channel = await connection.CreateChannelAsync();
                // declare a queue
                await channel.QueueDeclareAsync(
                    queue: eventName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                    );
                // serialize the event to JSON and convert it to a byte array
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));

                // publish the event to the queue
                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: eventName,
                    body: body
                    );

                logger.LogInformation("Published event {EventType} to queue {Queue}", typeof(T).Name, eventName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish event {EventType}", typeof(T).Name);
                throw;
            }
        }
        public void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            // check if the event type is already registered
            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }
            // check if the event name is already registered in the handlers dictionary
            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }
            // check if the handler type is already registered for the event name
            if (_handlers[eventName].Any(s => s.GetType() == handlerType))
            {
                throw new ArgumentException($"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }
            _handlers[eventName].Add(handlerType);

            // start consuming messages from the queue
            StartBasicConsume<T>();
        }

        private void StartBasicConsume<T>() where T : Event
        {
            // create a connection to the RabbitMQ server
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };
            var connection = factory.CreateConnectionAsync();
            var channel = connection.Result.CreateChannelAsync();

            var eventName = typeof(T).Name;

            channel.Result.QueueDeclareAsync(
                queue: eventName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
                );

            var consumer = new AsyncEventingBasicConsumer(channel.Result);

            //place holder to events (delegate)
            consumer.ReceivedAsync += Consumer_Received;

            channel.Result.BasicConsumeAsync(
                queue: eventName,
                autoAck: true,
                consumer: consumer
                );

        }
        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var eventName = e.RoutingKey;
            var message = Encoding.UTF8.GetString(e.Body.ToArray());
            try
            {
               await ProcessEvent(eventName, message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message {Message} for event {EventName}", message, eventName);
                // Optionally, you can implement retry logic or move the message to a dead-letter queue here.
            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (_handlers.ContainsKey(eventName))
            {
                var subscriptions = _handlers[eventName];
                foreach (var subscription in subscriptions)
                {
                    var handler = Activator.CreateInstance(subscription);
                    if (handler == null) continue;
                    var eventType = _eventTypes.SingleOrDefault(t => t.Name == eventName);
                    var @event = JsonSerializer.Deserialize(message, eventType);
                    var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                    await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });
                }

            }

        }
    }
}
