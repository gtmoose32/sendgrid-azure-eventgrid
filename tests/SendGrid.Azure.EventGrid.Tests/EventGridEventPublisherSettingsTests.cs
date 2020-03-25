using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moosesoft.SendGrid.Azure.EventGrid;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SendGrid.Azure.EventGrid.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class EventGridEventPublisherSettingsTests
    {
        private EventGridEventPublisherSettings _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = EventGridEventPublisherSettings.Default;
        }

        [TestMethod]
        public void BuildEventType_ThrowsInvalidOperationException_Test()
        {
            //Arrange
            var json = new JObject();

            //Act
            Action act = () => _sut.BuildEventType(json);

            //Assert
            act.Should()
                .ThrowExactly<InvalidOperationException>()
                .WithMessage("'event' property cannot be extracted from the SendGrid event json.");
        }

        [TestMethod]
        public void BuildEventSubject_ThrowsInvalidOperationException_Test()
        {
            //Arrange
            var json = new JObject();

            //Act
            Action act = () => _sut.BuildEventSubject(json);

            //Assert
            act.Should()
                .ThrowExactly<InvalidOperationException>()
                .WithMessage("'sg_message_id' property cannot be extracted from the SendGrid event json.");
        }
    }
}