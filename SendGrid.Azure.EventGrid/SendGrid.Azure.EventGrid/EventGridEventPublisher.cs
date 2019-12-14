using Microsoft.Azure.EventGrid;
using Sendgrid.Webhooks.Events;
using Sendgrid.Webhooks.Service;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SendGrid.Azure.EventGrid
{
    public class EventGridEventPublisher : IEventGridEventPublisher
    {
        private readonly IEventGridClient _eventGridClient;
        private readonly string _topicHostName;
        private readonly Func<WebhookEventBase, string> _buildSubject;
        private readonly WebhookParser _webhookParser;

        public EventGridEventPublisher(IEventGridClient eventGridClient, string topicHostName, Func<WebhookEventBase, string> buildSubject = null)
        {
            _eventGridClient = eventGridClient;
            _topicHostName = topicHostName;
            _buildSubject = buildSubject;
            _webhookParser = new WebhookParser();

        }

        public async Task PublishEventsAsync(string sendGridEventsJson, CancellationToken cancellationToken = default)
        {
            var events = _webhookParser.ParseEvents(sendGridEventsJson)
                .Select(e => _buildSubject != null
                    ? e.ToEventGridEvent(_buildSubject(e))
                    : e.ToEventGridEvent($"/events/{e.SgEventId}"))
                .ToList();

            await _eventGridClient.PublishEventsWithHttpMessagesAsync(_topicHostName, events, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }
}