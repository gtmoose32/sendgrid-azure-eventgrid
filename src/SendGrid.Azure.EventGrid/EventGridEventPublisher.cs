using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IEventGridClient _eventGridClient;
        private readonly Uri _topicUri;

        /// <summary>
        /// Initialize a new instance of <see cref="EventGridEventPublisher"/>.
        /// </summary>
        /// <param name="eventGridClient">The client used to publish SendGrid events to an Azure EventGrid topic.</param>
        /// <param name="topicUri">The Azure EventGrid topic endpoint where events will be published.</param>
        /// <param name="settings">Settings used to help build events.</param>
        public EventGridEventPublisher(
            IEventGridClient eventGridClient,
            Uri topicUri,
            EventGridEventPublisherSettings settings = null)
        {
            _eventGridClient = eventGridClient ?? throw new ArgumentNullException(nameof(eventGridClient));
            _topicUri = topicUri ?? throw new ArgumentNullException(nameof(topicUri));
            _settings = settings ?? EventGridEventPublisherSettings.Default;
        }

        /// <inheritdoc cref="IEventGridEventPublisher"/>
        public Task PublishEventsAsync(string sendGridEventsJson, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(sendGridEventsJson))
                throw new ArgumentException("Events json cannot be null, empty or whitespace.", nameof(sendGridEventsJson));

            var events = JsonConvert.DeserializeObject<IEnumerable<JObject>>(sendGridEventsJson).ToArray();
            if (!events.Any()) return Task.CompletedTask;

            var eventGridEvents = events.Select(@event => @event.ToEventGridEvent(_settings)).ToList();

            return _eventGridClient.PublishEventsWithHttpMessagesAsync(_topicUri.Host, eventGridEvents, cancellationToken: cancellationToken);
        }
    }
}