using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json.Linq;
using System;

namespace Moosesoft.SendGrid.Azure.EventGrid
{
    /// <summary>
    /// Provides settings to configure instances of <see cref="EventGridEventPublisher"/>.
    /// </summary>
    public class EventGridEventPublisherSettings
    {
        private const string EventTypeKey = "event";
        private const string MessageIdKey = "sg_message_id";

        /// <summary>
        /// Initializes a new instance of <see cref="EventGridEventPublisherSettings"/>.
        /// </summary>
        /// <param name="eventTypeBuilder">Custom func used to build EventGrid event type.</param>
        /// <param name="eventSubjectBuilder">Custom func used to build EventGrid event subject.</param>
        public EventGridEventPublisherSettings(
            Func<JObject, string> eventTypeBuilder, 
            Func<JObject, string> eventSubjectBuilder)
        {
            BuildEventType = eventTypeBuilder ?? throw new ArgumentNullException(nameof(eventTypeBuilder));
            BuildEventSubject = eventSubjectBuilder ?? throw new ArgumentNullException(nameof(eventSubjectBuilder));
        }

        /// <summary>
        /// Delegate used to build the event type for an <see cref="EventGridEvent"/>.
        /// </summary>
        public Func<JObject, string> BuildEventType { get; }

        /// <summary>
        /// Delegate used to build the event subject for an <see cref="EventGridEvent"/>.
        /// </summary>
        public Func<JObject, string> BuildEventSubject { get; }

        /// <summary>
        /// Initializes an instance of <see cref="EventGridEventPublisherSettings"/> with default settings.
        /// </summary>
        public static EventGridEventPublisherSettings Default => 
            new EventGridEventPublisherSettings(DefaultBuildEventType, DefaultBuildEventSubject);

        private static string DefaultBuildEventType(JObject sendGridEvent) =>
            $"Twilio.SendGrid.{sendGridEvent.GetPropertyStringValue(EventTypeKey)}";

        private static string DefaultBuildEventSubject(JObject sendGridEvent) =>
            $"/sendgrid/messages/{sendGridEvent.GetPropertyStringValue(MessageIdKey)}";
    }
}