using Azure.Messaging.EventGrid;
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
            new(DefaultBuildEventType, DefaultBuildEventSubject);

        private static string DefaultBuildEventType(JObject sendGridEvent) =>
            $"twilio.sendgrid.{GetPropertyStringValue(sendGridEvent, EventTypeKey)}";

        private static string DefaultBuildEventSubject(JObject sendGridEvent)
        {
            var messageId = $"unknown-{MessageIdKey}";
            if (sendGridEvent.TryGetValue(MessageIdKey, StringComparison.OrdinalIgnoreCase, out var token))
                messageId = token.Value<string>();

            return $"/sendgrid/messages/{messageId}";
        }

        private const string InvalidOperationExceptionMessageTemplate =
            "'{0}' property cannot be extracted from the SendGrid event json.";

        private static string GetPropertyStringValue(JObject json, string propertyName)
        {
            if (!json.TryGetValue(propertyName, StringComparison.OrdinalIgnoreCase, out var token))
                throw new InvalidOperationException(
                    string.Format(InvalidOperationExceptionMessageTemplate, propertyName));

            return token.Value<string>();
        }
    }
}