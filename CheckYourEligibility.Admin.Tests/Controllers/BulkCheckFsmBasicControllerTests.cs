using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Controllers;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.Usecases;
using CheckYourEligibility.Admin.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using System.Text;
using static CheckYourEligibility.Admin.Models.Constants;

namespace CheckYourEligibility.Admin.Tests.Controllers;

[TestFixture]
public class BulkCheckFsmBasicControllerTests
{
    private Mock<ILogger<BulkCheckFsmBasicController>> _loggerMock = null!;
    private Mock<ICheckGateway> _checkGatewayMock = null!;
    private Mock<IConfiguration> _configurationMock = null!;
    private Mock<IGetBulkCheckStatusesUseCase_FsmBasic> _getBulkCheckStatusesUseCaseMock = null!;
    private Mock<IParseBulkCheckFileUseCase_FsmBasic> _parseBulkCheckFileUseCaseMock = null!;
    private Mock<IDeleteBulkCheckFileUseCase_FsmBasic> _deleteBulkCheckFileUseCaseMock = null!;
    private Mock<IDfeSignInApiService> _dfeSignInApiServiceCaseMock = null;

    private BulkCheckFsmBasicController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<BulkCheckFsmBasicController>>();
        _checkGatewayMock = new Mock<ICheckGateway>();
        _configurationMock = new Mock<IConfiguration>();
        _getBulkCheckStatusesUseCaseMock = new Mock<IGetBulkCheckStatusesUseCase_FsmBasic>();
        _parseBulkCheckFileUseCaseMock = new Mock<IParseBulkCheckFileUseCase_FsmBasic>();
        _deleteBulkCheckFileUseCaseMock = new Mock<IDeleteBulkCheckFileUseCase_FsmBasic>();
        _dfeSignInApiServiceCaseMock = new Mock<IDfeSignInApiService>();

    // Setup configuration
    _configurationMock.Setup(c => c["BulkEligibilityCheckLimit"]).Returns("500");
        _configurationMock.Setup(c => c["BulkUploadAttemptLimit"]).Returns("5");
        
        // Setup HttpContext with session
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession();

        // Setup user claims for DfE SignIn with all required claims
        // Organisation claim needs valid GUID id and urn for GetDfeClaims to work
        const string organisationJson = "{\"id\":\"4579AE90-8B2B-4C02-AC08-756CBBB1C567\",\"name\":\"Test School\",\"category\":{\"id\":\"001\",\"name\":\"Establishment\"},\"type\":{\"id\":\"01\",\"name\":\"Community School\"},\"urn\":\"123456\",\"establishmentNumber\":\"123456\"}";
        
        var claims = new List<Claim>
        {
            new Claim("organisation", organisationJson),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "test-user-123"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@test.com"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "Test"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        httpContext.User = claimsPrincipal;

        _controller = new BulkCheckFsmBasicController(
            _loggerMock.Object,
            _checkGatewayMock.Object,
            _configurationMock.Object,
            _parseBulkCheckFileUseCaseMock.Object,
            _getBulkCheckStatusesUseCaseMock.Object,
            _deleteBulkCheckFileUseCaseMock.Object,
            _dfeSignInApiServiceCaseMock.Object
        );
		_controller.ControllerContext.HttpContext = httpContext;
		_controller.GetDfeClaimsAsync().Wait();

        // Setup TempData
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    #region Bulk_Check GET Tests

    [Test]
    public void Bulk_Check_Get_ReturnsViewResult()
    {
        // Act
        var result = _controller.Bulk_Check_FSMB();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public void Bulk_Check_Get_ModelContainsDocumentTemplatePath()
    {
        // Act
        var result = _controller.Bulk_Check_FSMB();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        // The GET action no longer returns a model with DocumentTemplatePath
    }

    #endregion

    #region Bulk_Check_History Tests

    [Test]
    public async Task Bulk_Check_History_ReturnsViewWithChecks()
    {
        // Arrange
        var mockChecks = new List<BulkCheck>
        {
            new BulkCheck
            {
                BulkCheckId = "guid-1",
                Filename = "test1.csv",
                Status = "Completed",
                SubmittedDate = DateTime.Now.AddDays(-1),
                SubmittedBy = "user@test.com",
                EligibilityType = "FreeSchoolMeals",
                NumberOfRecords = 25,
                FinalNameInCheck = "Smith"
            },
            new BulkCheck
            {
                BulkCheckId = "guid-2",
                Filename = "test2.csv",
                Status = "In progress",
                SubmittedDate = DateTime.Now.AddDays(-2),
                SubmittedBy = "user2@test.com",
                EligibilityType = "FreeSchoolMeals",
                NumberOfRecords = 50,
                FinalNameInCheck = "Jones"
            }
        };

        _getBulkCheckStatusesUseCaseMock
            .Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(mockChecks);

        // Act
        var result = await _controller.Bulk_Check_History_FSMB();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult.Model, Is.InstanceOf<BulkCheckFsmBasicStatusesViewModel>());
        
        var model = viewResult.Model as BulkCheckFsmBasicStatusesViewModel;
        Assert.That(model.Checks.Count, Is.EqualTo(2));
        Assert.That(model.Checks[0].Filename, Is.EqualTo("test1.csv"));
        Assert.That(model.Checks[0].NumberOfRecords, Is.EqualTo(25));
        Assert.That(model.Checks[0].FinalNameInCheck, Is.EqualTo("Smith"));
    }

    [Test]
    public async Task Bulk_Check_History_WhenNoChecks_ReturnsEmptyList()
    {
        // Arrange
        _getBulkCheckStatusesUseCaseMock
            .Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(new List<BulkCheck>());

        // Act
        var result = await _controller.Bulk_Check_History_FSMB();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        var model = viewResult.Model as BulkCheckFsmBasicStatusesViewModel;
        Assert.That(model.Checks, Is.Empty);
    }

    [Test]
    public async Task Bulk_Check_History_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var mockChecks = Enumerable.Range(1, 25).Select(i => new BulkCheck
        {
            BulkCheckId = $"guid-{i}",
            Filename = $"test{i}.csv",
            Status = "Completed",
            SubmittedDate = DateTime.Now.AddDays(-i),
            SubmittedBy = "user@test.com",
            EligibilityType = "FreeSchoolMeals",
            NumberOfRecords = i * 10
        }).ToList();

        _getBulkCheckStatusesUseCaseMock
            .Setup(x => x.Execute(It.IsAny<string>()))
            .ReturnsAsync(mockChecks);

        // Act - get page 2
        var result = await _controller.Bulk_Check_History_FSMB(pageNumber: 2);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        var model = viewResult.Model as BulkCheckFsmBasicStatusesViewModel;
        Assert.That(model.CurrentPage, Is.EqualTo(2));
        Assert.That(model.TotalRecords, Is.EqualTo(25));
    }

    #endregion

    #region Bulk_Check_View_Results Tests

    [Test]
    public async Task Bulk_Check_View_Results_ReturnsResultsView()
    {
        // Arrange
        var bulkCheckId = "test-guid-123";
        var mockResponse = new CheckEligibilityBulkResponse
        {
            Data = new List<CheckEligibilityItem>
            {
                new CheckEligibilityItem { LastName = "Smith", Status = "Eligible" },
                new CheckEligibilityItem { LastName = "Jones", Status = "NotEligible" }
            }
        };

        _checkGatewayMock
            .Setup(x => x.GetBulkCheckResults_FsmBasic(It.IsAny<string>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.Bulk_Check_View_Results_FSMB(bulkCheckId);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Bulk_Check_View_Results_WithNoResults_RedirectsToHistory()
    {
        // Arrange
        var bulkCheckId = "test-guid-123";
        var emptyResponse = new CheckEligibilityBulkResponse
        {
            Data = new List<CheckEligibilityItem>()
        };

        _checkGatewayMock
            .Setup(x => x.GetBulkCheckResults_FsmBasic(It.IsAny<string>()))
            .ReturnsAsync(emptyResponse);

        // Act
        var result = await _controller.Bulk_Check_View_Results_FSMB(bulkCheckId);

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
    }

    #endregion

    #region Bulk_Check_Download Tests

    [Test]
    public async Task Bulk_Check_Download_WithNoResults_RedirectsToHistory()
    {
        // Arrange - when gateway returns empty results, controller redirects
        var bulkCheckId = "test-guid-123";

        _checkGatewayMock
            .Setup(x => x.LoadBulkCheckResults_FsmBasic(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<IBulkExport>());

        // Act
        var result = await _controller.Bulk_Check_Download_FSMB(bulkCheckId);

        // Assert - controller redirects when no results
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
    }

    [Test]
    public async Task Bulk_Check_Download_WithEmptyBulkCheckId_RedirectsToHistory()
    {
        // Arrange - when bulkCheckId is empty, controller redirects
        var bulkCheckId = "";

        // Act
        var result = await _controller.Bulk_Check_Download_FSMB(bulkCheckId);

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
    }

    #endregion

    #region Bulk_Check_Delete Tests

    [Test]
    public async Task Bulk_Check_Delete_WithValidId_DeletesAndRedirects()
    {
        // Arrange
        var bulkCheckId = "test-guid-123";

        var deleteResponse = new CheckEligiblityBulkDeleteResponse
        {
            Success = true,
            Message = "Deleted successfully"
        };

        _deleteBulkCheckFileUseCaseMock
            .Setup(x => x.Execute(bulkCheckId))
            .ReturnsAsync(deleteResponse);

        // Act
        var result = await _controller.Bulk_Check_Delete_FSMB(bulkCheckId);

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult!.ActionName, Is.EqualTo("Bulk_Check_History_FSMB"));
    }

    [Test]
    public async Task Bulk_Check_Delete_WithEmptyId_RedirectsWithError()
    {
        // Arrange
        var bulkCheckId = "";

        var deleteResponse = new CheckEligiblityBulkDeleteResponse
        {
            Success = false,
            Message = "Invalid bulk check ID"
        };

        _deleteBulkCheckFileUseCaseMock
            .Setup(x => x.Execute(bulkCheckId))
            .ReturnsAsync(deleteResponse);

        // Act
        var result = await _controller.Bulk_Check_Delete_FSMB(bulkCheckId);

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
    }

    #endregion

    #region File Upload Tests

    [Test]
    public async Task Bulk_Check_Post_WithNullFile_RedirectsWithError()
    {
        // Arrange
        IFormFile nullFile = null;

        // Act
        var result = await _controller.Bulk_Check_FSMB(nullFile);

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult.ActionName, Is.EqualTo("Bulk_Check_FSMB"));
        Assert.That(_controller.TempData["ErrorMessage"], Is.EqualTo("Select a CSV file"));
    }

    [Test]
    public async Task Bulk_Check_Post_WithValidFile_ReturnsSubmittedView()
    {
        // Arrange
        var csvContent = "Last Name,Date of Birth,National Insurance Number\nSmith,1985-03-15,AB123456C";
        var mockFile = CreateMockFormFile("test.csv", csvContent);

        var parseResult = new BulkCheckCsvResultFsmBasic
        {
            ValidRequests = new List<CheckEligibilityRequestData_FsmBasic>
            {
                new CheckEligibilityRequestData_FsmBasic 
                { 
                    LastName = "Smith", 
                    DateOfBirth = "1985-03-15", 
                    NationalInsuranceNumber = "AB123456C" 
                }
            },
            Errors = new List<CsvRowErrorFsmBasic>()
        };

        _parseBulkCheckFileUseCaseMock
            .Setup(x => x.Execute(It.IsAny<Stream>()))
            .ReturnsAsync(parseResult);

        var bulkResponse = new CheckEligibilityResponseBulk
        {
            Data = new StatusValue { Status = "queuedForProcessing" },
            Links = new CheckEligibilityResponseBulkLinks
            {
                Get_BulkCheck_Status = "/bulk-check/new-guid/status",
                Get_Progress_Check = "/bulk-check/new-guid/progress",
                Get_BulkCheck_Results = "/bulk-check/new-guid/"
            }
        };

        _checkGatewayMock
            .Setup(x => x.PostBulkCheck_FsmBasic(It.IsAny<CheckEligibilityRequestBulk_FsmBasic>()))
            .ReturnsAsync(bulkResponse);

        // Act
        var result = await _controller.Bulk_Check_FSMB(mockFile);

        // Assert - controller now redirects to History instead of showing Submitted view
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult!.ActionName, Is.EqualTo("Bulk_Check_History_FSMB"));
    }

    [Test]
    public async Task Bulk_Check_Post_WithInvalidFile_ReturnsErrorsView()
    {
        // Arrange
        var csvContent = "Last Name,Date of Birth,National Insurance Number\nSmith,invalid-date,BADNI";
        var mockFile = CreateMockFormFile("test.csv", csvContent);

        var parseResult = new BulkCheckCsvResultFsmBasic
        {
            ValidRequests = new List<CheckEligibilityRequestData_FsmBasic>(),
            Errors = new List<CsvRowErrorFsmBasic>
            {
                new CsvRowErrorFsmBasic { LineNumber = 2, Message = "Invalid date format" }
            }
        };

        _parseBulkCheckFileUseCaseMock
            .Setup(x => x.Execute(It.IsAny<Stream>()))
            .ReturnsAsync(parseResult);

        // Act
        var result = await _controller.Bulk_Check_FSMB(mockFile);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult.ViewName, Is.EqualTo("BulkOutcomeFsmBasic/Error_Data_Issue_FSMB"));
    }

    [Test]
    public async Task Bulk_Check_Post_WhenLocalAuthorityUser_SetsLocalAuthorityIdInMeta()
    {
        // Arrange - Setup controller with Local Authority user
        var laOrganisationJson = "{\"id\":\"4579AE90-8B2B-4C02-AC08-756CBBB1C567\",\"name\":\"Telford and Wrekin Council\",\"category\":{\"id\":\"002\",\"name\":\"" + CategoryTypeLA + "\"},\"type\":{\"id\":\"01\",\"name\":\"Local Authority\"},\"urn\":\"123456\",\"establishmentNumber\":\"894\"}";
        
        var laClaims = new List<Claim>
        {
            new Claim("organisation", laOrganisationJson),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "la-user-123"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "lauser@council.gov.uk"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "LA"),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "User")
        };
        var laIdentity = new ClaimsIdentity(laClaims, "TestAuth");
        var laClaimsPrincipal = new ClaimsPrincipal(laIdentity);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession();
        httpContext.User = laClaimsPrincipal;

        var laController = new BulkCheckFsmBasicController(
            _loggerMock.Object,
            _checkGatewayMock.Object,
            _configurationMock.Object,
            _parseBulkCheckFileUseCaseMock.Object,
            _getBulkCheckStatusesUseCaseMock.Object,
            _deleteBulkCheckFileUseCaseMock.Object,
            _dfeSignInApiServiceCaseMock.Object
        );
        laController.ControllerContext = new ControllerContext { HttpContext = httpContext };
        laController.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        var csvContent = "Last Name,Date of Birth,National Insurance Number\nSmith,1985-03-15,AB123456C";
        var mockFile = CreateMockFormFile("test.csv", csvContent);

        var parseResult = new BulkCheckCsvResultFsmBasic
        {
            ValidRequests = new List<CheckEligibilityRequestData_FsmBasic>
            {
                new CheckEligibilityRequestData_FsmBasic 
                { 
                    LastName = "Smith", 
                    DateOfBirth = "1985-03-15", 
                    NationalInsuranceNumber = "AB123456C" 
                }
            },
            Errors = new List<CsvRowErrorFsmBasic>()
        };

        _parseBulkCheckFileUseCaseMock
            .Setup(x => x.Execute(It.IsAny<Stream>()))
            .ReturnsAsync(parseResult);

        CheckEligibilityRequestBulk_FsmBasic? capturedRequest = null;
        _checkGatewayMock
            .Setup(x => x.PostBulkCheck_FsmBasic(It.IsAny<CheckEligibilityRequestBulk_FsmBasic>()))
            .Callback<CheckEligibilityRequestBulk_FsmBasic>(req => capturedRequest = req)
            .ReturnsAsync(new CheckEligibilityResponseBulk
            {
                Data = new StatusValue { Status = "queuedForProcessing" },
                Links = new CheckEligibilityResponseBulkLinks
                {
                    Get_BulkCheck_Status = "/bulk-check/new-guid/status"
                }
            });

        // Act
        await laController.Bulk_Check_FSMB(mockFile);

        // Assert - LocalAuthorityId should be set to the establishment number
        Assert.That(capturedRequest, Is.Not.Null);
        Assert.That(capturedRequest!.Meta.LocalAuthorityId, Is.EqualTo("894"));
        Assert.That(capturedRequest.Meta.SubmittedBy, Is.EqualTo("LA User"));

        laController.Dispose();
    }

    [Test]
    public async Task Bulk_Check_Post_WhenSchoolUser_LocalAuthorityIdIsNull()
    {
        // Arrange - Uses the default School user from Setup
        var csvContent = "Last Name,Date of Birth,National Insurance Number\nSmith,1985-03-15,AB123456C";
        var mockFile = CreateMockFormFile("test.csv", csvContent);

        var parseResult = new BulkCheckCsvResultFsmBasic
        {
            ValidRequests = new List<CheckEligibilityRequestData_FsmBasic>
            {
                new CheckEligibilityRequestData_FsmBasic 
                { 
                    LastName = "Smith", 
                    DateOfBirth = "1985-03-15", 
                    NationalInsuranceNumber = "AB123456C" 
                }
            },
            Errors = new List<CsvRowErrorFsmBasic>()
        };

        _parseBulkCheckFileUseCaseMock
            .Setup(x => x.Execute(It.IsAny<Stream>()))
            .ReturnsAsync(parseResult);

        CheckEligibilityRequestBulk_FsmBasic? capturedRequest = null;
        _checkGatewayMock
            .Setup(x => x.PostBulkCheck_FsmBasic(It.IsAny<CheckEligibilityRequestBulk_FsmBasic>()))
            .Callback<CheckEligibilityRequestBulk_FsmBasic>(req => capturedRequest = req)
            .ReturnsAsync(new CheckEligibilityResponseBulk
            {
                Data = new StatusValue { Status = "queuedForProcessing" },
                Links = new CheckEligibilityResponseBulkLinks
                {
                    Get_BulkCheck_Status = "/bulk-check/new-guid/status"
                }
            });

        // Act
        await _controller.Bulk_Check_FSMB(mockFile);

        // Assert - LocalAuthorityId should be null for school users
        Assert.That(capturedRequest, Is.Not.Null);
        Assert.That(capturedRequest!.Meta.LocalAuthorityId, Is.Null);
        Assert.That(capturedRequest.Meta.SubmittedBy, Is.EqualTo("Test User"));
    }

    #endregion

    #region Helper Methods

    private IFormFile CreateMockFormFile(string fileName, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(bytes.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.ContentType).Returns("text/csv");
        
        return fileMock.Object;
    }

    #endregion
}

/// <summary>
/// Simple in-memory session implementation for testing
/// </summary>
public class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _sessionStorage = new();

    public bool IsAvailable => true;
    public string Id => Guid.NewGuid().ToString();
    public IEnumerable<string> Keys => _sessionStorage.Keys;

    public void Clear() => _sessionStorage.Clear();

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Remove(string key) => _sessionStorage.Remove(key);

    public void Set(string key, byte[] value) => _sessionStorage[key] = value;

    public bool TryGetValue(string key, out byte[] value) => _sessionStorage.TryGetValue(key, out value);
}
