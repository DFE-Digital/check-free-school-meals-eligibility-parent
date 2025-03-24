using AutoFixture;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.UseCases;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace CheckYourEligibility.Admin.Tests.UseCases
{
    [TestFixture]
    public class ProcessChildDetailsUseCaseTests
    {
        private ProcessChildDetailsUseCase _sut;
        private Fixture _fixture;
        private Mock<ISession> _sessionMock;
        private Dictionary<string, byte[]> _sessionStore;

        [SetUp]
        public void SetUp()
        {
            _sut = new ProcessChildDetailsUseCase();
            _fixture = new Fixture();
            _sessionMock = new Mock<ISession>();
            _sessionStore = new Dictionary<string, byte[]>();

            // Setup default session data
            var defaultSessionData = new Dictionary<string, string>
            {
                ["ParentFirstName"] = "John",
                ["ParentLastName"] = "Doe",
                ["ParentDOB"] = "1990-01-01",
                ["ParentEmail"] = "john@example.com",
                ["ParentNINO"] = "AB123456C",
                ["ParentNASS"] = null
            };

            // Setup session mock to store and retrieve bytes
            foreach (var item in defaultSessionData)
            {
                if (item.Value != null)
                {
                    _sessionStore[item.Key] = System.Text.Encoding.UTF8.GetBytes(item.Value);
                }
            }

            _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) =>
                {
                    if (_sessionStore.TryGetValue(key, out byte[] storedValue))
                    {
                        value = storedValue;
                        return true;
                    }
                    value = null;
                    return false;
                });
        }

        [Test]
        public async Task Execute_Should_Create_FsmApplication_With_Session_Data()
        {
            // Arrange
            var children = _fixture.Create<Children>();

            // Act
            var result = await _sut.Execute(children, _sessionMock.Object);

            // Assert
            result.Should().NotBeNull();
            result.Children.Should().BeEquivalentTo(children);
            result.ParentFirstName.Should().Be("John");
            result.ParentLastName.Should().Be("Doe");
            result.ParentDateOfBirth.Should().Be("1990-01-01");
            result.ParentEmail.Should().Be("john@example.com");
            result.ParentNino.Should().Be("AB123456C");
            result.ParentNass.Should().BeNull();
        }

        [Test]
        public async Task Execute_Should_Create_FsmApplication_With_NASS_Number()
        {
            // Arrange
            var children = _fixture.Create<Children>();
            _sessionStore.Remove("ParentNINO");
            _sessionStore["ParentNASS"] = System.Text.Encoding.UTF8.GetBytes("2407001");

            // Act
            var result = await _sut.Execute(children, _sessionMock.Object);

            // Assert
            result.Should().NotBeNull();
            result.ParentNino.Should().BeNull();
            result.ParentNass.Should().Be("2407001");
        }

        [Test]
        public async Task Execute_Should_Handle_Missing_Session_Data()
        {
            // Arrange
            var children = _fixture.Create<Children>();
            _sessionStore.Clear(); // Remove all session data

            // Act
            var result = await _sut.Execute(children, _sessionMock.Object);

            // Assert
            result.Should().NotBeNull();
            result.Children.Should().BeEquivalentTo(children);
            result.ParentFirstName.Should().BeNull();
            result.ParentLastName.Should().BeNull();
            result.ParentDateOfBirth.Should().BeNull();
            result.ParentEmail.Should().BeNull();
            result.ParentNino.Should().BeNull();
            result.ParentNass.Should().BeNull();
        }
    }
}