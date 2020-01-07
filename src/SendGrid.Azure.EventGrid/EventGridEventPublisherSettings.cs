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
        public EventGridEventPublisherSettings()
            : this(BuildEventType, BuildEventSubject)
        {
        }

        private EventGridEventPublisherSettings(
            Func<WebhookEventBase, string> eventTypeBuilder, 
            Func<WebhookEventBase, string> eventSubjectBuilder)
        {
            EventTypeBuilder = eventTypeBuilder;
            EventSubjectBuilder = eventSubjectBuilder;
        }

        /// <summary>
        /// Delegate used to build the event type for an <see cref="EventGridEvent"/>.
        /// </summary>
        public Func<WebhookEventBase, string> EventTypeBuilder { get; set; }

        /// <summary>
        /// Delegate used to build the event subject for an <see cref="EventGridEvent"/>.
        /// </summary>
        public Func<WebhookEventBase, string> EventSubjectBuilder { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="EventGridEventPublisherSettings"/> with default settings.
        /// </summary>
        public static EventGridEventPublisherSettings Default => 
            new EventGridEventPublisherSettings(BuildEventType, BuildEventSubject);

        private static string BuildEventType(WebhookEventBase @event) => $"Twilio.SendGrid.{@event.EventType}";

        private static string BuildEventSubject(WebhookEventBase @event) =>
            $"/sendgrid/messages/{@event.SgMessageId}";
    }
}