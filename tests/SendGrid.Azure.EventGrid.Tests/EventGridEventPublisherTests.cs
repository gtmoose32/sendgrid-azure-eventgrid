using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
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

        private IEventGridClient _eventGridClient;

        [TestInitialize]
        public void Init()
        {
            _eventGridClient = Substitute.For<IEventGridClient>();
            
        }

        [TestMethod]
        public async Task PublishEventsAsync_Test()
        {
            //Arrange 
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SendGridEvents.json");
            var json = File.ReadAllText(path);

            var sut = new EventGridEventPublisher(_eventGridClient, TopicHostName);

            //Act
            await sut.PublishEventsAsync(json).ConfigureAwait(false);

            //Assert
            await _eventGridClient.Received()
                .PublishEventsWithHttpMessagesAsync(Arg.Is(TopicHostName), Arg.Is<IList<EventGridEvent>>(events => events.Count == 11))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task PublishEventsAsync_WithSubjectBuilder_Test()
        {
            //Arrange 
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SendGridEvents.json");
            var json = File.ReadAllText(path);

            var sut = new EventGridEventPublisher(_eventGridClient, TopicHostName, e => $"/sendgrid/events/{e.EventType}/{e.SgEventId}");

            //Act
            await sut.PublishEventsAsync(json).ConfigureAwait(false);

            //Assert
            await _eventGridClient.Received()
                .PublishEventsWithHttpMessagesAsync(Arg.Is(TopicHostName), Arg.Is<IList<EventGridEvent>>(events => events.Count == 11))
                .ConfigureAwait(false);
        }
    }
}