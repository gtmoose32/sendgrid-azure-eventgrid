using Azure.Messaging.EventGrid;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Moosesoft.SendGrid.Azure.EventGrid
{
    /// <summary>
    /// Reads SendGrid events json posted to a webhook, converts them to <see cref="EventGridEvent"/> and publishes those events to a topic.
    /// </summary>
    public class EventGridEventPublisher : IEventGridEventPublisher
    {
        private readonly EventGridEventPublisherSettings _settings;
        private readonly EventGridPublisherClient _eventGridClient;

        /// <summary>
        /// Initialize a new instance of <see cref="EventGridEventPublisher"/>.
        /// </summary>
        /// <param name="eventGridClient">The client used to publish SendGrid events to an Azure EventGrid topic.</param>
        /// <param name="settings">Settings used to help build events.</param>
        public EventGridEventPublisher(EventGridPublisherClient eventGridClient, EventGridEventPublisherSettings settings = null)
        {
            _eventGridClient = eventGridClient ?? throw new ArgumentNullException(nameof(eventGridClient));
            _settings = settings ?? EventGridEventPublisherSettings.Default;
        }

        /// <inheritdoc cref="IEventGridEventPublisher"/>
        public async Task PublishEventsAsync(string sendGridEventsJson, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(sendGridEventsJson))
                throw new ArgumentException("Cannot be null, empty or whitespace.", nameof(sendGridEventsJson));

            var events = JsonConvert.DeserializeObject<IEnumerable<JObject>>(sendGridEventsJson).ToArray();
            if (!events.Any()) return;

            var eventGridEvents = events
                .Select(ToEventGridEvent)
                .ToArray();

            await _eventGridClient.SendEventsAsync(eventGridEvents, cancellationToken: cancellationToken);
        }

        private const string EventIdKey = "sg_event_id";
        private EventGridEvent ToEventGridEvent(JObject eventJson)
        {
            return new EventGridEvent(
                _settings.BuildEventSubject(eventJson),
                _settings.BuildEventType(eventJson),
                "1", 
                new BinaryData(Encoding.UTF8.GetBytes(eventJson.ToString())))
            {
                Id = eventJson.TryGetValue(EventIdKey, StringComparison.OrdinalIgnoreCase, out var token)
                    ? token.Value<string>()
                    : Guid.NewGuid().ToString()
            };
        }
    }
}