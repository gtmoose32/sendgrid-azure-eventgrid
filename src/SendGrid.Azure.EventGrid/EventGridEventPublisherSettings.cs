using Microsoft.Azure.EventGrid.Models;
using Sendgrid.Webhooks.Events;
using System;

namespace Moosesoft.SendGrid.Azure.EventGrid
{
    /// <summary>
    /// Provides settings to configure instances of <see cref="EventGridEventPublisher"/>.
    /// </summary>
    public class EventGridEventPublisherSettings
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EventGridEventPublisherSettings"/>.
        /// </summary>
        /// <param name="eventTypeBuilder">Custom func used to build EventGrid event type.</param>
        /// <param name="eventSubjectBuilder">Custom func used to build EventGrid event subject.</param>
        public EventGridEventPublisherSettings(
            Func<WebhookEventBase, string> eventTypeBuilder, 
            Func<WebhookEventBase, string> eventSubjectBuilder)
        {
            EventTypeBuilder = eventTypeBuilder ?? throw new ArgumentNullException(nameof(eventTypeBuilder));
            EventSubjectBuilder = eventSubjectBuilder ?? throw new ArgumentNullException(nameof(eventSubjectBuilder));
        }

        /// <summary>
        /// Delegate used to build the event type for an <see cref="EventGridEvent"/>.
        /// </summary>
        public Func<WebhookEventBase, string> EventTypeBuilder { get; }

        /// <summary>
        /// Delegate used to build the event subject for an <see cref="EventGridEvent"/>.
        /// </summary>
        public Func<WebhookEventBase, string> EventSubjectBuilder { get; }

        /// <summary>
        /// Initializes an instance of <see cref="EventGridEventPublisherSettings"/> with default settings.
        /// </summary>
        public static EventGridEventPublisherSettings Default => 
            new EventGridEventPublisherSettings(
                e => $"Twilio.SendGrid.{e.EventType}", 
                e => $"/sendgrid/messages/{e.SgMessageId}");
    }
}