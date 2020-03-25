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
    public class ExtensionsTests
    {
        [TestMethod]
        public void BuildEventType_ThrowsInvalidOperationException_Test()
        {
            //Arrange
            var json = new JObject();

            //Act
            Action act = () => json.ToEventGridEvent(EventGridEventPublisherSettings.Default);

            //Assert
            act.Should()
                .ThrowExactly<InvalidOperationException>()
                .WithMessage("'sg_event_id' property cannot be extracted from the SendGrid event json.");
        }
    }
}