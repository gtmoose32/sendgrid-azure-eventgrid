using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json.Linq;
using System;

namespace Moosesoft.SendGrid.Azure.EventGrid
{
    internal static class Extensions
    {
        private const string InvalidOperationExceptionMessageTemplate = "'{0}' property cannot be extracted from the SendGrid event json.";
        private const string EventIdKey = "sg_event_id";

        public static EventGridEvent ToEventGridEvent(this JObject @event, EventGridEventPublisherSettings settings) =>
            new EventGridEvent(
                @event.TryGetValue(EventIdKey, StringComparison.OrdinalIgnoreCase, out var token) 
                    ? token.Value<string>()
                    : Guid.NewGuid().ToString(),
                settings.BuildEventSubject(@event),
                @event,
                settings.BuildEventType(@event),
                DateTime.UtcNow,
                "1");

        public static string GetPropertyStringValue(this JObject json, string propertyName)
        {
            if (!json.TryGetValue(propertyName, StringComparison.OrdinalIgnoreCase, out var token))
                throw new InvalidOperationException(
                    string.Format(InvalidOperationExceptionMessageTemplate, propertyName));

            return token.Value<string>();
        }
    }
}