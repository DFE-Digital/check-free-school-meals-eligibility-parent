using AutoFixture;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CheckYourEligibility.Admin.Tests.UseCases
{
    [TestFixture]
    public class AddChildUseCaseTests : TestBase
    {
        private Mock<ILogger<AddChildUseCase>> _loggerMock;
        private AddChildUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AddChildUseCase>>();
            
            var inMemorySettings = new Dictionary<string, string> {
                {"MaxChildren", "99"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            _sut = new AddChildUseCase(_loggerMock.Object, configuration);
        }

        [Test]
        public void Execute_With_Null_Request_Should_Throw_ArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _sut.Execute(null));
            exception.ParamName.Should().Be("request");
        }

        [Test]
        public void Execute_Should_Add_Child_To_Existing_List()
        {
            // Arrange
            var request = new Children { ChildList = new List<Child> { new Child() } };
            var initialCount = request.ChildList.Count;

            // Act
            var result = _sut.Execute(request);

            // Assert
            result.ChildList.Should().HaveCount(initialCount + 1);
            result.ChildList.Last().Should().BeOfType<Child>();
        }
    }
}