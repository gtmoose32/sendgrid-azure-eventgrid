using Microsoft.Azure.EventGrid.Models;
using Sendgrid.Webhooks.Events;
using System;

namespace Moosesoft.SendGrid.Azure.EventGrid
{
    internal static class Extensions
    {
        public static EventGridEvent ToEventGridEvent(this WebhookEventBase @event, string subject, string eventType)
        {
            return new EventGridEvent(
                Guid.NewGuid().ToString(),
                subject,
                @event,
                eventType,
                DateTime.UtcNow,
                "1");
        }
    }
}