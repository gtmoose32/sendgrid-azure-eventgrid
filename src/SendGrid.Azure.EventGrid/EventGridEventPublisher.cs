using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Sendgrid.Webhooks.Service;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Moosesoft.SendGrid.Azure.EventGrid
{
    /// <summary>
    /// Reads SendGrid events json posted to a webhook, converts them to <see cref="EventGridEvent"/> and publishes those events to a specified topic.
    /// </summary>
    public class EventGridEventPublisher : IEventGridEventPublisher
    {
        private readonly EventGridEventPublisherSettings _settings;
        private readonly IEventGridClient _eventGridClient;
        private readonly string _topicHostName;
        private readonly WebhookParser _webhookParser;

        /// <summary>
        /// Initialize a new instance of <see cref="EventGridEventPublisher"/>.
        /// </summary>
        /// <param name="eventGridClient">The client used to publish SendGrid events to an Azure EventGrid topic.</param>
        /// <param name="topicHostName">The topic host name where events will be published.</param>
        /// <param name="settings"></param>
        public EventGridEventPublisher(
            IEventGridClient eventGridClient,
            string topicHostName,
            EventGridEventPublisherSettings settings = null)
        {
            _eventGridClient = eventGridClient ?? throw new ArgumentNullException(nameof(eventGridClient));
            _topicHostName = topicHostName ?? throw new ArgumentNullException(nameof(topicHostName));
            _settings = settings ?? EventGridEventPublisherSettings.Default;
            _webhookParser = new WebhookParser();
        }

        /// <inheritdoc cref="IEventGridEventPublisher"/>
        public Task PublishEventsAsync(string sendGridEventsJson, CancellationToken cancellationToken = default)
        {
            var events = _webhookParser.ParseEvents(sendGridEventsJson)
                .Select(e => 
                    e.ToEventGridEvent(
                        _settings.EventSubjectBuilder(e), 
                        _settings.EventTypeBuilder(e)))
                .ToList();

            return _eventGridClient.PublishEventsWithHttpMessagesAsync(_topicHostName, events, cancellationToken: cancellationToken);
        }
    }
}