using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moosesoft.SendGrid.Azure.EventGrid;
using NSubstitute;
using Sendgrid.Webhooks.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SendGrid.Azure.EventGrid.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class EventGridEventPublisherTests
    {
        private const string TopicHostName = "topic.host.com";

        private readonly EventGridEventPublisherSettings _defaultSettings = EventGridEventPublisherSettings.Default;
        private IEventGridClient _eventGridClient;
        private IEventGridEventPublisher _sut;

        [TestInitialize]
        public void Init()
        {
            _eventGridClient = Substitute.For<IEventGridClient>();
            _sut = new EventGridEventPublisher(_eventGridClient, TopicHostName);
        }

        [TestMethod]
        public async Task PublishEventsAsync_Test()
        {
            //Arrange 
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SendGridEvents.json");
            var json = File.ReadAllText(path);

            //Act
            await _sut.PublishEventsAsync(json).ConfigureAwait(false);

            //Assert
            await _eventGridClient.Received()
                .PublishEventsWithHttpMessagesAsync(Arg.Is(TopicHostName), Arg.Is<IList<EventGridEvent>>(events => ValidatePublishedEvents(events, _defaultSettings)))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task PublishEventsAsync_WithCustomBuilders_Test()
        {
            //Arrange 
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SendGridEvents.json");
            var json = File.ReadAllText(path);

            var settings = new EventGridEventPublisherSettings(
                e => $"/my/custom/subject/{e.SgEventId}",
                e => $"CustomEventType.{e.EventType}");

            var sut = new EventGridEventPublisher(_eventGridClient, TopicHostName, settings);

            //Act
            await sut.PublishEventsAsync(json).ConfigureAwait(false);

            //Assert
            await _eventGridClient.Received()
                .PublishEventsWithHttpMessagesAsync(
                    Arg.Is(TopicHostName),
                    Arg.Is<IList<EventGridEvent>>(events => ValidatePublishedEvents(events, settings)))
                .ConfigureAwait(false);
        }

        private static bool ValidatePublishedEvents(
            ICollection<EventGridEvent> events,
            EventGridEventPublisherSettings settings)
        {
            if (events.Count != 11) return false;

            foreach (var @event in events)
            {
                if (@event.Data is WebhookEventBase webhookEvent &&
                    @event.Id.Equals(webhookEvent.SgEventId, StringComparison.OrdinalIgnoreCase) &&
                    @event.Subject.Equals(settings.EventSubjectBuilder(webhookEvent), StringComparison.OrdinalIgnoreCase) &&
                    @event.EventType.Equals(settings.EventTypeBuilder(webhookEvent), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}