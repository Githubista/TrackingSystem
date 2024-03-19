using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using PixelService.Resources;
using PixelService.Tracking;
using RabbitMq.CommunicationContracts;
using Xunit;

namespace PixelServiceUnitTests.Tracking
{
    public class TrackingServiceUnitTests
    {
        private TrackingService _systemUnderTest;

        private readonly Mock<ILogger<TrackingService>> _loggerMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;

        public TrackingServiceUnitTests()
        {
            _loggerMock = new Mock<ILogger<TrackingService>>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _systemUnderTest = new TrackingService(_publishEndpointMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public void Given_InvalidPublishEndpointDependency_When_CreateTrackingService_Then_ExceptionIsThrown()
        {
            //act
            Func<TrackingService> act = () =>
                _systemUnderTest = new TrackingService(null, _loggerMock.Object);

            //assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("publishEndpoint");
        }

        [Fact]
        public void Given_InvalidLoggerDependency_When_CreateTrackingService_Then_ExceptionIsThrown()
        {
            //act
            Func<TrackingService> act = () =>
                _systemUnderTest = new TrackingService(_publishEndpointMock.Object, null);

            //assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Theory]
        [InlineData("referer", "userAgent", "127.0.0.1")]
        [InlineData("", "userAgent", "127.0.0.1")]
        [InlineData(null, "userAgent", "127.0.0.1")]
        [InlineData("", "", "127.0.0.1")]
        [InlineData(null, null, "127.0.0.1")]
        [InlineData("refer", "", "127.0.0.1")]
        [InlineData("referer", null, "127.0.0.1")]
        public async Task Given_ValidInput_When_GetAsync_Then_TrackingImageIsReturned(string inputReferer,
            string inputUserAgent, string inputIpAddress)
        {
            //arrange
            var request = new TrackingRequestModel(inputReferer, inputUserAgent, inputIpAddress);

            await ImageLoader.LoadTrackingImage();

            //act
            var response = await _systemUnderTest.GetAsync(request);

            //assert
            response.Should().NotBe(null);
            response.Should().BeOfType<TrackingResponseModel>();
            response.ImageBytes.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("referer", "userAgent", "")]
        [InlineData("referer", "userAgent", " ")]
        [InlineData("referer", "userAgent", null)]
        public async Task Given_InvalidInput_When_GetAsync_Then_TrackingImageIsReturned(string inputReferer,
            string inputUserAgent, string inputIpAddress)
        {
            //arrange
            var request = new TrackingRequestModel(inputReferer, inputUserAgent, inputIpAddress);

            await ImageLoader.LoadTrackingImage();

            //act
            var response = await _systemUnderTest.GetAsync(request);

            //assert
            response.Should().NotBe(null);
            response.Should().BeOfType<TrackingResponseModel>();
            response.ImageBytes.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("referer", "userAgent", "127.0.0.1")]
        [InlineData("", "userAgent", "127.0.0.1")]
        [InlineData(null, "userAgent", "127.0.0.1")]
        [InlineData("", "", "127.0.0.1")]
        [InlineData(null, null, "127.0.0.1")]
        [InlineData("refer", "", "127.0.0.1")]
        [InlineData("referer", null, "127.0.0.1")]
        public async Task Given_ValidInput_When_GetAsync_Then_InputIsPublishedToDedicatedEndpoint(string inputReferer,
            string inputUserAgent, string inputIpAddress)
        {
            //arrange
            var request = new TrackingRequestModel(inputReferer, inputUserAgent, inputIpAddress);

            //act
            await _systemUnderTest.GetAsync(request);

            //assert
            _publishEndpointMock.Verify(x => x.Publish(It.Is<VisitorTrackedEvent>(e => 
                e.Referer == request.Referer &&
                e.UserAgent == request.UserAgent &&
                e.IpAddress == request.IpAddress), CancellationToken.None), 
                Times.Once);
        }

        [Theory]
        [InlineData("referer", "userAgent", "")]
        [InlineData("referer", "userAgent", " ")]
        [InlineData("referer", "userAgent", null)]
        public async Task Given_InvalidInput_When_GetAsync_Then_InputIsNOTPublishedToDedicatedEndpoint(
            string inputReferer,
            string inputUserAgent, string inputIpAddress)
        {
            //arrange
            var request = new TrackingRequestModel(inputReferer, inputUserAgent, inputIpAddress);

            //act
            await _systemUnderTest.GetAsync(request);

            //assert
            _publishEndpointMock.Verify(x => x.Publish(
                    It.IsAny<VisitorTrackedEvent>(), CancellationToken.None),
                Times.Never);
        }

        [Fact]
        public async Task Given_PublishingToEndpointThrowsException_When_GetAsync_Then_ExceptionIsLogged()
        {
            //arrange
            var request = new TrackingRequestModel("inputReferer", "inputUserAgent", "inputIpAddress");

            var exceptionMessage = "fatal exception";
            _publishEndpointMock.Setup(x => x.Publish(
                It.IsAny<VisitorTrackedEvent>(), CancellationToken.None))
                .ThrowsAsync(new Exception(exceptionMessage));

            //act
            await _systemUnderTest.GetAsync(request);

            //assert
            var message = $"Failed to publish {typeof(VisitorTrackedEvent)} event";
            _loggerMock.Verify(logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                    It.Is<EventId>(eventId => eventId.Id == 0),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Equals(message, StringComparison.InvariantCultureIgnoreCase)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

    }
}