using System;
using Microsoft.Azure.EventGrid.Models;
using Sendgrid.Webhooks.Events;

namespace SendGrid.Azure.EventGrid
{
    public static class Extensions
    {
        public static EventGridEvent ToEventGridEvent(this WebhookEventBase @event, string subject)
        {
            return new EventGridEvent(
                Guid.NewGuid().ToString(),
                subject,
                @event,
                @event.EventType.ToString(),
                DateTime.UtcNow,
                "1");
        }

        public static EventGridEvent ToEventGridEvent(this WebhookEventBase @event, Func<WebhookEventBase, string> buildSubject) 
            => @event.ToEventGridEvent(buildSubject(@event));
    }
}