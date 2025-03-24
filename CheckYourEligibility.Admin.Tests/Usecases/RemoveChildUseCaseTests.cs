using AutoFixture;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.UseCases;
using FluentAssertions;
using NUnit.Framework;

namespace CheckYourEligibility.Admin.Tests.UseCases
{
    [TestFixture]
    public class RemoveChildUseCaseTests
    {
        private Fixture _fixture;
        private RemoveChildUseCase _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _sut = new RemoveChildUseCase();
        }

        [Test]
        public async Task Execute_WhenValidIndex_ShouldRemoveChildFromList()
        {
            // Arrange
            var children = new Children
            {
                ChildList = new List<Child>
                {
                    _fixture.Create<Child>(),
                    _fixture.Create<Child>(),
                    _fixture.Create<Child>()
                }
            };
            var indexToRemove = 1;
            var expectedChildCount = children.ChildList.Count - 1;
            var childToBeRemoved = children.ChildList[indexToRemove];

            // Act
            var result = await _sut.Execute(children, indexToRemove);

            // Assert
            result.ChildList.Should().HaveCount(expectedChildCount);
            result.ChildList.Should().NotContain(childToBeRemoved);
        }

        [Test]
        public async Task Execute_WhenRemovingLastChild_ShouldRemoveCorrectly()
        {
            // Arrange
            var children = new Children
            {
                ChildList = new List<Child>
                {
                    _fixture.Create<Child>(),
                    _fixture.Create<Child>()
                }
            };
            var indexToRemove = children.ChildList.Count - 1;
            var expectedChildCount = children.ChildList.Count - 1;
            var childToBeRemoved = children.ChildList[indexToRemove];

            // Act
            var result = await _sut.Execute(children, indexToRemove);

            // Assert
            result.ChildList.Should().HaveCount(expectedChildCount);
            result.ChildList.Should().NotContain(childToBeRemoved);
        }

        [Test]
        public async Task Execute_WhenRemovingFirstChild_ShouldRemoveCorrectly()
        {
            // Arrange
            var children = new Children
            {
                ChildList = new List<Child>
                {
                    _fixture.Create<Child>(),
                    _fixture.Create<Child>()
                }
            };
            var indexToRemove = 0;
            var expectedChildCount = children.ChildList.Count - 1;
            var childToBeRemoved = children.ChildList[indexToRemove];

            // Act
            var result = await _sut.Execute(children, indexToRemove);

            // Assert
            result.ChildList.Should().HaveCount(expectedChildCount);
            result.ChildList.Should().NotContain(childToBeRemoved);
        }

        [Test]
        public void Execute_WhenInvalidIndex_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var children = new Children
            {
                ChildList = new List<Child>
                {
                    _fixture.Create<Child>(),
                    _fixture.Create<Child>()
                }
            };
            var invalidIndex = children.ChildList.Count + 1;

            // Act & Assert
            Assert.ThrowsAsync<RemoveChildValidationException>(async () =>
                await _sut.Execute(children, invalidIndex));
        }

        [Test]
        public void Execute_WhenNegativeIndex_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var children = new Children
            {
                ChildList = new List<Child>
                {
                    _fixture.Create<Child>()
                }
            };
            var invalidIndex = -1;

            // Act & Assert
            Assert.ThrowsAsync<RemoveChildValidationException>(async () =>
                await _sut.Execute(children, invalidIndex));
        }

        [Test]
        public void Execute_WhenEmptyChildList_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var children = new Children
            {
                ChildList = new List<Child>()
            };
            var indexToRemove = 0;

            // Act & Assert
            Assert.ThrowsAsync<RemoveChildValidationException>(async () =>
                await _sut.Execute(children, indexToRemove));
        }
    }
}