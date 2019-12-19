using Microsoft.Azure.EventGrid.Models;
using Sendgrid.Webhooks.Events;
using System;

namespace Moosesoft.SendGrid.Azure.EventGrid
{
    /// <summary>
    /// Provides a set of extension methods for converting SendGrid events into Azure EventGrid events.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extension method that converts a SendGrid event into an Azure EventGrid event.
        /// </summary>
        /// <param name="event">SendGrid event to convert to an Azure EventGrid event.</param>
        /// <param name="subject">Subject will be used to set EventGrid event subject that is returned.</param>
        /// <returns><see cref="EventGridEvent"/></returns>
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

        /// <summary>
        /// Extension method that converts a SendGrid event into an Azure EventGrid event.
        /// </summary>
        /// <param name="event">SendGrid event.</param>
        /// <param name="buildSubject">Function used to build the EventGrid event subject that is returned.</param>
        /// <returns><see cref="EventGridEvent"/></returns>
        public static EventGridEvent ToEventGridEvent(this WebhookEventBase @event, Func<WebhookEventBase, string> buildSubject) 
            => @event.ToEventGridEvent(buildSubject(@event));
    }
}