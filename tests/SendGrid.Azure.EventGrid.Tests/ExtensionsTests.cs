using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moosesoft.SendGrid.Azure.EventGrid;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;

namespace SendGrid.Azure.EventGrid.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        public void BuildEventType_MissingEventId_MessageId_Test()
        {
            //Arrange
            var json = JObject.Parse(@"{ ""event"": ""delivered"" }");

            //Act
            var result = json.ToEventGridEvent(EventGridEventPublisherSettings.Default);

           //Assert
           result.Should().NotBeNull();
           result.EventType.Should().NotBeNullOrWhiteSpace();
           result.Subject.Should().Be("/sendgrid/messages/unknown-sg_message_id");
        }
    }
}