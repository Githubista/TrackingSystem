using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMq.CommunicationContracts;
using StorageService;
using StorageService.Consumers;
using StorageService.Settings;
using Xunit;

namespace StorageServiceUnitTests.Consumers
{
    public class VisitorTrackedEventConsumerUnitTests
    {
        private VisitorTrackedEventConsumer _systemUnderTest;

        private readonly Mock<ILogger<VisitorTrackedEventConsumer>> _loggerMock;
        private readonly Mock<IFileWriter> _fileWriterMock;
        private readonly Mock<IOptionsMonitor<VisitorsFileSettings>> _fileSettingsMock;
        private readonly Mock<ConsumeContext<VisitorTrackedEvent>> _consumerContextMock;
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
        private readonly VisitorsFileSettings _fileSettings;

        public VisitorTrackedEventConsumerUnitTests()
        {
            _fileSettings = new VisitorsFileSettings
            {
                FilePath = "dir/file.txt"
            };
            _fileSettingsMock = new Mock<IOptionsMonitor<VisitorsFileSettings>>();
            _fileSettingsMock.Setup(x => x.CurrentValue).Returns(_fileSettings);

            _loggerMock = new Mock<ILogger<VisitorTrackedEventConsumer>>();
            _fileWriterMock = new Mock<IFileWriter>();
            _consumerContextMock = new Mock<ConsumeContext<VisitorTrackedEvent>>();
            _dateTimeProviderMock = new Mock<IDateTimeProvider>();

            _systemUnderTest = new VisitorTrackedEventConsumer(_fileSettingsMock.Object,
                _fileWriterMock.Object,
                _dateTimeProviderMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public void Given_InvalidLoggerDependency_When_CreateNewConsumer_Then_ExceptionIsThrown()
        {
            //act
            Func<VisitorTrackedEventConsumer> act = () =>
                _systemUnderTest =
                    new VisitorTrackedEventConsumer(_fileSettingsMock.Object, _fileWriterMock.Object, _dateTimeProviderMock.Object, null);

            //assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Given_InvalidIpAddress_When_ConsumeMessage_Then_ValidationExceptionIsThrown(string ipAddress)
        {
            //arrange
            var message = new VisitorTrackedEvent { IpAddress = ipAddress, UserAgent = "ua", Referer = "r" };
            _consumerContextMock.Setup(x => x.Message).Returns(message);

            //act
            var act = async () => await _systemUnderTest.Consume(_consumerContextMock.Object);

            //assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task
            Given_ValidMessageAndUserAgentIsUnknown_When_ConsumeMessage_Then_NullForUserAgentIsWrittenToFile(
                string userAgent)
        {
            //arrange
            var message = new VisitorTrackedEvent { IpAddress = "ipAddress", UserAgent = userAgent, Referer = "r" };
            _consumerContextMock.Setup(x => x.Message).Returns(message);
            var now = DateTime.Parse("2024-03-19T12:22:22Z").ToString("O");
            _dateTimeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);
            
            //act
            await _systemUnderTest.Consume(_consumerContextMock.Object);

            //assert
            var expectedInput = $"{now}|{message.Referer}|null|{message.IpAddress}{Environment.NewLine}";
            _fileWriterMock.Verify(x => x.AppendToFile(_fileSettings.FilePath, expectedInput), 
                Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task
            Given_ValidMessageAndRefererIsUnknown_When_ConsumeMessage_Then_NullForRefererIsWrittenToFile(
                string referer)
        {
            //arrange
            var message = new VisitorTrackedEvent { IpAddress = "ipAddress", UserAgent = "userAgent", Referer = referer };
            _consumerContextMock.Setup(x => x.Message).Returns(message);
            var now = DateTime.Parse("2024-03-19T12:22:22Z").ToString("O");
            _dateTimeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);

            //act
            await _systemUnderTest.Consume(_consumerContextMock.Object);

            //assert
            var expectedInput = $"{now}|null|{message.UserAgent}|{message.IpAddress}{Environment.NewLine}";
            _fileWriterMock.Verify(x => x.AppendToFile(_fileSettings.FilePath, expectedInput),
                Times.Once);
        }

        [Fact]
        public async Task
            Given_ValidMessage_When_ConsumeMessage_Then_InputIsWrittenToFile()
        {
            //arrange
            var message = new VisitorTrackedEvent { IpAddress = "ipAddress", UserAgent = "userAgent", Referer = "referer" };
            _consumerContextMock.Setup(x => x.Message).Returns(message);
            var now = DateTime.Parse("2024-03-19T12:22:22Z").ToString("O");
            _dateTimeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);

            //act
            await _systemUnderTest.Consume(_consumerContextMock.Object);

            //assert
            var expectedInput = $"{now}|{message.Referer}|{message.UserAgent}|{message.IpAddress}{Environment.NewLine}";
            _fileWriterMock.Verify(x => x.AppendToFile(_fileSettings.FilePath, expectedInput),
                Times.Once);
        }

        [Fact]
        public async Task Given_WritingToFileThrowsException_When_ConsumeMessage_Then_ExceptionIsLogged()
        {
            //arrange
            var message = new VisitorTrackedEvent { IpAddress = "ipAddress", UserAgent = "userAgent", Referer = "referer" };
            _consumerContextMock.Setup(x => x.Message).Returns(message);
            var now = DateTime.Parse("2024-03-19T12:22:22Z").ToString("O");
            _dateTimeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);

            var exceptionMessage = "fatal exception";
            _fileWriterMock.Setup(x => x.AppendToFile(
                    It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            //act
            var act = async () => await _systemUnderTest.Consume(_consumerContextMock.Object);

            //assert
            await act.Should().ThrowAsync<Exception>().WithMessage(exceptionMessage);

            var loggedMessage = $"Writing {typeof(VisitorTrackedEvent)} event with referer: {message.Referer}, user agent: {message.UserAgent} and Ip: {message.IpAddress} failed!";
            _loggerMock.Verify(logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                    It.Is<EventId>(eventId => eventId.Id == 0),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Equals(loggedMessage, StringComparison.InvariantCultureIgnoreCase)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}