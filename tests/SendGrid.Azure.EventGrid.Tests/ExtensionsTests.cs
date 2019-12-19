using AutoFixture;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moosesoft.SendGrid.Azure.EventGrid;
using Sendgrid.Webhooks.Events;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SendGrid.Azure.EventGrid.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ExtensionsTests
    {
        private readonly IFixture _fixture = new Fixture();

        private WebhookEventBase _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = _fixture.Build<BounceEvent>()
                .With(e => e.EventType, WebhookEventType.Bounce)
                .Without(be => be.UniqueParameters)
                .Create();
        }

        [TestMethod]
        public void ToEventGridEvent_WithSubject_Test()
        {
            //Arrange 
            const string subject = "test-subject";

            //Act
            var result = _sut.ToEventGridEvent(subject);

            //Assert
            result.Should().NotBeNull();
            result.Subject.Should().Be(subject);
            result.Data.Should().BeOfType<BounceEvent>();
        }

        [TestMethod]
        public void ToEventGridEvent_WithSubjectBuilder_Test()
        {
            //Arrange 
            const string correlationKey = "correlation-id";
            var id = _fixture.Create<string>();
            _sut.UniqueParameters = new Dictionary<string, string> {{correlationKey, id}};
            
            //Act
            var result = _sut.ToEventGridEvent(e => $"/sendgrid/events/{e.EventType}/{e.UniqueParameters[correlationKey]}");

            //Assert
            result.Should().NotBeNull();
            result.Subject.Should().Be($"/sendgrid/events/{WebhookEventType.Bounce}/{_sut.UniqueParameters[correlationKey]}");
            result.Data.Should().BeOfType<BounceEvent>();
        }
    }
}
