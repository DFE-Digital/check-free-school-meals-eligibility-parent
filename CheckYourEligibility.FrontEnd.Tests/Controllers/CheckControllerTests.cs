using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using CheckYourEligibility.FrontEnd.Boundary.Requests;
using CheckYourEligibility.FrontEnd.Boundary.Responses;
using CheckYourEligibility.FrontEnd.Controllers;
using CheckYourEligibility.FrontEnd.Domain.Enums;
using CheckYourEligibility.FrontEnd.Gateways.Interfaces;
using CheckYourEligibility.FrontEnd.Models;
using CheckYourEligibility.FrontEnd.UseCases;
using FluentAssertions;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Child = CheckYourEligibility.FrontEnd.Models.Child;
using ChildsSchool = CheckYourEligibility.FrontEnd.Models.School;
using School = CheckYourEligibility.FrontEnd.Boundary.Responses.Establishment;

namespace CheckYourEligibility.FrontEnd.Tests.Controllers;

[TestFixture]
public class CheckControllerTests
{
    [SetUp]
    public void SetUp()
    {
        try
        {
            SetUpInitialMocks();
            SetUpTestData();
            SetUpServiceMocks();
            SetUpSessionData();
            SetUpHTTPContext();
            SetUpTempData();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Setup failed: {ex}");
            throw;
        }
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            _sut?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Teardown failed: {ex}");
        }
        finally
        {
            _sut = null;
        }
    }

    // mocks
    private ILogger<CheckController> _loggerMock;
    private Mock<IParentGateway> _parentGatewayMock;
    private Mock<ICheckGateway> _checkGatewayMock;
    private Mock<ISession> _sessionMock;
    private Mock<HttpContext> _httpContext;
    private Mock<IConfiguration> _configMock;
    private Mock<ISearchSchoolsUseCase> _searchSchoolsUseCaseMock;
    private Mock<ICreateUserUseCase> _createUserUseCaseMock;
    private Mock<ILoadParentDetailsUseCase> _loadParentDetailsUseCaseMock;
    private Mock<IPerformEligibilityCheckUseCase> _performEligibilityCheckUseCaseMock;
    private Mock<IGetCheckStatusUseCase> _getCheckStatusUseCaseMock;
    private Mock<ISignInUseCase> _signInUseCaseMock;
    private Mock<IEnterChildDetailsUseCase> _enterChildDetailsUseCaseMock;
    private Mock<IProcessChildDetailsUseCase> _processChildDetailsUseCaseMock;
    private Mock<IAddChildUseCase> _addChildUseCaseMock;
    private Mock<IRemoveChildUseCase> _removeChildUseCaseMock;
    private Mock<ISubmitApplicationUseCase> _submitApplicationUseCaseMock;
    private Mock<IChangeChildDetailsUseCase> _changeChildDetailsUseCaseMock;
    private Mock<ISendNotificationUseCase> _sendNotificationUseCaseMock;
    private Mock<IUploadEvidenceFileUseCase> _uploadEvidenceFileUseCaseMock;
    private Mock<IDeleteEvidenceFileUseCase> _deleteEvidenceFileUseCaseMock;
    private Mock<IValidateEvidenceFileUseCase> _validateEvidenceFileUseCaseMock;

    // check eligibility responses
    private CheckEligibilityResponse _eligibilityResponse;
    private CheckEligibilityStatusResponse _eligibilityStatusResponse;
    private EstablishmentSearchResponse _schoolSearchResponse;
    private ApplicationSaveItemResponse _applicationSaveItemResponse;

    // test data entities
    private FsmApplication _fsmApplication;
    private ChildsSchool[] _schools;
    private Parent _parent;
    private Children _children;
    private Children _defaultChildren;

    // system under test
    private CheckController _sut;

    private void SetUpInitialMocks()
    {
        // Initialize service mocks
        _configMock = new Mock<IConfiguration>();
        _parentGatewayMock = new Mock<IParentGateway>();
        _checkGatewayMock = new Mock<ICheckGateway>();
        _loggerMock = Mock.Of<ILogger<CheckController>>();
        _searchSchoolsUseCaseMock = new Mock<ISearchSchoolsUseCase>();
        _createUserUseCaseMock = new Mock<ICreateUserUseCase>();
        _loadParentDetailsUseCaseMock = new Mock<ILoadParentDetailsUseCase>();
        _performEligibilityCheckUseCaseMock = new Mock<IPerformEligibilityCheckUseCase>();
        _getCheckStatusUseCaseMock = new Mock<IGetCheckStatusUseCase>();
        _signInUseCaseMock = new Mock<ISignInUseCase>();
        _enterChildDetailsUseCaseMock = new Mock<IEnterChildDetailsUseCase>();
        _processChildDetailsUseCaseMock = new Mock<IProcessChildDetailsUseCase>();
        _addChildUseCaseMock = new Mock<IAddChildUseCase>();
        _removeChildUseCaseMock = new Mock<IRemoveChildUseCase>();
        _submitApplicationUseCaseMock = new Mock<ISubmitApplicationUseCase>();
        _changeChildDetailsUseCaseMock = new Mock<IChangeChildDetailsUseCase>();
        _sendNotificationUseCaseMock = new Mock<ISendNotificationUseCase>();
        _uploadEvidenceFileUseCaseMock = new Mock<IUploadEvidenceFileUseCase>();
        _deleteEvidenceFileUseCaseMock = new Mock<IDeleteEvidenceFileUseCase>();
        _validateEvidenceFileUseCaseMock = new Mock<IValidateEvidenceFileUseCase>();

        // Create the controller with mocked dependencies
        _sut = new CheckController(
            _loggerMock,
            _parentGatewayMock.Object,
            _checkGatewayMock.Object,
            _configMock.Object,
            _searchSchoolsUseCaseMock.Object,
            _loadParentDetailsUseCaseMock.Object,
            _createUserUseCaseMock.Object,
            _performEligibilityCheckUseCaseMock.Object,
            _getCheckStatusUseCaseMock.Object,
            _signInUseCaseMock.Object,
            _enterChildDetailsUseCaseMock.Object,
            _processChildDetailsUseCaseMock.Object,
            _addChildUseCaseMock.Object,
            _removeChildUseCaseMock.Object,
            _submitApplicationUseCaseMock.Object,
            _changeChildDetailsUseCaseMock.Object,
            _sendNotificationUseCaseMock.Object,
            _uploadEvidenceFileUseCaseMock.Object,
            _deleteEvidenceFileUseCaseMock.Object,
            _validateEvidenceFileUseCaseMock.Object
        );
    }

    private void SetUpTestData()
    {
        _defaultChildren = new Children
        {
            ChildList = new List<Child> { new() }
        };

        _schools = new[]
        {
            new ChildsSchool
            {
                Name = "Springfield Elementary",
                LA = "Springfield",
                Postcode = "SP1 3LE",
                URN = "100021"
            },
            new ChildsSchool
            {
                Name = "Springfield Nursery",
                LA = "Springfield",
                Postcode = "SP1 3NU",
                URN = "100011"
            }
        };

        _parent = new Parent
        {
            FirstName = "Homer",
            LastName = "Simpson",
            Day = "1",
            Month = "1",
            Year = "1990",
            NationalInsuranceNumber = "AB123456C",
            NationalAsylumSeekerServiceNumber = null,
            IsNinoSelected = true,
            IsNassSelected = null
        };

        _children = new Children
        {
            ChildList =
            [
                new Child
                {
                    FirstName = "Bart",
                    LastName = "Simpson",
                    Day = "1",
                    Month = "1",
                    Year = "2015",
                    School = _schools[0]
                },
                new Child
                {
                    FirstName = "Lisa",
                    LastName = "Simpson",
                    Day = "1",
                    Month = "1",
                    Year = "2018",
                    School = _schools[0]
                },
                new Child
                {
                    FirstName = "Maggie",
                    LastName = "Simpson",
                    Day = "1",
                    Month = "1",
                    Year = "2020",
                    School = _schools[1]
                }
            ]
        };

        _fsmApplication = new FsmApplication
        {
            ParentFirstName = _parent.FirstName,
            ParentLastName = _parent.LastName,
            ParentDateOfBirth = "01/01/1990",
            ParentNino = _parent.NationalInsuranceNumber,
            ParentNass = _parent.NationalAsylumSeekerServiceNumber,
            Children = _children
        };

        _eligibilityResponse = new CheckEligibilityResponse
        {
            Data = new StatusValue
            {
                Status = "queuedForProcessing"
            },
            Links = new CheckEligibilityResponseLinks
            {
                Get_EligibilityCheck = "",
                Get_EligibilityCheckStatus = "",
                Put_EligibilityCheckProcess = ""
            }
        };

        _eligibilityStatusResponse = new CheckEligibilityStatusResponse
        {
            Data = new StatusValue
            {
                Status = "eligible"
            }
        };

        _schoolSearchResponse = new EstablishmentSearchResponse
        {
            Data =
            [
                new School
                {
                    Id = 100,
                    County = "Test County",
                    Distance = 0.05d,
                    La = "Test LA",
                    Locality = "Test Locality",
                    Name = "Test School",
                    Postcode = "TE55 5ST",
                    Street = "Test Street",
                    Town = "Test Town"
                }
            ]
        };

        _applicationSaveItemResponse = new ApplicationSaveItemResponse
        {
            Data = new ApplicationResponse
            {
                ParentFirstName = _fsmApplication.ParentFirstName,
                ParentLastName = _fsmApplication.ParentLastName,
                ParentDateOfBirth = _fsmApplication.ParentDateOfBirth,
                ParentNationalInsuranceNumber = _fsmApplication.ParentNino,
                ChildFirstName = _fsmApplication.Children.ChildList[0].FirstName,
                ChildLastName = _fsmApplication.Children.ChildList[0].LastName,
                ChildDateOfBirth = new DateOnly(int.Parse(_fsmApplication.Children.ChildList[0].Year),
                    int.Parse(_fsmApplication.Children.ChildList[0].Month),
                    int.Parse(_fsmApplication.Children.ChildList[0].Day)).ToString("dd/MM/yyyy"),
                ParentNationalAsylumSeekerServiceNumber = _fsmApplication.ParentNass,
                Id = "",
                Establishment = new ApplicationResponse.ApplicationEstablishment
                {
                    Id = 10002,
                    LocalAuthority = new ApplicationResponse.ApplicationEstablishment.EstablishmentLocalAuthority
                        { Id = 123 }
                },
                Reference = ""
            },
            Links = new ApplicationResponseLinks
            {
                get_Application = ""
            }
        };
    }

    private void SetUpSessionData()
    {
        _sessionMock = new Mock<ISession>();
        var sessionStorage = new Dictionary<string, byte[]>();

        _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => sessionStorage[key] = value);

        _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) =>
            {
                var result = sessionStorage.TryGetValue(key, out var storedValue);
                value = storedValue;
                return result;
            });
    }

    private void SetUpHTTPContext()
    {
        _httpContext = new Mock<HttpContext>();
        _httpContext.Setup(ctx => ctx.Session).Returns(_sessionMock.Object);
        var controllerContext = new ControllerContext
        {
            HttpContext = _httpContext.Object
        };
        _sut.ControllerContext = controllerContext;
    }

    private void SetUpTempData()
    {
        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        _sut.TempData = tempData;
    }

    private void SetUpServiceMocks()
    {
        // Service mocks
        _checkGatewayMock.Setup(s => s.GetStatus(It.IsAny<CheckEligibilityResponse>()))
            .ReturnsAsync(_eligibilityStatusResponse);

        _checkGatewayMock.Setup(s => s.PostCheck(It.IsAny<CheckEligibilityRequest_Fsm>()))
            .ReturnsAsync(_eligibilityResponse);

        _parentGatewayMock.Setup(s => s.GetSchool(It.IsAny<string>()))
            .ReturnsAsync(_schoolSearchResponse);

        _parentGatewayMock.Setup(s => s.PostApplication_Fsm(It.IsAny<ApplicationRequest>()))
            .ReturnsAsync(_applicationSaveItemResponse);

        _parentGatewayMock.Setup(s => s.CreateUser(It.IsAny<UserCreateRequest>()))
            .ReturnsAsync(new UserSaveItemResponse { Data = "defaultUserId" });

        // Use Case mocks
        _enterChildDetailsUseCaseMock
            .Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<bool?>()))
            .Returns(_defaultChildren);

        _processChildDetailsUseCaseMock
            .Setup(x => x.Execute(It.IsAny<Children>(), It.IsAny<ISession>()))
            .ReturnsAsync(_fsmApplication);

        _addChildUseCaseMock
            .Setup(x => x.Execute(It.IsAny<Children>()))
            .Returns(new Children { ChildList = new List<Child>(_children.ChildList) { new() } });

        _removeChildUseCaseMock
            .Setup(x => x.Execute(It.IsAny<Children>(), It.IsAny<int>()))
            .Returns(_children);

        _submitApplicationUseCaseMock
            .Setup(x => x.Execute(
                It.IsAny<FsmApplication>(),
                It.Is<string>(s => s == CheckEligibilityStatus.eligible.ToString()),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new List<ApplicationSaveItemResponse> { _applicationSaveItemResponse });

        _changeChildDetailsUseCaseMock
            .Setup(x => x.Execute(It.IsAny<string>()))
            .Returns(_children);
    }

    [Test]
    public async Task SearchSchools_WhenQueryIsValid_ShouldReturnSchools()
    {
        // Arrange
        var query = "Test School";
        var schools = new List<Establishment>
        {
            new() { Name = "Test School 1" },
            new() { Name = "Test School 2" }
        };
        _searchSchoolsUseCaseMock.Setup(x => x.Execute(query))
            .ReturnsAsync(schools);

        // Act
        var result = await _sut.SearchSchools(query);

        // Assert
        result.Should().BeOfType<JsonResult>();
        var jsonResult = result as JsonResult;
        jsonResult.Value.Should().BeEquivalentTo(schools);
        _searchSchoolsUseCaseMock.Verify(x => x.Execute(query), Times.Once);
    }

    [Test]
    public async Task SearchSchools_WhenQueryIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var query = "ab";
        _searchSchoolsUseCaseMock.Setup(x => x.Execute(query))
            .ThrowsAsync(new ArgumentException("Query must be at least 3 characters long."));

        // Act
        var result = await _sut.SearchSchools(query);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Value.Should().Be("Query must be at least 3 characters long.");
    }

    [Test]
    public async Task SearchSchools_WhenExceptionOccurs_ShouldReturnBadRequest()
    {
        // Arrange
        var query = "Test School";
        _searchSchoolsUseCaseMock.Setup(x => x.Execute(query))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _sut.SearchSchools(query);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Value.Should().Be("An error occurred while searching for schools.");
    }

    [Test]
    public async Task Given_EnterDetails_When_LoadingPageFromTempData_Should_BeAbleToLoadEnterDetailsPage()
    {
        // Arrange
        _sut.TempData["ParentDetails"] = JsonConvert.SerializeObject(_parent);
        var expectedViewModel = new LoadParentDetailsViewModel { Parent = _parent };
        _loadParentDetailsUseCaseMock
            .Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedViewModel);

        // Act
        var result = await _sut.Enter_Details();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeOfType<Parent>();
        var model = viewResult.Model as Parent;
        model.Should().BeEquivalentTo(_parent);
    }

    [Test]
    public async Task Given_EnterDetails_When_LoadingPage_Should_LoadEnterDetailsPage()
    {
        // Arrange
        var expectedViewModel = new LoadParentDetailsViewModel();
        _loadParentDetailsUseCaseMock
            .Setup(x => x.Execute(null, null))
            .ReturnsAsync(expectedViewModel);

        // Act
        var result = await _sut.Enter_Details();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeNull();
    }

    [Test]
    public async Task Given_EnterDetails_When_ValidDataProvided_Should_RedirectToLoaderPage()
    {
        // Arrange
        _performEligibilityCheckUseCaseMock
            .Setup(x => x.Execute(It.IsAny<Parent>(), It.IsAny<ISession>()))
            .ReturnsAsync((new CheckEligibilityResponse(), "Success"));

        // Act
        var result = await _sut.Enter_Details(_parent);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectToActionResult = result as RedirectToActionResult;
        redirectToActionResult.ActionName.Should().Be("Loader");
    }


    [Test]
    public async Task Given_EnterDetails_When_ValidDataProvided_Should_SetSessionData()
    {
        // Arrange
        var expectedDob = new DateOnly(1990, 01, 01).ToString("yyyy-MM-dd");
        var sessionStorage = new Dictionary<string, byte[]>();

        _sessionMock
            .Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => sessionStorage[key] = value);

        _sessionMock
            .Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) =>
            {
                var result = sessionStorage.TryGetValue(key, out var storedValue);
                value = storedValue;
                return result;
            });

        _performEligibilityCheckUseCaseMock
            .Setup(x => x.Execute(It.IsAny<Parent>(), It.IsAny<ISession>()))
            .Callback<Parent, ISession>((parent, session) =>
            {
                session.Set("ParentFirstName", Encoding.UTF8.GetBytes(parent.FirstName));
                session.Set("ParentLastName", Encoding.UTF8.GetBytes(parent.LastName));
                session.Set("ParentDOB", Encoding.UTF8.GetBytes(expectedDob));
                session.Set("ParentNINO", Encoding.UTF8.GetBytes(parent.NationalInsuranceNumber));
            })
            .ReturnsAsync((new CheckEligibilityResponse(), "Success"));

        // Act
        await _sut.Enter_Details(_parent);

        // Assert
        Encoding.UTF8.GetString(sessionStorage["ParentFirstName"]).Should().Be(_parent.FirstName);
        Encoding.UTF8.GetString(sessionStorage["ParentLastName"]).Should().Be(_parent.LastName);
        Encoding.UTF8.GetString(sessionStorage["ParentDOB"]).Should().Be(expectedDob);
        Encoding.UTF8.GetString(sessionStorage["ParentNINO"]).Should().Be(_parent.NationalInsuranceNumber);
    }


    [Test]
    public async Task Given_EnterDetails_When_ModelStateIsInvalid_Should_ErrorsShouldBeInTempData()
    {
        // Arrange
        var request = _parent;
        _sut.ModelState.AddModelError("SomeErrorKey", "SomeErrorMessage");

        // Act
        var result = await _sut.Enter_Details(request);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Enter_Details");

        _sut.TempData["ParentDetails"].Should().Be(JsonConvert.SerializeObject(request));

        var errors =
            JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(_sut.TempData["Errors"].ToString());
        errors.Should().ContainKey("SomeErrorKey");
        errors["SomeErrorKey"].Should().Contain("SomeErrorMessage");
    }


    [Test]
    public async Task Given_EnterDetails_When_ErrorsExistInTempData_Should_BeAddedToTheModelState()
    {
        // arrange
        _sut.ModelState.AddModelError("SomeErrorKey", "SomeErrorMessage");
        var error = _sut.ModelState.Where(x => x.Value.Errors.Count > 0)
            .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage).ToList());
        _sut.TempData["Errors"] = JsonConvert.SerializeObject(error);

        // act
        var result = await _sut.Enter_Details(_parent);

        // assert
        var redirectResult = result as RedirectToActionResult;
        redirectResult.ActionName.Should().Be("Enter_Details");
    }

    [Test]
    public void Given_Nass_When_LoadingPage_Should_LoadNassPage()
    {
        // Arrange
        var mockParent = new Parent { FirstName = "Test", LastName = "Parent" };
        _sut.TempData["ParentDetails"] = JsonConvert.SerializeObject(mockParent);

        // Act
        var result = _sut.Nass();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeOfType<Parent>();
        var model = viewResult.Model as Parent;
        model.Should().BeEquivalentTo(mockParent);
    }

    [Test]
    public async Task Given_QueuedForProcessingStatus_When_LoaderIsCalled_Should_ReturnLoaderView()
    {
        // Arrange
        var response = new CheckEligibilityResponse
        {
            Data = new StatusValue { Status = "queuedForProcessing" }
        };
        var responseJson = JsonConvert.SerializeObject(response);
        _sut.TempData["Response"] = responseJson;

        _getCheckStatusUseCaseMock
            .Setup(x => x.Execute(responseJson, _sessionMock.Object))
            .ReturnsAsync("queuedForProcessing");

        // Act
        var result = await _sut.Loader();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Loader");
        _getCheckStatusUseCaseMock.Verify(x => x.Execute(responseJson, _sessionMock.Object), Times.Once);
    }

    [Test]
    public async Task Given_Nass_When_ValidDataProvided_Should_RedirectToNass()
    {
        // Arrange
        _parent.IsNinoSelected = false;
        _parent.IsNassSelected = false;
        _parent.NationalInsuranceNumber = null;
        _parent.NationalAsylumSeekerServiceNumber = "202405001";

        // Mock TempData
        var tempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>());
        _sut.TempData = tempData;

        // Mock ModelState
        _sut.ModelState.Clear();

        // Set up session and context properly using the existing session mock from SetUp
        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object; // Use the session mock we already set up
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Mock ProcessParentDetailsUseCase to return expected values
        _performEligibilityCheckUseCaseMock
            .Setup(x => x.Execute(
                It.IsAny<Parent>(),
                It.IsAny<ISession>()))
            .ReturnsAsync((null, "Nass"));

        // Act
        var result = await _sut.Enter_Details(_parent);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var actionResult = (RedirectToActionResult)result;
        actionResult.ActionName.Should().Be("Nass");
    }


    [TestCase("DwpError", "Outcome/Technical_Error")]
    [TestCase("eligible", "Outcome/Eligible")]
    [TestCase("notEligible", "Outcome/Not_Eligible")]
    [TestCase("parentNotFound", "Outcome/Not_Found")]
    [TestCase("queuedForProcessing", "Loader")]
    [TestCase("unknownStatus", "Outcome/Technical_Error")]
    public async Task Given_Loader_When_StatusProvided_Should_ReturnCorrectView(string status, string expectedView)
    {
        // Arrange
        var response = new CheckEligibilityResponse
        {
            Data = new StatusValue { Status = status }
        };
        var responseJson = JsonConvert.SerializeObject(response);
        _sut.TempData["Response"] = responseJson;

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
    public async Task Given_CheckAnswers_When_LoadingPage_Should_LoadCheckAnswersPage()
    {
        // Act
        var result = _sut.Check_Answers();

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeNull();
    }

    [Test]
    public async Task Given_CheckAnswers_When_ValidDataProvided_Should_RedirectToApplicationSentPage()
    {
        // Arrange 

        _sut.ControllerContext.HttpContext.Session.SetString("CheckResult", CheckEligibilityStatus.eligible.ToString());
        _sut.TempData["FsmApplicationResponses"] = JsonConvert.SerializeObject(_applicationSaveItemResponse);
        var finishedConfirmation = "finishedConfirmationChecked";
        // Act
        var result = _sut.Check_Answers_Post(_fsmApplication, finishedConfirmation);

        // Assert
        var actionResult = result.Result as RedirectToActionResult;
        actionResult.ActionName.Should().Be("Application_Sent");
        var applicationSaveItemResponses =
            JsonConvert.DeserializeObject<List<ApplicationSaveItemResponse>>(
                _sut.TempData["FsmApplicationResponses"] as string);
        applicationSaveItemResponses.First(x => x.Data.ParentFirstName == "Homer");
        applicationSaveItemResponses.First(x => x.Data.ChildFirstName == "Bart");
    }

    //[Test]
    //public async Task CheckAnswers_WhenSuccessful_ShouldSendNotifications()
    //{
    //    // Arrange
    //    var userId = "testUserId";
    //    var email = "test@example.com";
    //    var checkResult = CheckEligibilityStatus.eligible.ToString();
    //    var notificationResponse = new NotificationItemResponse
    //    {
    //        Data = new NotificationResponse
    //        {
    //            Status = "delivered"
    //        }
    //    };

    //    // Setup session with TryGetValue instead of extension method
    //    var sessionStorage = new Dictionary<string, byte[]>
    //    {
    //        ["UserId"] = Encoding.UTF8.GetBytes(userId),
    //        ["Email"] = Encoding.UTF8.GetBytes(email),
    //        ["CheckResult"] = Encoding.UTF8.GetBytes(checkResult)
    //    };

    //    _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
    //        .Returns((string key, out byte[] value) =>
    //        {
    //            var result = sessionStorage.TryGetValue(key, out var storedValue);
    //            value = storedValue;
    //            return result;
    //        });

    //    // Setup application response - ensure it has email and reference
    //    _applicationSaveItemResponse.Data.ParentEmail = email;
    //    _applicationSaveItemResponse.Data.Reference = "TEST001";
        
    //    var applicationResponses = new List<ApplicationSaveItemResponse> { _applicationSaveItemResponse };
    //    _submitApplicationUseCaseMock
    //        .Setup(x => x.Execute(_fsmApplication, checkResult, userId, email))
    //        .ReturnsAsync(applicationResponses);

    //    // Setup notification mock
    //    _sendNotificationUseCaseMock
    //        .Setup(x => x.Execute(It.IsAny<NotificationRequest>()))
    //        .ReturnsAsync(notificationResponse);

    //    var finishedConfirmation = "finishedConfirmationChecked";
    //    // Act
    //    var result = _sut.Check_Answers_Post(_fsmApplication, finishedConfirmation);

    //    // Assert
    //    var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
    //    redirectResult.ActionName.Should().Be("Application_Sent");

    //    // Verify notification was sent with correct data - use more flexible verification
    //    _sendNotificationUseCaseMock.Verify(x => x.Execute(It.Is<NotificationRequest>(r => 
    //        r.Data.Email == email && 
    //        r.Data.Type == NotificationType.ParentApplicationSuccessful &&
    //        r.Data.Personalisation.ContainsKey("reference") &&
    //        r.Data.Personalisation.ContainsKey("parentFirstName")
    //    )), Times.Once);
    //}

    //[Test]
    //public async Task CheckAnswers_WhenNotificationFails_ShouldContinueProcessing()
    //{
    //    // Arrange
    //    var userId = "testUserId";
    //    var email = "test@example.com";
    //    var checkResult = CheckEligibilityStatus.eligible.ToString();

    //    // Setup session with TryGetValue instead of extension method
    //    var sessionStorage = new Dictionary<string, byte[]>
    //    {
    //        ["UserId"] = Encoding.UTF8.GetBytes(userId),
    //        ["Email"] = Encoding.UTF8.GetBytes(email),
    //        ["CheckResult"] = Encoding.UTF8.GetBytes(checkResult)
    //    };

    //    _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
    //        .Returns((string key, out byte[] value) =>
    //        {
    //            var result = sessionStorage.TryGetValue(key, out var storedValue);
    //            value = storedValue;
    //            return result;
    //        });

    //    // Setup application response
    //    var applicationResponses = new List<ApplicationSaveItemResponse> { _applicationSaveItemResponse };
    //    _submitApplicationUseCaseMock
    //        .Setup(x => x.Execute(_fsmApplication, checkResult, userId, email))
    //        .ReturnsAsync(applicationResponses);

    //    // Setup notification mock to throw an exception
    //    _sendNotificationUseCaseMock
    //        .Setup(x => x.Execute(It.IsAny<NotificationRequest>()))
    //        .ThrowsAsync(new Exception("Failed to send notification"));

    //    var finishedConfirmation = "finishedConfirmationChecked";
    //    // Act
    //    var result = _sut.Check_Answers_Post(_fsmApplication, finishedConfirmation);

    //    // Assert
    //    var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
    //    redirectResult.ActionName.Should().Be("Application_Sent");

    //    // Verify notification was attempted
    //    _sendNotificationUseCaseMock.Verify(x => x.Execute(It.IsAny<NotificationRequest>()), Times.Once);
        
    //    // Verify the process continued despite notification failure
    //    _sut.TempData.Should().ContainKey("FsmApplicationResponses");
    //}

    //[Test]
    //public async Task CheckAnswers_WithMultipleApplications_ShouldSendNotificationForEach()
    //{
    //    // Arrange
    //    var userId = "testUserId";
    //    var email = "test@example.com";
    //    var checkResult = CheckEligibilityStatus.eligible.ToString();
    //    var notificationResponse = new NotificationItemResponse
    //    {
    //        Data = new NotificationResponse
    //        {
    //            Status = "delivered"
    //        }
    //    };

    //    // Setup session with TryGetValue instead of extension method
    //    var sessionStorage = new Dictionary<string, byte[]>
    //    {
    //        ["UserId"] = Encoding.UTF8.GetBytes(userId),
    //        ["Email"] = Encoding.UTF8.GetBytes(email),
    //        ["CheckResult"] = Encoding.UTF8.GetBytes(checkResult)
    //    };

    //    _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
    //        .Returns((string key, out byte[] value) =>
    //        {
    //            var result = sessionStorage.TryGetValue(key, out var storedValue);
    //            value = storedValue;
    //            return result;
    //        });

    //    // Create multiple application responses
    //    var secondApplication = new ApplicationSaveItemResponse
    //    {
    //        Data = new ApplicationResponse
    //        {
    //            ParentFirstName = _fsmApplication.ParentFirstName,
    //            ParentLastName = _fsmApplication.ParentLastName,
    //            ParentDateOfBirth = _fsmApplication.ParentDateOfBirth,
    //            ParentNationalInsuranceNumber = _fsmApplication.ParentNino,
    //            ChildFirstName = _fsmApplication.Children.ChildList[1].FirstName,
    //            ChildLastName = _fsmApplication.Children.ChildList[1].LastName,
    //            ChildDateOfBirth = new DateOnly(int.Parse(_fsmApplication.Children.ChildList[1].Year),
    //                int.Parse(_fsmApplication.Children.ChildList[1].Month),
    //                int.Parse(_fsmApplication.Children.ChildList[1].Day)).ToString("dd/MM/yyyy"),
    //            ParentNationalAsylumSeekerServiceNumber = _fsmApplication.ParentNass,
    //            Id = "2",
    //            Reference = "REF002",
    //            ParentEmail = email,
    //            Establishment = new ApplicationResponse.ApplicationEstablishment
    //            {
    //                Id = 10002,
    //                LocalAuthority = new ApplicationResponse.ApplicationEstablishment.EstablishmentLocalAuthority
    //                    { Id = 123 }
    //            }
    //        },
    //        Links = new ApplicationResponseLinks
    //        {
    //            get_Application = ""
    //        }
    //    };

    //    // Add email to first application
    //    _applicationSaveItemResponse.Data.ParentEmail = email;
    //    _applicationSaveItemResponse.Data.Reference = "REF001";

    //    var applicationResponses = new List<ApplicationSaveItemResponse> 
    //    { 
    //        _applicationSaveItemResponse,
    //        secondApplication
    //    };

    //    _submitApplicationUseCaseMock
    //        .Setup(x => x.Execute(_fsmApplication, checkResult, userId, email))
    //        .ReturnsAsync(applicationResponses);

    //    // Setup notification mock
    //    _sendNotificationUseCaseMock
    //        .Setup(x => x.Execute(It.IsAny<NotificationRequest>()))
    //        .ReturnsAsync(notificationResponse);

    //    var finishedConfirmation = "finishedConfirmationChecked";
    //    // Act
    //    var result = _sut.Check_Answers_Post(_fsmApplication, finishedConfirmation);

    //    // Assert
    //    var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
    //    redirectResult.ActionName.Should().Be("Application_Sent");

    //    // Verify notifications were sent for each application
    //    _sendNotificationUseCaseMock.Verify(x => x.Execute(It.Is<NotificationRequest>(r => 
    //        r.Data.Email == email && 
    //        r.Data.Type == NotificationType.ParentApplicationSuccessful
    //    )), Times.Exactly(2));
    //}

    [Test]
    public async Task CheckAnswers_ShouldCreateCorrectNotificationPersonalisation()
    {
        // Arrange
        var userId = "testUserId";
        var email = "test@example.com";
        var checkResult = CheckEligibilityStatus.eligible.ToString();
        var reference = "FSM123456";
        
        NotificationRequest capturedRequest = null;

        // Setup session with TryGetValue instead of extension method
        var sessionStorage = new Dictionary<string, byte[]>
        {
            ["UserId"] = Encoding.UTF8.GetBytes(userId),
            ["Email"] = Encoding.UTF8.GetBytes(email),
            ["CheckResult"] = Encoding.UTF8.GetBytes(checkResult)
        };

        _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) =>
            {
                var result = sessionStorage.TryGetValue(key, out var storedValue);
                value = storedValue;
                return result;
            });

        // Add email and reference to application
        _applicationSaveItemResponse.Data.ParentEmail = email;
        _applicationSaveItemResponse.Data.Reference = reference;

        var applicationResponses = new List<ApplicationSaveItemResponse> { _applicationSaveItemResponse };
        
        _submitApplicationUseCaseMock
            .Setup(x => x.Execute(_fsmApplication, checkResult, userId, email))
            .ReturnsAsync(applicationResponses);

        // Capture the notification request
        _sendNotificationUseCaseMock
            .Setup(x => x.Execute(It.IsAny<NotificationRequest>()))
            .Callback<NotificationRequest>(req => capturedRequest = req)
            .ReturnsAsync(new NotificationItemResponse { Data = new NotificationResponse { Status = "sent" } });

        var finishedConfirmation = "finishedConfirmationChecked";
        // Act
        var result = _sut.Check_Answers_Post(_fsmApplication, finishedConfirmation);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest.Data.Email.Should().Be(email);
        capturedRequest.Data.Type.Should().Be(NotificationType.ParentApplicationSuccessful);
        capturedRequest.Data.Personalisation.Should().ContainKey("reference");
        capturedRequest.Data.Personalisation["reference"].Should().Be(reference);
        capturedRequest.Data.Personalisation.Should().ContainKey("parentFirstName");
        capturedRequest.Data.Personalisation["parentFirstName"].Should().Be(_fsmApplication.ParentFirstName);
    }

    [Test]
    public async Task Application_Sent_WhenLoadingPage_ShouldReturnView()
    {
        // Act
        var result = await _sut.Application_Sent();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("Application_Sent");
        viewResult.Model.Should().BeNull();
    }

    [Test]
    public async Task Application_Sent_ShouldClearModelStateAndReturnView()
    {
        // Add some model state errors to verify they get cleared
        _sut.ModelState.AddModelError("test", "test error");

        // Act
        var result = await _sut.Application_Sent();

        // Assert
        _sut.ModelState.ErrorCount.Should().Be(0);
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("Application_Sent");
        viewResult.Model.Should().BeNull();
    }

    [Test]
    public async Task Add_Child_WhenSuccessful_ShouldAddChildAndRedirectToEnterDetails()
    {
        // Arrange
        var request = new Children { ChildList = _children.ChildList };
        var updatedChildren = new Children
        {
            ChildList = new List<Child>(_children.ChildList) { new() }
        };

        _addChildUseCaseMock
            .Setup(x => x.Execute(request))
            .Returns(updatedChildren);

        // Act
        var result = await _sut.Add_Child(request);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Enter_Child_Details");
        _sut.TempData["IsChildAddOrRemove"].Should().Be(true);

        var savedChildren = JsonConvert.DeserializeObject<List<Child>>(_sut.TempData["ChildList"] as string);
        savedChildren.Should().NotBeNull();
        savedChildren.Should().HaveCount(request.ChildList.Count + 1);
        savedChildren.Last().Should().BeEquivalentTo(new Child());

        _addChildUseCaseMock.Verify(x => x.Execute(request), Times.Once);
    }

    [Test]
    public async Task Remove_Child_WhenSuccessful_ShouldRemoveChildAndRedirectToEnterDetails()
    {
        // Arrange
        var request = new Children
        {
            ChildList = new List<Child>
            {
                new() { FirstName = "Child1" },
                new() { FirstName = "Child2" }
            }
        };
        var updatedChildren = new Children
        {
            ChildList = new List<Child> { new() { FirstName = "Child2" } }
        };

        _removeChildUseCaseMock
            .Setup(x => x.Execute(request, 0))
            .Returns(updatedChildren);

        // Act
        var result = await _sut.Remove_Child(request, 0);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Enter_Child_Details");
        _sut.TempData["IsChildAddOrRemove"].Should().Be(true);

        var savedChildren = JsonConvert.DeserializeObject<List<Child>>(_sut.TempData["ChildList"] as string);
        savedChildren.Should().NotBeNull();
        savedChildren.Should().HaveCount(1);
        savedChildren.Should().NotContain(x => x.FirstName == "Child1");

        _removeChildUseCaseMock.Verify(x => x.Execute(request, 0), Times.Once);
    }

    [Test]
    public async Task Remove_Child_WhenInvalidIndex_ShouldRedirectWithError()
    {
        // Arrange
        var request = new Children { ChildList = new List<Child> { new() } };

        _removeChildUseCaseMock
            .Setup(x => x.Execute(request, 1))
            .Throws(new RemoveChildValidationException("Invalid child index"));

        // Act
        var result = await _sut.Remove_Child(request, 1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Enter_Child_Details");
        _sut.TempData["IsChildAddOrRemove"].Should().Be(true);

        // Verify error was added to ModelState
        _sut.ModelState.ErrorCount.Should().Be(1);
        _sut.ModelState.Values.First().Errors.First().ErrorMessage.Should().Be("Invalid child index");
    }

    [Test]
    public async Task Remove_Child_WhenUseCaseThrows_ShouldPropagateException()
    {
        // Arrange
        var request = new Children { ChildList = new List<Child> { new() } };

        _removeChildUseCaseMock
            .Setup(x => x.Execute(request, 0))
            .Throws(new Exception("Test exception"));

        // Act & Assert
        await FluentActions.Invoking(() =>
                _sut.Remove_Child(request, 0))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Test exception");
    }

    [Test]
    public async Task ChangeChildDetails_WhenCalled_ShouldSetTempDataAndReturnView()
    {
        // Arrange
        var children = new Children
        {
            ChildList = new List<Child>
            {
                new() { FirstName = "Test", LastName = "Child" }
            }
        };

        _changeChildDetailsUseCaseMock
            .Setup(x => x.Execute(It.IsAny<string>()))
            .Returns(children);

        var child = 0;
        // Act
        var result = await _sut.ChangeChildDetails(child);

        // Assert
        _sut.TempData["IsRedirect"].Should().Be(true);
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be("Enter_Child_Details");
        viewResult.Model.Should().BeEquivalentTo(children);
    }

    [Test]
    public async Task ChangeChildDetails_WhenUseCaseThrows_ShouldPropagateException()
    {
        // Arrange
        _changeChildDetailsUseCaseMock
            .Setup(x => x.Execute(It.IsAny<string>()))
            .Throws(new Exception("Test exception"));

        // Act & Assert
        var child = 0;
        await FluentActions.Invoking(() =>
                _sut.ChangeChildDetails(child))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Test exception");
    }


    [Test]
    public async Task Enter_Child_Details_When_LoadingPage_Should_LoadEnterChildDetailsPage()
    {
        // Arrange
        _enterChildDetailsUseCaseMock
            .Setup(x => x.Execute(null, null))
            .Returns(_defaultChildren);

        // Act
        var result = await _sut.Enter_Child_Details();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Should().NotBeNull();
        viewResult.Model.Should().NotBeNull();
        viewResult.Model.Should().BeAssignableTo<Children>();
        viewResult.Model.Should().BeEquivalentTo(_defaultChildren);

        // Verify the use case was called with expected parameters
        _enterChildDetailsUseCaseMock.Verify(
            x => x.Execute(null, null),
            Times.Once);
    }

    [Test]
    public async Task Enter_Child_Details_When_IsChildAddOrRemove_Should_PopulateModelFromTempData()
    {
        // Arrange
        var children = new Children
        {
            ChildList = new List<Child>
            {
                new() { FirstName = "TestChild", LastName = "TestLastName" }
            }
        };
        _sut.TempData["IsChildAddOrRemove"] = true;
        _sut.TempData["ChildList"] = JsonConvert.SerializeObject(children.ChildList);

        _enterChildDetailsUseCaseMock
            .Setup(x => x.Execute(
                It.Is<string>(s => s == JsonConvert.SerializeObject(children.ChildList)),
                It.Is<bool?>(b => b == true)))
            .Returns(children);

        // Act
        var result = await _sut.Enter_Child_Details();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<Children>().Subject;
        model.Should().BeEquivalentTo(children);

        _enterChildDetailsUseCaseMock.Verify(
            x => x.Execute(
                It.Is<string>(s => s == JsonConvert.SerializeObject(children.ChildList)),
                It.Is<bool?>(b => b == true)),
            Times.Once);
    }

    [Test]
    public async Task Enter_Child_Details_When_NoTempData_Should_ReturnDefaultModel()
    {
        // Arrange
        _enterChildDetailsUseCaseMock
            .Setup(x => x.Execute(null, null))
            .Returns(new Children { ChildList = new List<Child> { new() } });

        // Act
        var result = await _sut.Enter_Child_Details();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<Children>().Subject;
        model.ChildList.Should().HaveCount(1);
        model.ChildList.First().Should().BeOfType<Child>();
    }

    [Test]
    public async Task Enter_Child_Details_WhenUseCaseThrows_ShouldPropagateException()
    {
        // Arrange
        _enterChildDetailsUseCaseMock
            .Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<bool?>()))
            .Throws(new Exception("Test exception"));

        // Act & Assert
        await FluentActions.Invoking(() =>
                _sut.Enter_Child_Details())
            .Should().ThrowAsync<Exception>()
            .WithMessage("Test exception");
    }


    [Test]
    public async Task Add_Child_WhenMaxChildrenReached_ShouldRedirectToEnterDetailsWithoutAddingChild()
    {
        // Arrange
        var maxChildren = Enumerable.Range(1, 99).Select(x => new Child
        {
            FirstName = "Test",
            LastName = "Test",
            Day = "1",
            Month = "1",
            Year = "2010",
            School = _schools[0]
        }).ToList();

        var request = new Children { ChildList = maxChildren };

        _addChildUseCaseMock
            .Setup(x => x.Execute(request))
            .Returns(request);

        // Act
        var result = await _sut.Add_Child(request);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Enter_Child_Details");
        _sut.TempData["IsChildAddOrRemove"].Should().Be(true);
    }

    //[Test]
    //public async Task Add_Child_WhenUseCaseThrows_ShouldPropagateException()
    //{
    //    // Arrange
    //    var request = new Children { ChildList = new List<Child> { new Child() } };

    //    _addChildUseCaseMock
    //        .Setup(x => x.Execute(request))
    //        .Throws(new Exception("Test exception"));

    //    // Act & Assert
    //    await FluentActions.Invoking(() =>
    //        _sut.Add_Child(request))
    //        .Should().ThrowAsync<Exception>()
    //        .WithMessage("Test exception");
    //}

    [Test]
    public async Task Add_Child_ShouldAlwaysSetIsChildAddOrRemoveToTrue()
    {
        // Arrange
        var request = new Children { ChildList = new List<Child> { new() } };
        var updatedChildren = new Children
        {
            ChildList = new List<Child> { new(), new() }
        };

        _addChildUseCaseMock
            .Setup(x => x.Execute(request))
            .Returns(updatedChildren);

        // Act
        await _sut.Add_Child(request);

        // Assert
        _sut.TempData["IsChildAddOrRemove"].Should().Be(true);
    }

    [Test]
    public async Task Add_Child_WhenSuccessful_ShouldSerializeUpdatedChildrenToTempData()
    {
        // Arrange
        var request = new Children { ChildList = new List<Child> { new() } };
        var updatedChildren = new Children
        {
            ChildList = new List<Child> { new(), new() }
        };

        _addChildUseCaseMock
            .Setup(x => x.Execute(request))
            .Returns(updatedChildren);

        // Act
        await _sut.Add_Child(request);

        // Assert
        var serializedChildren = _sut.TempData["ChildList"] as string;
        serializedChildren.Should().NotBeNull();

        var deserializedChildren = JsonConvert.DeserializeObject<List<Child>>(serializedChildren);
        deserializedChildren.Should().BeEquivalentTo(updatedChildren.ChildList);
    }

    [Test]
    public async Task SignIn_ShouldReturnChallengeResultWithCorrectSchemeAndProperties()
    {
        // Arrange
        var mockAuthProperties = new AuthenticationProperties
        {
            RedirectUri = "/Check/CreateUser"
        };
        mockAuthProperties.SetString("vector_of_trust", @"[""Cl""]");

        _signInUseCaseMock
            .Setup(x => x.Execute("/Check/CreateUser"))
            .ReturnsAsync(mockAuthProperties);

        // Act
        var result = await _sut.SignIn();

        // Assert
        result.Should().BeOfType<ChallengeResult>();
        var challengeResult = result as ChallengeResult;

        challengeResult.AuthenticationSchemes.Should().ContainSingle()
            .Which.Should().Be(OneLoginDefaults.AuthenticationScheme);

        challengeResult.Properties.Should().NotBeNull();
        challengeResult.Properties.RedirectUri.Should().Be("/Check/CreateUser");
        challengeResult.Properties.GetString("vector_of_trust").Should().Be(@"[""Cl""]");

        _signInUseCaseMock.Verify(
            x => x.Execute("/Check/CreateUser"),
            Times.Once);
    }


    [Test]
    public async Task Given_EnterDetails_When_UserHasSelectedToInputANass_Should_RedirectToNassPage()
    {
        // Arrange
        _parent.IsNinoSelected = false;
        _parent.NationalInsuranceNumber = null;

        _performEligibilityCheckUseCaseMock
            .Setup(x => x.Execute(It.IsAny<Parent>(), It.IsAny<ISession>()))
            .ReturnsAsync((null, "Nass"));

        // Act
        var result = await _sut.Enter_Details(_parent);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult.ActionName.Should().Be("Nass");
    }


    [Test]
    public async Task CreateUser_WhenSuccessful_ShouldSetSessionAndRedirect()
    {
        // Arrange
        var email = "test@example.com";
        var uniqueId = "unique123";
        var userId = "user123";

        var claims = new List<Claim>
        {
            new("email", email),
            new("sub", uniqueId)
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        var sessionStorage = new Dictionary<string, byte[]>();
        var sessionMock = new Mock<ISession>();

        sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => sessionStorage[key] = value);

        sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) =>
            {
                if (sessionStorage.TryGetValue(key, out var storedValue))
                {
                    value = storedValue;
                    return true;
                }

                value = null;
                return false;
            });

        var httpContext = new DefaultHttpContext
        {
            User = principal,
            Session = sessionMock.Object
        };

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        _createUserUseCaseMock.Setup(x => x.Execute(email, uniqueId))
            .ReturnsAsync(userId);

        // Act
        var result = await _sut.CreateUser();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Enter_Child_Details");

        // Verify the session data
        Encoding.UTF8.GetString(sessionStorage["Email"]).Should().Be(email);
        Encoding.UTF8.GetString(sessionStorage["UserId"]).Should().Be(userId);

        _createUserUseCaseMock.Verify(x => x.Execute(email, uniqueId), Times.Once);
    }


    [Test]
    public async Task CreateUser_WhenUseCaseThrowsException_ShouldReturnTechnicalError()
    {
        // Arrange
        var email = "test@example.com";
        var uniqueId = "unique123";

        var claims = new List<Claim>
        {
            new("email", email),
            new("sub", uniqueId)
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _sut.ControllerContext.HttpContext = new DefaultHttpContext();
        _sut.ControllerContext.HttpContext.User = principal;

        _createUserUseCaseMock.Setup(x => x.Execute(
                It.Is<string>(e => e == email),
                It.Is<string>(u => u == uniqueId)))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _sut.CreateUser();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Outcome/Technical_Error");
        _createUserUseCaseMock.Verify(x => x.Execute(email, uniqueId), Times.Once);
    }

    [Test]
    public async Task CreateUser_WhenClaimsAreMissing_ShouldReturnTechnicalError()
    {
        // Arrange
        _sut.ControllerContext.HttpContext = new DefaultHttpContext();
        _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _sut.CreateUser();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("Outcome/Technical_Error");
        _createUserUseCaseMock.Verify(x => x.Execute(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }


    [Test]
    public void UploadEvidence_Get_Should_Return_View()
    {
        // Arrange
        _sut.TempData["FsmApplication"] = JsonConvert.SerializeObject(_fsmApplication);

        // Act
        var result = _sut.UploadEvidence();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeEquivalentTo(_fsmApplication, options =>
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

        var evidenceFile = new EvidenceFile
        {
            FileName = fileName,
            FileType = "application/pdf",
            StorageAccountReference = "test-reference"
        };

        _fsmApplication.Evidence.EvidenceList.Add(evidenceFile);

        _sut.TempData["FsmApplication"] = JsonConvert.SerializeObject(_fsmApplication);

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
        var request = _fsmApplication;

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

        _validateEvidenceFileUseCaseMock
            .Setup(x => x.Execute(It.IsAny<IFormFile>()))
            .Returns(new EvidenceFileValidationResult() { IsValid = true });

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
        var request = new FsmApplication(); 
        request.EvidenceFiles = new List<IFormFile>();

        // Create existing evidence in TempData
        var existingEvidence = new Evidences
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
        var request = _fsmApplication;

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

        _validateEvidenceFileUseCaseMock
            .Setup(x => x.Execute(It.IsAny<IFormFile>()))
            .Returns(new EvidenceFileValidationResult() { IsValid = true });

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
    public async Task UploadEvidence_Post_When_UploadInvalid_Should_Add_ErrorMessage_And_Redirect()
    {
        // Arrange
        var request = _fsmApplication;

        // Create a mock file that will fail to upload
        var fileMock = new Mock<IFormFile>();
        var fileName = "error-file.txt";
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.ContentType).Returns("plain/text");

        request.EvidenceFiles = new List<IFormFile> { fileMock.Object };

        // Make the upload throw an exception
        _uploadEvidenceFileUseCaseMock
            .Setup(x => x.Execute(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Upload failed"));

        _validateEvidenceFileUseCaseMock
            .Setup(x => x.Execute(It.IsAny<IFormFile>()))
            .Returns(new EvidenceFileValidationResult() { IsValid = false, ErrorMessage = "Invalid file type" });

        // Act
        var result = await _sut.UploadEvidence(request);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be("UploadEvidence");

        // Verify model state has errors
        _sut.TempData.Should().NotBeNull();
        _sut.TempData["ErrorMessage"].ToString().Should().Match("Invalid file type");

        _uploadEvidenceFileUseCaseMock.Verify(
            x => x.Execute(It.IsAny<IFormFile>(), It.IsAny<string>()),
            Times.Never);
    }

}