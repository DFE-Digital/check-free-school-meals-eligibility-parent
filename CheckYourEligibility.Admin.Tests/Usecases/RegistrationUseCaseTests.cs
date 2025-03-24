using System.Collections.Generic;
using AutoFixture;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.ViewModels;
using CheckYourEligibility.Admin.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CheckYourEligibility.Admin.Tests.UseCases
{
    [TestFixture]
    public class RegistrationUseCaseTests
    {
        private RegistrationUseCase _sut;
        private Mock<ILogger<RegistrationUseCase>> _loggerMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<RegistrationUseCase>>();
            _sut = new RegistrationUseCase(_loggerMock.Object);
            _fixture = new Fixture();
        }

        [Test]
        public async System.Threading.Tasks.Task Execute_WithValidJson_DeserializesViewModel()
        {
            // Arrange: create a FsmApplication instance.
            var fsmApplication = _fixture.Build<FsmApplication>()
                .With(x => x.ParentFirstName, "John")
                .With(x => x.ParentLastName, "Doe")
                .With(x => x.Children, new Children
                {
                    ChildList = new List<Child>
                    {
                        new Child { FirstName = "Jane", LastName = "Doe", ChildIndex = 1 }
                    }
                })
                .Create();

            // Build the expected view model based on the FsmApplication.
            var expected = new ApplicationConfirmationEntitledViewModel
            {
                ParentName = $"{fsmApplication.ParentFirstName} {fsmApplication.ParentLastName}",
                Children = new List<ApplicationConfirmationEntitledChildViewModel>()
            };

            if (fsmApplication.Children?.ChildList != null)
            {
                foreach (var child in fsmApplication.Children.ChildList)
                {
                    expected.Children.Add(new ApplicationConfirmationEntitledChildViewModel
                    {
                        ParentName = expected.ParentName,
                        ChildName = $"{child.FirstName} {child.LastName}",
                        Reference = $"-{child.ChildIndex}"
                    });
                }
            }

            // Serialize the FsmApplication to JSON.
            var json = JsonConvert.SerializeObject(fsmApplication);

            // Act
            var result = await _sut.Execute(json);

            // Assert
            result.Should().BeEquivalentTo(expected);
        }
    }
}
