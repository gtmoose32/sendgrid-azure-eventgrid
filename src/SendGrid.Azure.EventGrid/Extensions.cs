using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json.Linq;
using System;

namespace Moosesoft.SendGrid.Azure.EventGrid
{
    internal static class Extensions
    {
        private const string InvalidOperationExceptionMessageTemplate = "'{0}' property cannot be extracted from the SendGrid event json.";
        private const string EventIdKey = "sg_event_id";

        public static EventGridEvent ToEventGridEvent(this JObject @event, EventGridEventPublisherSettings settings)
        {
            if (!@event.TryGetValue(EventIdKey, StringComparison.OrdinalIgnoreCase, out var eventId))
                throw EventIdKey.CreateInvalidOperationException();

            return new EventGridEvent(
                eventId.Value<string>(),
                settings.BuildEventSubject(@event),
                @event,
                settings.BuildEventType(@event),
                DateTime.UtcNow,
                "1");
        }

        public static InvalidOperationException CreateInvalidOperationException(this string jobjectKey) =>
            new InvalidOperationException(string.Format(InvalidOperationExceptionMessageTemplate, jobjectKey));

    }
}