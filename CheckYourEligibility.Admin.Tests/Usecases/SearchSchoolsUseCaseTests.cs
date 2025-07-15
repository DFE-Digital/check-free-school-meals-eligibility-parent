using AutoFixture;
using CheckYourEligibility.Admin.UseCases;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using Moq;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Models;
using FluentAssertions;

namespace CheckYourEligibility.Admin.Tests.Usecases
{
    [TestFixture]
    public class SearchSchoolsUseCaseTests
    {
        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _parentGatewayMock = new Mock<IParentGateway>();
            _sut = new SearchSchoolsUseCase(_parentGatewayMock.Object);
        }

        private Fixture _fixture;
        private Mock<IParentGateway> _parentGatewayMock;
        private SearchSchoolsUseCase _sut;

        [Test]
        public void Execute_QueryLessThan3Characters_ThrowsArgumentExcpetion()
        {
            // Arrange
            var shortQuery = "ab";
            var la = _fixture.Create<string>();

            // Act
            var ex = Assert.ThrowsAsync<ArgumentException>(() => _sut.Execute(shortQuery, la));

            //Assert
            Assert.That(ex.Message, Is.EqualTo("Query must be at least 3 characters long. (Parameter 'query')"));
        }

        [Test]
        public async Task Execute_ValidQuery_ReturnsExpectedResults()
        {
            // Arrange
            var query = "TestSchool";
            var la = "TestLA";
            var expectedResults = _fixture.CreateMany<Establishment>(5);

            _parentGatewayMock.Setup(x => x.GetSchool(query, la))
            .ReturnsAsync(new EstablishmentSearchResponse { Data = expectedResults });

            // Act
            var result = await _sut.Execute(query, la);

            // Assert
            expectedResults.Should().BeEquivalentTo(result);
            Assert.That(result, Is.Not.Null);
        }
    }
}
