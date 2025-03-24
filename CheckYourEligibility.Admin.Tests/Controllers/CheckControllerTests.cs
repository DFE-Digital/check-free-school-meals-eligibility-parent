using AutoFixture;
using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility.Admin.Controllers;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.UseCases;
using CheckYourEligibility.Admin.ViewModels;
using CsvHelper.Configuration.Attributes;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Security.Claims;
using CheckYourEligibility.Admin.Gateways;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using Child = CheckYourEligibility.Admin.Models.Child;
using School = CheckYourEligibility.Admin.Models.School;

namespace CheckYourEligibility.Admin.Tests.Controllers
{
    [TestFixture]
    public class CheckControllerTests : TestBase
    {
        // Mocks for use cases
        private ILogger<CheckController> _loggerMock;
        private Mock<ILoadParentDetailsUseCase> _loadParentDetailsUseCaseMock;
        private Mock<IPerformEligibilityCheckUseCase> _performEligibilityCheckUseCaseMock;
        private Mock<IGetCheckStatusUseCase> _getCheckStatusUseCaseMock;
        private Mock<IEnterChildDetailsUseCase> _enterChildDetailsUseCaseMock;
        private Mock<IProcessChildDetailsUseCase> _processChildDetailsUseCaseMock;
        private Mock<IAddChildUseCase> _addChildUseCaseMock;
        private Mock<IRemoveChildUseCase> _removeChildUseCaseMock;
        private Mock<IChangeChildDetailsUseCase> _changeChildDetailsUseCaseMock;
        private Mock<IRegistrationResponseUseCase> _registrationResponseUseCaseMock;
        private Mock<ICreateUserUseCase> _createUserUseCaseMock;
        private Mock<ISubmitApplicationUseCase> _submitApplicationUseCaseMock;
        private Mock<IValidateParentDetailsUseCase> _validateParentDetailsUseCaseMock;
        private Mock<IInitializeCheckAnswersUseCase> _initializeCheckAnswersUseCaseMock;

        // Legacy service mocks - keep temporarily during transition
        private Mock<IParentGateway> _parentGatewayMock;
        private Mock<ICheckGateway> _checkGatewayMock;

        // System under test
        private CheckController _sut;

        [SetUp]
        public void SetUp()
        {
            // Initialize legacy service mocks
            _parentGatewayMock = new Mock<IParentGateway>();
            _checkGatewayMock = new Mock<ICheckGateway>();
            _loggerMock = Mock.Of<ILogger<CheckController>>();

            // Initialize use case mocks
            _loadParentDetailsUseCaseMock = new Mock<ILoadParentDetailsUseCase>();
            _performEligibilityCheckUseCaseMock = new Mock<IPerformEligibilityCheckUseCase>();
            _getCheckStatusUseCaseMock = new Mock<IGetCheckStatusUseCase>();
            _enterChildDetailsUseCaseMock = new Mock<IEnterChildDetailsUseCase>();
            _processChildDetailsUseCaseMock = new Mock<IProcessChildDetailsUseCase>();
            _addChildUseCaseMock = new Mock<IAddChildUseCase>();
            _removeChildUseCaseMock = new Mock<IRemoveChildUseCase>();
            _changeChildDetailsUseCaseMock = new Mock<IChangeChildDetailsUseCase>();
            _registrationResponseUseCaseMock = new Mock<IRegistrationResponseUseCase>();
            _createUserUseCaseMock = new Mock<ICreateUserUseCase>();
            _submitApplicationUseCaseMock = new Mock<ISubmitApplicationUseCase>();
            _validateParentDetailsUseCaseMock = new Mock<IValidateParentDetailsUseCase>();
            _initializeCheckAnswersUseCaseMock = new Mock<IInitializeCheckAnswersUseCase>();

            // Initialize controller with all dependencies
            _sut = new CheckController(
                _loggerMock,
                _parentGatewayMock.Object,
                _checkGatewayMock.Object,
                _configMock.Object,
                _loadParentDetailsUseCaseMock.Object,
                _performEligibilityCheckUseCaseMock.Object,
                _enterChildDetailsUseCaseMock.Object,
                _processChildDetailsUseCaseMock.Object,
                _getCheckStatusUseCaseMock.Object,
                _addChildUseCaseMock.Object,
                _removeChildUseCaseMock.Object,
                _changeChildDetailsUseCaseMock.Object,
                _createUserUseCaseMock.Object,
                _submitApplicationUseCaseMock.Object,
                _validateParentDetailsUseCaseMock.Object
            );
            
            SetUpSessionData();


            base.SetUp();

            _sut.TempData = _tempData;
            _sut.ControllerContext.HttpContext = _httpContext.Object;
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public async Task Enter_Details_Get_When_NoResponseInTempData_Should_ReturnView()
        {
            // Arrange
            var expectedParent = _fixture.Create<ParentGuardian>();
            var expectedErrors = new Dictionary<string, List<string>>();

            _loadParentDetailsUseCaseMock
                .Setup(x => x.Execute(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((expectedParent, expectedErrors));

            // Act
            var result = await _sut.Enter_Details();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().Be(expectedParent);
        }

        [Test]
        [TestCase(0, "AB123456C", null)] // NinSelected = 0
        [TestCase(1, null, "2407001")]   // AsrnSelected = 1
        public async Task Enter_Details_Post_When_ValidationFails_Should_RedirectBack(
    int ninAsrSelectValue,
    string? nino,
    string? nass)
        {
            // Arrange
            var request = _fixture.Create<ParentGuardian>();
            request.NationalInsuranceNumber = nino;
            request.NationalAsylumSeekerServiceNumber = nass;
            request.NinAsrSelection = (ParentGuardian.NinAsrSelect)ninAsrSelectValue;
            request.Day = "1";
            request.Month = "1";
            request.Year = "1990";

            var validationResult = new ValidationResult
            {
                IsValid = false,
                Errors = new Dictionary<string, List<string>>
        {
            { "Error Key", new List<string> { "Error Message" } }
        }
            };

            _validateParentDetailsUseCaseMock
                .Setup(x => x.Execute(request, It.IsAny<ModelStateDictionary>()))
                .Returns(validationResult);

            // Act
            var result = await _sut.Enter_Details(request);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Enter_Details");

            // Verify TempData contains expected values
            _sut.TempData.Should().ContainKey("ParentDetails");
            _sut.TempData.Should().ContainKey("Errors");

            // Verify the mock was called with correct parameters
            _validateParentDetailsUseCaseMock.Verify(
                x => x.Execute(request, It.IsAny<ModelStateDictionary>()),
                Times.Once);
        }

        [Test]
        [TestCase(ParentGuardian.NinAsrSelect.NinSelected, "AB123456C", null)]
        [TestCase(ParentGuardian.NinAsrSelect.AsrnSelected, null, "2407001")]
        public async Task Enter_Details_Post_When_Valid_Should_ProcessAndRedirectToLoader(
    ParentGuardian.NinAsrSelect ninasSelection,
    string? nino,
    string? nass)
        {
            // Arrange
            var request = _fixture.Create<ParentGuardian>();
            request.NationalInsuranceNumber = nino;
            request.NationalAsylumSeekerServiceNumber = nass;
            request.NinAsrSelection = ninasSelection;
            request.Day = "01";
            request.Month = "01";
            request.Year = "1990";

            var validationResult = new ValidationResult { IsValid = true };
            var checkEligibilityResponse = _fixture.Create<CheckEligibilityResponse>();

            _validateParentDetailsUseCaseMock
                .Setup(x => x.Execute(request, It.IsAny<ModelStateDictionary>()))
                .Returns(validationResult);

            _performEligibilityCheckUseCaseMock
                .Setup(x => x.Execute(request, _sut.HttpContext.Session))
                .ReturnsAsync(checkEligibilityResponse);

            // Act
            var result = await _sut.Enter_Details(request);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Loader");
            _sut.TempData["Response"].Should().NotBeNull();

            _validateParentDetailsUseCaseMock.Verify(
                x => x.Execute(request, It.IsAny<ModelStateDictionary>()),
                Times.Once);

            _performEligibilityCheckUseCaseMock.Verify(
                x => x.Execute(request, _sut.HttpContext.Session),
                Times.Once);
        }


        [Test]
        public void Enter_Child_Details_Get_Should_Handle_Initial_Load()
        {
            // Arrange
            var expectedResult = new Children { ChildList = new List<Child> { new Child() } };

            _enterChildDetailsUseCaseMock
                .Setup(x => x.Execute(
                    It.IsAny<string>(),
                    It.IsAny<bool?>()
                    ))
                .Returns(expectedResult);

            // Act
            var result = _sut.Enter_Child_Details() as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result.Model.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void Enter_Child_Details_Post_When_Valid_Should_Process_And_Return_CheckAnswers()
        {
            // Arrange
            var request = _fixture.Create<Children>();
            var fsmApplication = _fixture.Create<FsmApplication>();

            _processChildDetailsUseCaseMock
                .Setup(x => x.Execute(request, _sut.HttpContext.Session))
                .ReturnsAsync(fsmApplication);

            // Act
            var result = _sut.Enter_Child_Details(request);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Check_Answers");
            viewResult.Model.Should().Be(fsmApplication);

            _processChildDetailsUseCaseMock.Verify(
                x => x.Execute(request, _sut.HttpContext.Session),
                Times.Once);
        }

        [Test]
        public async Task Add_Child_Should_Execute_UseCase_And_Redirect()
        {
            // Arrange
            var request = _fixture.Create<Children>();
            var updatedChildren = _fixture.Create<Children>();

            _addChildUseCaseMock
                .Setup(x => x.Execute(request))
                .Returns(updatedChildren);

            // Act
            var result = _sut.Add_Child(request);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Enter_Child_Details");
        }
        [Test]
        public async Task Remove_Child_Should_Execute_UseCase_And_Redirect()
        {
            // Arrange
            var request = _fixture.Create<Children>();
            var expectedChildren = new Children
            {
                ChildList = new List<Child> { _fixture.Create<Child>() }
            };
            const int index = 1;

            _removeChildUseCaseMock
                .Setup(x => x.Execute(It.IsAny<Children>(), index))
                .ReturnsAsync(expectedChildren);

            // Act
            var result = await _sut.Remove_Child(request, index);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("Enter_Child_Details");

            _removeChildUseCaseMock.Verify(
                x => x.Execute(It.IsAny<Children>(), index),
                Times.Once);

            _sut.TempData["IsChildAddOrRemove"].Should().Be(true);
            var serializedChildren = _sut.TempData["ChildList"] as string;
            serializedChildren.Should().NotBeNull();
            var deserializedChildren = JsonConvert.DeserializeObject<List<Child>>(serializedChildren);
            deserializedChildren.Should().BeEquivalentTo(expectedChildren.ChildList);
        }

        [Test]
        public async Task Remove_Child_When_InvalidIndex_Should_Throw_Exception()
        {
            // Arrange
            var request = _fixture.Create<Children>();
            const int invalidIndex = 999;
            _removeChildUseCaseMock
                .Setup(x => x.Execute(request, invalidIndex))
                .ThrowsAsync(new ArgumentOutOfRangeException());

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await _sut.Remove_Child(request, invalidIndex));

            // Additional assertions if needed
            exception.Should().NotBeNull();
        }

        [Test]
        public void Check_Answers_Get_Should_Return_View()
        {
            // Act
            var result = _sut.Check_Answers();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Check_Answers");
        }

        [Test]
        public async Task Check_Answers_Post_Should_Submit_And_RedirectTo_AppealsRegistered()
        {
            // Arrange
            var request = _fixture.Create<FsmApplication>();
            var userId = "test-user-id";
            var lastResponse = new ApplicationSaveItemResponse
            {
                Data = new ApplicationResponse { Status = "NotEntitled" }
            };

            _createUserUseCaseMock
                .Setup(x => x.Execute(It.IsAny<IEnumerable<Claim>>()))
                .ReturnsAsync(userId);

            _submitApplicationUseCaseMock
                .Setup(x => x.Execute(request, userId, It.IsAny<string>()))
                .ReturnsAsync(new List<ApplicationSaveItemResponse>());

            // Act
            var result = await _sut.Check_Answers(request);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("AppealsRegistered");
        }


        [Test]
        public async Task Check_Answers_Post_Should_Submit_And_RedirectTo_ApplicationsRegistered()
        {
            // Arrange
            var request = _fixture.Create<FsmApplication>();
            var userId = "test-user-id";
            var viewModel = _fixture.Create<List<ApplicationSaveItemResponse>>();
            viewModel.First().Data = new ApplicationResponse { Status = "Entitled" };

            _createUserUseCaseMock
                .Setup(x => x.Execute(It.IsAny<IEnumerable<Claim>>()))
                .ReturnsAsync(userId);

            _submitApplicationUseCaseMock
                .Setup(x => x.Execute(request, userId, It.IsAny<string>()))
                .ReturnsAsync(viewModel);

            // Act
            var result = await _sut.Check_Answers(request);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("ApplicationsRegistered");
        }

        [Test]
        public async Task Check_Answers_Post_With_Invalid_Application_Should_ThrowException()
        {
            // Arrange
            var request = new FsmApplication();
            var userId = "test-user-id";

            _createUserUseCaseMock
                .Setup(x => x.Execute(It.IsAny<IEnumerable<Claim>>()))
                .ReturnsAsync(userId);

            _submitApplicationUseCaseMock
                .Setup(x => x.Execute(request, userId, It.IsAny<string>()))
                .ThrowsAsync(new NullReferenceException("Invalid request"));

            // Act & Assert
            try
            {
                await _sut.Check_Answers(request);
                Assert.Fail("Expected NullReferenceException was not thrown");
            }
            catch (NullReferenceException ex)
            {
                ex.Message.Should().Be("Invalid request");
            }

            _createUserUseCaseMock.Verify(
                x => x.Execute(It.IsAny<IEnumerable<Claim>>()),
                Times.Once);

            _submitApplicationUseCaseMock.Verify(
                x => x.Execute(request, userId, It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public void ApplicationsRegistered_Should_Process_And_Return_View()
        {
            // Arrange
            var expectedViewModel = _fixture.Create<List<ApplicationSaveItemResponse>>();
            _sut.TempData["FsmApplicationResponse"] = JsonConvert.SerializeObject(expectedViewModel);

            // Act
            var result = _sut.ApplicationsRegistered();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("ApplicationsRegistered");
            viewResult.Model.Should().BeEquivalentTo(expectedViewModel);
        }

        [Test]
        public void ChangeChildDetails_Should_Process_And_Return_View()
        {
            // Arrange
            var childIndex = 0;
            var fsmApplication = _fixture.Create<FsmApplication>();
            var expectedChildren = fsmApplication.Children; // Use the same Children instance

            _sut.TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);

            _changeChildDetailsUseCaseMock
                .Setup(x => x.Execute(It.IsAny<string>()))
                .Returns(expectedChildren);

            // Act
            var result = _sut.ChangeChildDetails(childIndex);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Enter_Child_Details");


            var resultModel = viewResult.Model as Children;
            resultModel.Should().NotBeNull();
            resultModel.ChildList.Should().NotBeNull();
            resultModel.ChildList.Count.Should().Be(expectedChildren.ChildList.Count);

            _sut.TempData["IsRedirect"].Should().Be(true);

            _changeChildDetailsUseCaseMock.Verify(
                x => x.Execute(It.IsAny<string>()),
                Times.Once);
        }

        [TestCase("eligible", "Outcome/Eligible")]
        [TestCase("notEligible", "Outcome/Not_Eligible")]
        [TestCase("parentNotFound", "Outcome/Not_Found")]
        [TestCase("queuedForProcessing", "Loader")]
        [TestCase("DwpError", "Outcome/Technical_Error")]
        public async Task Given_Poll_Status_With_Valid_Status_Returns_Correct_View(string status, string expectedView)
        {
            // Arrange

            var statusValue = _fixture.Build<StatusValue>()
                .With(x => x.Status, status.ToString())
                .Create();

            var checkEligibilityResponse = _fixture.Build<CheckEligibilityResponse>()
                .With(x => x.Data, statusValue)
                .Create();

            _httpContext.Setup(ctx => ctx.Session).Returns(_sessionMock.Object);
            _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "12345"),
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@example.com"),
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "John"),
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "Doe"),
                new Claim("OrganisationCategoryName", CheckYourEligibility.Admin.Models.Constants.CategoryTypeLA)
            }));

            var responseJson = JsonConvert.SerializeObject(checkEligibilityResponse);
            _tempData["Response"] = responseJson;
            _getCheckStatusUseCaseMock
                .Setup(x => x.Execute(responseJson, _sessionMock.Object))
                .ReturnsAsync(status);

            // Act
            var result = await _sut.Loader();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be(expectedView);
            _getCheckStatusUseCaseMock.Verify(x => x.Execute(responseJson, _sessionMock.Object), Times.Once);
        }

        [Test]
        public async Task Given_Poll_Status_When_Response_Is_Null_Returns_Error_Status()
        {
            // Arrange
            _tempData["Response"] = null;

            // Act
            var result = await _sut.Loader();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Outcome/Technical_Error");
        }

        [Test]
        public async Task Given_Poll_Status_When_Status_Is_Processing_Returns_Processing()
        {
            // Arrange
            var response = new CheckEligibilityResponse
            {
                Data = new StatusValue { Status = "queuedForProcessing" },
            };
            _tempData["Response"] = JsonConvert.SerializeObject(response);

            _getCheckStatusUseCaseMock.Setup(x => x.Execute(It.IsAny<string>(), _sessionMock.Object)).ReturnsAsync("queuedForProcessing");

            // Act
            var result = await _sut.Loader();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be("Loader");
        }




    }
}
