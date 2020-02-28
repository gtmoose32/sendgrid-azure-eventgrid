using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sendgrid.Webhooks.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Moosesoft.SendGrid.Azure.EventGrid
{
    /// <summary>
    /// This class was copied from sendgrid-webhooks repo and modified slightly to allow for deserializing events in a case insensitive way.
    /// The original version is found here.  https://github.com/mirajavora/sendgrid-webhooks/blob/master/Sendgrid.Webhooks/Converters/WebhookJsonConverter.cs
    /// </summary>
    public class CaseInsensitiveWebhookJsonConverter : JsonConverter
    {
        //these will be filtered out from the UniqueParams
        private static readonly string[] KnownProperties =
            {
                "event", "email", "category", "timestamp", "ip", "useragent", "type", "reason", "sg_event_id", "sg_message_id",
                "status", "url", "url_offset", "send_at", "tls", "cert_err", "smtp-id", "response", "attempt", "asm_group_id"
            };

        private static readonly IDictionary<string, Type> TypeMapping =
            new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                {"processed", typeof(ProcessedEvent)},
                {"bounce", typeof(BounceEvent)},
                {"click", typeof(ClickEvent)},
                {"deferred", typeof(DeferredEvent)},
                {"delivered", typeof(DeliveryEvent)},
                {"dropped", typeof(DroppedEvent)},
                {"open", typeof(OpenEvent)},
                {"spamreport", typeof(SpamReportEvent)},
                {"unsubscribe", typeof(UnsubscribeEvent)},
                {"group_resubscribe", typeof(GroupResubscribeEvent)},
                {"group_unsubscribe", typeof(GroupUnsubscribeEvent)}
            };

        [ExcludeFromCodeCoverage]
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("The webhook JSON converter does not support write operations.");
        }

        public override object ReadJson(
            JsonReader reader, 
            Type objectType, 
            object existingValue,
            JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            //serialize based on the event type
            jsonObject.TryGetValue("event", StringComparison.CurrentCultureIgnoreCase, out var eventName);

            if (!TypeMapping.ContainsKey(eventName.ToString()))
                throw new NotImplementedException($"Event {eventName} is not implemented yet.");

            Type type = TypeMapping[eventName.ToString()];
            WebhookEventBase webhookItem = (WebhookEventBase)jsonObject.ToObject(type, serializer);

            AddUnmappedPropertiesAsUnique(webhookItem, jsonObject);

            return webhookItem;
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(WebhookEventBase);
        
        private static void AddUnmappedPropertiesAsUnique(WebhookEventBase webhookEvent, JObject jObject)
        {
            var dict = jObject.ToObject<Dictionary<string, object>>();

            foreach (var o in dict.Where(o => !KnownProperties.Contains(o.Key)))
            {
                if (!webhookEvent.UniqueParameters.ContainsKey(o.Key))
                    webhookEvent.UniqueParameters.Add(o.Key, o.Value?.ToString());
            }
        }
    }
}