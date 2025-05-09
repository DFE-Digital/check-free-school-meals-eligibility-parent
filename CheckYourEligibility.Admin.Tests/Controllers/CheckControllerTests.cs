using System.Security.Claims;
using AutoFixture;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Controllers;
using CheckYourEligibility.Admin.Gateways;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.UseCases;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Child = CheckYourEligibility.Admin.Models.Child;

namespace CheckYourEligibility.Admin.Tests.Controllers;

[TestFixture]
public class CheckControllerTests : TestBase
{
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
        _blobStorageGateway = new Mock<IBlobStorageGateway>();
        _uploadEvidenceFileUseCaseMock = new Mock<IUploadEvidenceFileUseCase>();
        _sendNotificationsUseCaseMock = new Mock<ISendNotificationUseCase>();
        _deleteEvidenceFileUseCaseMock = new Mock<IDeleteEvidenceFileUseCase>();

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
            _validateParentDetailsUseCaseMock.Object,
            _uploadEvidenceFileUseCaseMock.Object,
            _sendNotificationsUseCaseMock.Object,
            _deleteEvidenceFileUseCaseMock.Object
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
    private Mock<IBlobStorageGateway> _blobStorageGateway;
    private Mock<IUploadEvidenceFileUseCase> _uploadEvidenceFileUseCaseMock;
    private Mock<ISendNotificationUseCase> _sendNotificationsUseCaseMock;
    private Mock<IDeleteEvidenceFileUseCase> _deleteEvidenceFileUseCaseMock;

    // Legacy service mocks - keep temporarily during transition
    private Mock<IParentGateway> _parentGatewayMock;
    private Mock<ICheckGateway> _checkGatewayMock;

    // System under test
    private CheckController _sut;

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
    public async Task Enter_Details_Get_When_ErrorsInTempData_Should_AddToModelState()
    {
        // Arrange
        var expectedParent = _fixture.Create<ParentGuardian>();
        var expectedErrors = new Dictionary<string, List<string>>
        {
            { "TestError", new List<string> { "Error message 1", "Error message 2" } }
        };

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
        
        // Verify ModelState contains the expected errors
        _sut.ModelState.ErrorCount.Should().Be(2);
        _sut.ModelState["TestError"].Errors.Count.Should().Be(2);
        _sut.ModelState["TestError"].Errors[0].ErrorMessage.Should().Be("Error message 1");
        _sut.ModelState["TestError"].Errors[1].ErrorMessage.Should().Be("Error message 2");
    }

    [Test]
    [TestCase(0, "AB123456C", null)] // NinSelected = 0
    [TestCase(1, null, "2407001")] // AsrnSelected = 1
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
    public async Task Enter_Details_Post_When_ValidationPasses_With_AsrnSelected_Should_ProcessAndRedirectToLoader()
    {
        // Arrange
        var request = _fixture.Create<ParentGuardian>();
        request.NationalInsuranceNumber = null;
        request.NationalAsylumSeekerServiceNumber = "2407001";
        request.NinAsrSelection = ParentGuardian.NinAsrSelect.AsrnSelected;
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
        
        // Verify that TempData entries are removed
        _sut.TempData.Keys.Should().NotContain("FsmApplication");
        _sut.TempData.Keys.Should().NotContain("FsmEvidence");
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
        var expectedResult = new Children { ChildList = new List<Child> { new() } };

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
    public void Enter_Child_Details_Post_When_Valid_Should_Process_And_Redirect()
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
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult.ActionName.Should().Be("UploadEvidence");

        _processChildDetailsUseCaseMock.Verify(
            x => x.Execute(request, _sut.HttpContext.Session),
            Times.Once);
            
        // Verify TempData has the FSM application
        _sut.TempData["FsmApplication"].Should().NotBeNull();
    }
    
    [Test]
    public void Enter_Child_Details_Post_When_IsRedirect_True_Should_Return_View()
    {
        // Arrange
        var request = _fixture.Create<Children>();
        _sut.TempData["FsmApplication"] = JsonConvert.SerializeObject(_fixture.Create<FsmApplication>());
        _sut.TempData["IsRedirect"] = true;

        // Act
        var result = _sut.Enter_Child_Details(request);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Enter_Child_Details");
        viewResult.Model.Should().BeEquivalentTo(request);
    }
    
    [Test]
    public void Enter_Child_Details_Post_When_ModelStateInvalid_Should_Return_View()
    {
        // Arrange
        var request = _fixture.Create<Children>();
        _sut.ModelState.AddModelError("test", "test error");

        // Act
        var result = _sut.Enter_Child_Details(request);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Enter_Child_Details");
        viewResult.Model.Should().BeEquivalentTo(request);
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

    //[Test]
    //public async Task Check_Answers_Post_Should_Submit_And_RedirectTo_AppealsRegistered()
    //{
    //    // Arrange
    //    var request = _fixture.Create<FsmApplication>();
    //    var userId = "test-user-id";
    //    var lastResponse = new ApplicationSaveItemResponse
    //    {
    //        Data = new ApplicationResponse { Status = "NotEntitled" }
    //    };

    //    _createUserUseCaseMock
    //        .Setup(x => x.Execute(It.IsAny<IEnumerable<Claim>>()))
    //        .ReturnsAsync(userId);

    //    _submitApplicationUseCaseMock
    //        .Setup(x => x.Execute(request, userId, It.IsAny<string>()))
    //        .ReturnsAsync(new List<ApplicationSaveItemResponse>());

    //    // Act
    //    var result = await _sut.Check_Answers_Post(request);

    //    // Assert
    //    result.Should().BeOfType<RedirectToActionResult>();
    //    var redirectResult = result as RedirectToActionResult;
    //    redirectResult.ActionName.Should().Be("AppealsRegistered");
    //}


    //[Test]
    //public async Task Check_Answers_Post_Should_Submit_And_RedirectTo_ApplicationsRegistered()
    //{
    //    // Arrange
    //    var request = _fixture.Create<FsmApplication>();
    //    var userId = "test-user-id";
    //    var viewModel = _fixture.Create<List<ApplicationSaveItemResponse>>();
    //    viewModel.First().Data = new ApplicationResponse { Status = "Entitled" };

    //    _createUserUseCaseMock
    //        .Setup(x => x.Execute(It.IsAny<IEnumerable<Claim>>()))
    //        .ReturnsAsync(userId);

    //    _submitApplicationUseCaseMock
    //        .Setup(x => x.Execute(request, userId, It.IsAny<string>()))
    //        .ReturnsAsync(viewModel);

    //    // Act
    //    var result = await _sut.Check_Answers_Post(request);

    //    // Assert
    //    result.Should().BeOfType<RedirectToActionResult>();
    //    var redirectResult = result as RedirectToActionResult;
    //    redirectResult.ActionName.Should().Be("ApplicationsRegistered");
    //}

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
            await _sut.Check_Answers_Post(request);
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
        var expectedChildren = new Children
        {
            ChildList = new List<Child> { _fixture.Create<Child>() }
        };

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

        _sut.TempData["IsRedirect"].Should().Be(true);
        _sut.TempData["FsmEvidence"].Should().NotBeNull();

        _changeChildDetailsUseCaseMock.Verify(
            x => x.Execute(It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public void AppealsRegistered_Should_Process_And_Return_View()
    {
        // Arrange
        var expectedViewModel = _fixture.Create<List<ApplicationSaveItemResponse>>();
        _sut.TempData["FsmApplicationResponse"] = JsonConvert.SerializeObject(expectedViewModel);

        // Act
        var result = _sut.AppealsRegistered();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("AppealsRegistered");
        viewResult.Model.Should().BeEquivalentTo(expectedViewModel);
    }

    [TestCase("eligible", "Outcome/Eligible")]
    [TestCase("notEligible", "Outcome/Not_Eligible")]
    [TestCase("parentNotFound", "Outcome/Not_Found")]
    [TestCase("queuedForProcessing", "Loader")]
    [TestCase("error", "Outcome/Technical_Error")]
    public async Task Given_Poll_Status_With_Valid_Status_Returns_Correct_View(string status, string expectedView)
    {
        // Arrange

        var statusValue = _fixture.Build<StatusValue>()
            .With(x => x.Status, status)
            .Create();

        var checkEligibilityResponse = _fixture.Build<CheckEligibilityResponse>()
            .With(x => x.Data, statusValue)
            .Create();

        _httpContext.Setup(ctx => ctx.Session).Returns(_sessionMock.Object);
        _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "12345"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@example.com"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "John"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "Doe"),
            new("OrganisationCategoryName", Constants.CategoryTypeLA)
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
            Data = new StatusValue { Status = "queuedForProcessing" }
        };
        _tempData["Response"] = JsonConvert.SerializeObject(response);

        _getCheckStatusUseCaseMock.Setup(x => x.Execute(It.IsAny<string>(), _sessionMock.Object))
            .ReturnsAsync("queuedForProcessing");

        // Act
        var result = await _sut.Loader();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Loader");
    }

    [Test]
    public async Task Consent_Declaration_Should_Return_View()
    {
        // Act
        var result = await _sut.Consent_Declaration();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().BeNull(); // Uses default view name
    }

    [Test]
    [TestCase("checked", "Enter_Details")]
    [TestCase("notchecked", "Consent_Declaration")]
    public async Task Consent_Declaration_Approval_Should_Redirect_Based_On_Consent(string consent, string expectedAction)
    {
        // Act
        var result = await _sut.Consent_Declaration_Approval(consent);

        // Assert
        if (consent == "checked")
        {
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be(expectedAction);
        }
        else
        {
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().Be(expectedAction);
            viewResult.Model.Should().Be(true);
        }
    }

    [Test]
    public void UploadEvidence_Get_Should_Return_View()
    {
        // Arrange
        var fsmApplication = _fixture.Create<FsmApplication>();
        _sut.TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);

        // Act
        var result = _sut.UploadEvidence();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeEquivalentTo(fsmApplication, options => 
            options.Excluding(x => x.EvidenceFiles));
    }

    [Test]
    public void UploadEvidence_Get_Should_Return_Empty_View_When_No_TempData()
    {
        // Arrange
        _sut.TempData["FsmApplication"] = null;

        // Act
        var result = _sut.UploadEvidence();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeNull();
    }

    [Test]
    public void RemoveEvidenceItem_Should_Remove_Item_And_Redirect()
    {
        // Arrange
        var fileName = "test-file.pdf";
        var redirectAction = "UploadEvidence";
        
        var fsmApplication = _fixture.Create<FsmApplication>();
        var evidenceFile = new EvidenceFile 
        { 
            FileName = fileName, 
            FileType = "application/pdf",
            StorageAccountReference = "test-reference"
        };
        
        fsmApplication.Evidence.EvidenceList.Add(evidenceFile);
        
        _sut.TempData["FsmApplication"] = JsonConvert.SerializeObject(fsmApplication);
        
        // Act
        var result = _sut.RemoveEvidenceItem(fileName, redirectAction);
        
        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult.ActionName.Should().Be(redirectAction);
        
        _deleteEvidenceFileUseCaseMock.Verify(
            x => x.Execute(evidenceFile.StorageAccountReference, It.IsAny<string>()), 
            Times.Once);
            
        // The item should be removed from temp data
        var updatedApp = JsonConvert.DeserializeObject<FsmApplication>(_sut.TempData["FsmApplication"].ToString());
        updatedApp.Evidence.EvidenceList.Should().NotContain(x => x.FileName == fileName);
    }

    [Test]
    public async Task UploadEvidence_Post_Should_Upload_Files_And_Redirect()
    {
        // Arrange
        var request = _fixture.Create<FsmApplication>();
        
        // Create a mock file
        var fileMock = new Mock<IFormFile>();
        var fileName = "test.pdf";
        var fileContent = "Test file content";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(fileContent);
        writer.Flush();
        ms.Position = 0;
        
        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(ms.Length);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");
        
        request.EvidenceFiles = new List<IFormFile> { fileMock.Object };
        
        _uploadEvidenceFileUseCaseMock
            .Setup(x => x.Execute(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync("blob-url");
        
        // Act
        var result = await _sut.UploadEvidence(request);
        
        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult.ActionName.Should().Be("Check_Answers");
        
        _uploadEvidenceFileUseCaseMock.Verify(
            x => x.Execute(It.IsAny<IFormFile>(), It.IsAny<string>()), 
            Times.Once);
    }

    [Test]
    public async Task UploadEvidence_Post_When_Existing_Evidence_In_TempData_Should_Preserve_It()
    {
        // Arrange
        var request = new FsmApplication(); // Use a new instance instead of AutoFixture
        request.EvidenceFiles = new List<IFormFile>();
        
        // Create existing evidence in TempData
        var existingEvidence = new Evidence
        {
            EvidenceList = new List<EvidenceFile>
            {
                new EvidenceFile
                {
                    FileName = "existing-file.pdf",
                    FileType = "application/pdf",
                    StorageAccountReference = "existing-blob-url"
                }
            }
        };
        
        var existingApplication = new FsmApplication
        {
            ParentFirstName = "Existing",
            ParentLastName = "Parent",
            Evidence = existingEvidence
        };
        
        _sut.TempData["FsmApplication"] = JsonConvert.SerializeObject(existingApplication);
        
        // Act
        var result = await _sut.UploadEvidence(request);
        
        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult.ActionName.Should().Be("Check_Answers");
        
        // Verify the existing evidence was preserved
        var savedApp = JsonConvert.DeserializeObject<FsmApplication>(_sut.TempData["FsmApplication"].ToString());
        savedApp.Evidence.EvidenceList.Should().HaveCount(1);
        savedApp.Evidence.EvidenceList.First().FileName.Should().Be("existing-file.pdf");
    }

    [Test]
    public async Task UploadEvidence_Post_When_UploadFails_Should_AddModelError()
    {
        // Arrange
        var request = _fixture.Create<FsmApplication>();
        
        // Create a mock file that will fail to upload
        var fileMock = new Mock<IFormFile>();
        var fileName = "error-file.pdf";
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");
        
        request.EvidenceFiles = new List<IFormFile> { fileMock.Object };
        
        // Make the upload throw an exception
        _uploadEvidenceFileUseCaseMock
            .Setup(x => x.Execute(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Upload failed"));
        
        // Act
        var result = await _sut.UploadEvidence(request);
        
        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("UploadEvidence");
        
        // Verify model state has errors
        _sut.ModelState.IsValid.Should().BeFalse();
        _sut.ModelState.Should().ContainKey("EvidenceFiles");
        
        _uploadEvidenceFileUseCaseMock.Verify(
            x => x.Execute(It.IsAny<IFormFile>(), It.IsAny<string>()), 
            Times.Once);
    }
    
    [Test]
    public async Task UploadEvidence_Post_When_Multiple_Files_Should_Upload_All()
    {
        // Arrange
        var request = new FsmApplication(); // Use a new instance instead of AutoFixture
        request.Evidence = new Evidence { EvidenceList = new List<EvidenceFile>() }; // Start with empty evidence list
        
        // Create multiple mock files
        var fileMock1 = new Mock<IFormFile>();
        fileMock1.Setup(f => f.FileName).Returns("file1.pdf");
        fileMock1.Setup(f => f.Length).Returns(100);
        fileMock1.Setup(f => f.ContentType).Returns("application/pdf");
        
        var fileMock2 = new Mock<IFormFile>();
        fileMock2.Setup(f => f.FileName).Returns("file2.jpg");
        fileMock2.Setup(f => f.Length).Returns(200);
        fileMock2.Setup(f => f.ContentType).Returns("image/jpeg");
        
        request.EvidenceFiles = new List<IFormFile> { fileMock1.Object, fileMock2.Object };
        
        _uploadEvidenceFileUseCaseMock
            .Setup(x => x.Execute(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync("blob-url");
        
        // Act
        var result = await _sut.UploadEvidence(request);
        
        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        
        // Verify both files were uploaded
        _uploadEvidenceFileUseCaseMock.Verify(
            x => x.Execute(It.IsAny<IFormFile>(), It.IsAny<string>()), 
            Times.Exactly(2));
        
        // Check that the temp data has both evidence files
        var savedApp = JsonConvert.DeserializeObject<FsmApplication>(_sut.TempData["FsmApplication"].ToString());
        savedApp.Evidence.EvidenceList.Should().HaveCount(2);
    }

    [Test] 
    public void ContinueWithoutMoreFiles_Should_Save_Application_And_Redirect()
    {
        // Arrange
        var request = _fixture.Create<FsmApplication>();
        
        // Act
        var result = _sut.ContinueWithoutMoreFiles(request);
        
        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult.ActionName.Should().Be("Check_Answers");
        
        // Verify TempData was set
        _sut.TempData["FsmApplication"].Should().NotBeNull();
        var savedApp = JsonConvert.DeserializeObject<FsmApplication>(_sut.TempData["FsmApplication"].ToString());
        
        // EvidenceFiles has JsonIgnore attribute, so it won't be included in serialization
        // Use BeEquivalentTo with config to exclude EvidenceFiles from comparison
        savedApp.Should().BeEquivalentTo(request, options => 
            options.Excluding(x => x.EvidenceFiles));
    }
}