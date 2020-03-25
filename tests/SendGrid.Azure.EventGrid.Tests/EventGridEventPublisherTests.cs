using FluentAssertions;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moosesoft.SendGrid.Azure.EventGrid;
using Newtonsoft.Json.Linq;
using NSubstitute;
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
        private readonly Uri _topicUri = new Uri("https://topic.host.com/api/events");

        private readonly EventGridEventPublisherSettings _defaultSettings = EventGridEventPublisherSettings.Default;
        private IEventGridClient _eventGridClient;
        private IEventGridEventPublisher _sut;

        [TestInitialize]
        public void Init()
        {
            _eventGridClient = Substitute.For<IEventGridClient>();
            _sut = new EventGridEventPublisher(_eventGridClient, _topicUri);
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
                .PublishEventsWithHttpMessagesAsync(Arg.Is(_topicUri.Host), Arg.Is<IList<EventGridEvent>>(events => ValidatePublishedEvents(events, _defaultSettings)))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task PublishEventsAsync_ReceivedNullJson_Test()
        {
            //Act
            Func<Task> act = () => _sut.PublishEventsAsync(null);

            //Assert
            act.Should()
                .ThrowExactly<ArgumentException>()
                .WithMessage("Events json cannot be null, empty or whitespace*");

            await _eventGridClient.DidNotReceiveWithAnyArgs()
                .PublishEventsWithHttpMessagesAsync(Arg.Any<string>(), Arg.Any<IList<EventGridEvent>>())
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task PublishEventsAsync_ReceivedEmptyJsonArray_Test()
        {
            //Arrange 
            var json = "[]";

            //Act
            await _sut.PublishEventsAsync(json).ConfigureAwait(false);

            //Assert
            await _eventGridClient.DidNotReceiveWithAnyArgs()
                .PublishEventsWithHttpMessagesAsync(Arg.Any<string>(), Arg.Any<IList<EventGridEvent>>())
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task PublishEventsAsync_WithCustomBuilders_Test()
        {
            //Arrange 
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SendGridEvents.json");
            var json = File.ReadAllText(path);

            var settings = new EventGridEventPublisherSettings(
                j => $"/my/custom/subject/{j["sg_event_id"].Value<string>()}",
                j => $"CustomEventType.{j["event"].Value<string>()}");

            var sut = new EventGridEventPublisher(_eventGridClient, _topicUri, settings);

            //Act
            await sut.PublishEventsAsync(json).ConfigureAwait(false);

            //Assert
            await _eventGridClient.Received()
                .PublishEventsWithHttpMessagesAsync(
                    Arg.Is(_topicUri.Host),
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
                if (@event.Data is JObject json &&
                    @event.Id.Equals(json["sg_event_id"].Value<string>(), StringComparison.OrdinalIgnoreCase) &&
                    @event.Subject.Equals(settings.BuildEventSubject(json), StringComparison.OrdinalIgnoreCase) &&
                    @event.EventType.Equals(settings.BuildEventType(json), StringComparison.OrdinalIgnoreCase) &&
                    json["custom_arg1"].Value<string>().Equals("test!"))
                {
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}