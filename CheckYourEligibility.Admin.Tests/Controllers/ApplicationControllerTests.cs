using System.Collections;
using AutoFixture;
using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Boundary.Shared;
using CheckYourEligibility.Admin.Controllers;
using CheckYourEligibility.Admin.Domain.Enums;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.UseCases;
using CheckYourEligibility.Admin.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using static CheckYourEligibility.Admin.Boundary.Responses.ApplicationResponse;
using CheckYourEligibility.Admin.Tests.Properties;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using System.Security.Claims;

namespace CheckYourEligibility.Admin.Tests.Controllers;

[TestFixture]
public class ApplicationControllerTests : TestBase
{
    [SetUp]
    public void SetUp()
    {
        _adminGatewayMock = new Mock<IAdminGateway>();
        _loggerMock = Mock.Of<ILogger<ApplicationController>>();
        _configurationMock = new Mock<IConfiguration>();
        _downloadEvidenceFileUseCaseMock = new Mock<IDownloadEvidenceFileUseCase>();
        _sendNotificationUseCaseMock = new Mock<ISendNotificationUseCase>();
        _sut = new ApplicationController(_loggerMock, _adminGatewayMock.Object, _configurationMock.Object, _downloadEvidenceFileUseCaseMock.Object, _sendNotificationUseCaseMock.Object);

        base.SetUp();
        _sut.ControllerContext.HttpContext = _httpContext.Object;
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
    }

    //mocks
    private ILogger<ApplicationController> _loggerMock;
    private Mock<IAdminGateway> _adminGatewayMock;
    private Mock<IConfiguration> _configurationMock;
    private Mock<IDownloadEvidenceFileUseCase> _downloadEvidenceFileUseCaseMock;
    private Mock<ISendNotificationUseCase> _sendNotificationUseCaseMock;

    // system under test
    private ApplicationController _sut;

    [Test]
    public async Task Given_Application_Search_Should_Load_ApplicationSearchPage()
    {
        // Arrange 
        _sut.TempData = _tempData;

        // Act
        var result = _sut.Search();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeNull();
    }

    [Test]
    public async Task Given_Application_Establishment_Search_Results_Page_Returns_Valid_Data()
    {
        //arrange
        _sut.TempData = _tempData;
        var response = _fixture.Create<ApplicationSearchResponse>();

        _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.Is<ApplicationRequestSearch>(r => r.Data.Establishment == 123456)))
            .ReturnsAsync(response);

        var request = new ApplicationSearch();

        //act
        var result = await _sut.SearchResults(request);

        //assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeAssignableTo<SearchAllRecordsViewModel>();

        var model = viewResult.Model as SearchAllRecordsViewModel;
        model.Should().NotBeNull();

        model.People.Count.Should().Be(response.Data.Count());
    }

    [Test]
    public async Task Given_Application_Search_LocalAuthority_Results_Page_Returns_Valid_Data()
    {
        //arrange
        _sut.TempData = _tempData;
        var response = _fixture.Create<ApplicationSearchResponse>();
        var localAuthority = 1;

        _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.Is<ApplicationRequestSearch>(r => r.Data.LocalAuthority == localAuthority)))
            .ReturnsAsync(response);

        var request = new ApplicationSearch();

        var organisationClaim = Resources.ClaimSchool
            .Replace("\"name\":\"Establishment\"", $"\"name\":\"{Constants.CategoryTypeLA}\"")
            .Replace("\"establishmentNumber\":\"2200\"", $"\"establishmentNumber\":\"{localAuthority}\"");
        var claimLA = new Claim("organisation", organisationClaim);
        _userMock.Setup(x => x.Claims).Returns(new List<Claim>
        {
            claimLA,
            new($"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/{ClaimConstants.NameIdentifier}", "123"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@test.com"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "testFirstName"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "testSurname")
        });

        //act
        var result = await _sut.SearchResults(request);

        //assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeAssignableTo<SearchAllRecordsViewModel>();

        var model = viewResult.Model as SearchAllRecordsViewModel;
        model.Should().NotBeNull();

        model.People.Count.Should().Be(response.Data.Count());
    }

    [Test]
    public async Task Given_Application_Search_MultiAcademyTrust_Results_Page_Returns_Valid_Data()
    {
        //arrange
        _sut.TempData = _tempData;
        var response = _fixture.Create<ApplicationSearchResponse>();
        var multiAcademyTrust = 1;

        _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.Is<ApplicationRequestSearch>(r => r.Data.MultiAcademyTrust == multiAcademyTrust)))
            .ReturnsAsync(response);

        var request = new ApplicationSearch();

        var organisationClaim = Resources.ClaimSchool
            .Replace("\"name\":\"Establishment\"", $"\"name\":\"{Constants.CategoryTypeMAT}\"")
            .Replace("\"uid\":null", $"\"uid\":\"{multiAcademyTrust}\"");
        var claimMAT = new Claim("organisation", organisationClaim);
        _userMock.Setup(x => x.Claims).Returns(new List<Claim>
        {
            claimMAT,
            new($"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/{ClaimConstants.NameIdentifier}", "123"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "test@test.com"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", "testFirstName"),
            new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "testSurname")
        });

        //act
        var result = await _sut.SearchResults(request);

        //assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeAssignableTo<SearchAllRecordsViewModel>();

        var model = viewResult.Model as SearchAllRecordsViewModel;
        model.Should().NotBeNull();

        model.People.Count.Should().Be(response.Data.Count());
    }

    [Test]
    public async Task SearchResults_When_ModelStateIsInvalid_Should_ReturnView_With_Errors()
    {
        // Arrange
        _sut.TempData = _tempData;
        var request = new ApplicationSearch();
        _sut.ModelState.AddModelError("Keyword", "Keyword is required");
        
        // Act
        var result = await _sut.SearchResults(request);
        
        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeAssignableTo<SearchAllRecordsViewModel>();
        _sut.TempData.Should().ContainKey("Errors");
        _sut.TempData.Should().ContainKey("ApplicationSearch");
    }

    [Test]
    public async Task Given_ApplicationDetail_Results_Page_Returns_Valid_Data()
    {
        //arrange
        var response = _fixture.Create<ApplicationItemResponse>();
        response.Data.ChildDateOfBirth = "2007-08-14";
        response.Data.ParentDateOfBirth = "2007-08-14";
        var claims = DfeSignInExtensions.GetDfeClaims(_httpContext.Object.User.Claims);
        response.Data.Establishment.Id = Convert.ToInt32(claims.Organisation.Urn);

        _adminGatewayMock.Setup(s => s.GetApplication(It.IsAny<string>()))
            .ReturnsAsync(response);

        var request = new ApplicationSearch();

        //act
        var result = await _sut.ApplicationDetail(response.Data.Id);

        //assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeAssignableTo<ApplicationDetailViewModel>();

        var model = viewResult.Model as ApplicationDetailViewModel;
        model.Should().NotBeNull();
    }

    [Test]
    public async Task Given_ApplicationDetail_Results_Returns_NotFound()
    {
        //arrange
        var response = _fixture.Create<ApplicationItemResponse>();
        var claims = DfeSignInExtensions.GetDfeClaims(_httpContext.Object.User.Claims);
        response.Data.Establishment.Id = Convert.ToInt32(claims.Organisation.Urn);

        _adminGatewayMock.Setup(s => s.GetApplication(It.IsAny<string>()))
            .ReturnsAsync(default(ApplicationItemResponse));

        var request = new ApplicationSearch();

        //act
        var result = await _sut.ApplicationDetail(response.Data.Id);

        //assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task Given_ApplicationDetail_Results_Returns_UnauthorizedResult()
    {
        //arrange
        var response = _fixture.Create<ApplicationItemResponse>();
        response.Data.Establishment.Id = -99;

        _adminGatewayMock.Setup(s => s.GetApplication(It.IsAny<string>()))
            .ReturnsAsync(response);

        var request = new ApplicationSearch();

        //act
        var result = await _sut.ApplicationDetail(response.Data.Id);

        //assert
        result.Should().BeOfType<ContentResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Test]
    public async Task ExportSearchResults_With_ValidResults_Should_ReturnCsvFile()
    {
        // Arrange
        _sut.TempData = _tempData;
        var response = _fixture.Create<ApplicationSearchResponse>();
        foreach (var item in response.Data)
        {
            item.ChildDateOfBirth = "1990-01-01";
            item.ParentDateOfBirth = "1970-01-01";
            item.Created = DateTime.Now;
        }
        
        var searchCriteria = new ApplicationRequestSearch
        {
            Meta = new ApplicationRequestSearchMeta() {
                PageNumber = 1,
                PageSize = 10,
            },
            Data = new ApplicationRequestSearchData()
        };
        
        _sut.TempData["SearchCriteria"] = JsonConvert.SerializeObject(searchCriteria);
        
        _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch>()))
            .ReturnsAsync(response);
            
        // Act
        var result = await _sut.ExportSearchResults();
        
        // Assert
        result.Should().BeOfType<FileContentResult>();
        var fileResult = result as FileContentResult;
        fileResult.ContentType.Should().Be("text/csv");
        fileResult.FileDownloadName.Should().StartWith("eligibility-applications-");
    }
    
    [Test]
    public async Task ExportSearchResults_With_NoResults_Should_RedirectToSearchResults()
    {
        // Arrange
        _sut.TempData = _tempData;
        var searchCriteria = new ApplicationRequestSearch
        {
            Meta = new ApplicationRequestSearchMeta() {
                PageNumber = 1,
                PageSize = 10
            },
            Data = new ApplicationRequestSearchData()
        };
        
        _sut.TempData["SearchCriteria"] = JsonConvert.SerializeObject(searchCriteria);
        
        _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch>()))
            .ReturnsAsync(new ApplicationSearchResponse { Data = new List<ApplicationResponse>() });
            
        // Act
        var result = await _sut.ExportSearchResults();
        
        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult.ActionName.Should().Be("SearchResults");
    }

    [Test]
    public async Task DownloadEvidence_With_ValidBlob_Should_ReturnFile()
    {
        // Arrange
        var id = "application-id";
        var blobReference = "file-reference";
        var evidenceFileName = "evidence.pdf";
        
        var response = _fixture.Create<ApplicationItemResponse>();
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes("test file content"));
        var contentType = "application/pdf";
        
        response.Data.Evidence = new List<ApplicationEvidence>
        {
            new()
            {
                StorageAccountReference = blobReference,
                FileName = evidenceFileName
            }
        };
        
        var claims = DfeSignInExtensions.GetDfeClaims(_httpContext.Object.User.Claims);
        response.Data.Establishment.Id = Convert.ToInt32(claims.Organisation.Urn);
        
        _adminGatewayMock.Setup(s => s.GetApplication(id))
            .ReturnsAsync(response);
            
        _downloadEvidenceFileUseCaseMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((fileStream, contentType));
        
        // Act
        var result = await _sut.DownloadEvidence(id, blobReference);
        
        // Assert
        result.Should().BeOfType<FileStreamResult>();
        var fileResult = result as FileStreamResult;
        fileResult.ContentType.Should().Be(contentType);
        fileResult.FileDownloadName.Should().Be(evidenceFileName);
    }
    
    [Test]
    public async Task DownloadEvidence_With_InvalidAccess_Should_ReturnForbidden()
    {
        // Arrange
        var id = "application-id";
        var blobReference = "file-reference";
        
        var response = _fixture.Create<ApplicationItemResponse>();
        response.Data.Establishment.Id = -99; // Different from user's organization
        
        _adminGatewayMock.Setup(s => s.GetApplication(id))
            .ReturnsAsync(response);
        
        // Act
        var result = await _sut.DownloadEvidence(id, blobReference);
        
        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;
        contentResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }
    
    [Test]
    public async Task DownloadEvidence_When_ApplicationNotFound_Should_ReturnNotFound()
    {
        // Arrange
        var id = "application-id";
        var blobReference = "file-reference";
        
        _adminGatewayMock.Setup(s => s.GetApplication(id))
            .ReturnsAsync(default(ApplicationItemResponse));
        
        // Act
        var result = await _sut.DownloadEvidence(id, blobReference);
        
        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
    
    [Test]
    public async Task DownloadEvidence_When_BlobNotFoundInApplication_Should_ReturnNotFound()
    {
        // Arrange
        var id = "application-id";
        var blobReference = "wrong-reference";
        
        var response = _fixture.Create<ApplicationItemResponse>();
        var claims = DfeSignInExtensions.GetDfeClaims(_httpContext.Object.User.Claims);
        response.Data.Establishment.Id = Convert.ToInt32(claims.Organisation.Urn);
        
        response.Data.Evidence = new List<ApplicationEvidence>
        {
            new()
            {
                StorageAccountReference = "correct-reference",
                FileName = "evidence.pdf"
            }
        };
        
        _adminGatewayMock.Setup(s => s.GetApplication(id))
            .ReturnsAsync(response);
        
        // Act
        var result = await _sut.DownloadEvidence(id, blobReference);
        
        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
    
    [Test]
    public async Task DownloadEvidence_When_StorageThrowsFileNotFoundException_Should_ReturnNotFound()
    {
        // Arrange
        var id = "application-id";
        var blobReference = "file-reference";
        
        var response = _fixture.Create<ApplicationItemResponse>();
        var claims = DfeSignInExtensions.GetDfeClaims(_httpContext.Object.User.Claims);
        response.Data.Establishment.Id = Convert.ToInt32(claims.Organisation.Urn);
        
        response.Data.Evidence = new List<ApplicationEvidence>
        {
            new()
            {
                StorageAccountReference = blobReference,
                FileName = "evidence.pdf"
            }
        };
        
        _adminGatewayMock.Setup(s => s.GetApplication(id))
            .ReturnsAsync(response);
            
        _downloadEvidenceFileUseCaseMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new FileNotFoundException());
        
        // Act
        var result = await _sut.DownloadEvidence(id, blobReference);
        
        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
    
    [Test]
    public async Task DownloadEvidence_When_StorageThrowsGeneralException_Should_ReturnInternalServerError()
    {
        // Arrange
        var id = "application-id";
        var blobReference = "file-reference";
        
        var response = _fixture.Create<ApplicationItemResponse>();
        var claims = DfeSignInExtensions.GetDfeClaims(_httpContext.Object.User.Claims);
        response.Data.Establishment.Id = Convert.ToInt32(claims.Organisation.Urn);
        
        response.Data.Evidence = new List<ApplicationEvidence>
        {
            new()
            {
                StorageAccountReference = blobReference,
                FileName = "evidence.pdf"
            }
        };
        
        _adminGatewayMock.Setup(s => s.GetApplication(id))
            .ReturnsAsync(response);
            
        _downloadEvidenceFileUseCaseMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Storage error"));
        
        // Act
        var result = await _sut.DownloadEvidence(id, blobReference);
        
        // Assert
        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
    }
    
    [Test]
    public async Task ViewEvidence_With_ValidBlob_Should_ReturnFile()
    {
        // Arrange
        var id = "application-id";
        var blobReference = "file-reference";
        
        var response = _fixture.Create<ApplicationItemResponse>();
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes("test file content"));
        var contentType = "application/pdf";
        
        response.Data.Evidence = new List<ApplicationEvidence>
        {
            new()
            {
                StorageAccountReference = blobReference,
                FileName = "evidence.pdf"
            }
        };
        
        var claims = DfeSignInExtensions.GetDfeClaims(_httpContext.Object.User.Claims);
        response.Data.Establishment.Id = Convert.ToInt32(claims.Organisation.Urn);
        
        _adminGatewayMock.Setup(s => s.GetApplication(id))
            .ReturnsAsync(response);
            
        _downloadEvidenceFileUseCaseMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((fileStream, contentType));
        
        // Act
        var result = await _sut.ViewEvidence(id, blobReference);
        
        // Assert
        result.Should().BeOfType<FileStreamResult>();
        var fileResult = result as FileStreamResult;
        fileResult.ContentType.Should().Be(contentType);
        fileResult.FileDownloadName.Should().Be("");
    }
    
    [Test]
    public async Task ViewEvidence_With_InvalidAccess_Should_ReturnForbidden()
    {
        // Arrange
        var id = "application-id";
        var blobReference = "file-reference";
        
        var response = _fixture.Create<ApplicationItemResponse>();
        response.Data.Establishment.Id = -99; // Different from user's organization
        
        _adminGatewayMock.Setup(s => s.GetApplication(id))
            .ReturnsAsync(response);
        
        // Act
        var result = await _sut.ViewEvidence(id, blobReference);
        
        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;
        contentResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }
    
    [Test]
    public void EvidenceGuidance_Should_Return_View()
    {
        // Act
        var result = _sut.EvidenceGuidance();
        
        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().BeNull(); // Default view
    }

    [Test]
    public async Task Given_Process_Appeals_Results_Page_Returns_Valid_Data()
    {
        //arrange
        _sut.TempData = _tempData;
        var response = _fixture.Create<ApplicationSearchResponse>();

        _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch>()))
            .ReturnsAsync(response);

        var request = new ApplicationSearch();

        //act
        var result = await _sut.AppealsApplications(0);

        //assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeAssignableTo<PeopleSelectionViewModel>();

        var model = viewResult.Model as PeopleSelectionViewModel;
        model.Should().NotBeNull();
    }

    [Test]
    public async Task Given_Process_Appeals_Returns_No_Records_null()
    {
        //Arrange
        _sut.TempData = _tempData;
        _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch>()))
            .ReturnsAsync(new ApplicationSearchResponse { Data = new List<ApplicationResponse>(), Meta = new ApplicationSearchResponseMeta() });

        var request = new ApplicationSearch();

        //act
        var result = await _sut.AppealsApplications(0);

        //assert 
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        var resultData = viewResult.Model as PeopleSelectionViewModel;
    }

    [Test]
    public async Task Given_ApplicationDetailAppeal_Results_Page_Returns_Valid_Data()
    {
        //arrange
        var response = _fixture.Create<ApplicationItemResponse>();
        response.Data.ChildDateOfBirth = "2007-08-14";
        response.Data.ParentDateOfBirth = "2007-08-14";
        var claims = DfeSignInExtensions.GetDfeClaims(_httpContext.Object.User.Claims);
        response.Data.Establishment.Id = Convert.ToInt32(claims.Organisation.Urn);

        _adminGatewayMock.Setup(s => s.GetApplication(It.IsAny<string>()))
            .ReturnsAsync(response);

        var request = new ApplicationSearch();

        //act
        var result = await _sut.ApplicationDetailAppeal(response.Data.Id);

        //assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeAssignableTo<ApplicationDetailViewModel>();

        var model = viewResult.Model as ApplicationDetailViewModel;
        model.Should().NotBeNull();
    }

    [Test]
    public async Task Given_ApplicationDetailAppeal_Results_Returns_NotFound()
    {
        //arrange
        var response = _fixture.Create<ApplicationItemResponse>();
        var claims = DfeSignInExtensions.GetDfeClaims(_httpContext.Object.User.Claims);
        response.Data.Establishment.Id = Convert.ToInt32(claims.Organisation.Urn);

        _adminGatewayMock.Setup(s => s.GetApplication(It.IsAny<string>()))
            .ReturnsAsync(default(ApplicationItemResponse));

        var request = new ApplicationSearch();

        //act
        var result = await _sut.ApplicationDetailAppeal(response.Data.Id);

        //assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task Given_ApplicationDetailAppeal_Results_Returns_ForbiddenResult()
    {
        //arrange
        var response = _fixture.Create<ApplicationItemResponse>();
        response.Data.Establishment.Id = -99;

        _adminGatewayMock.Setup(s => s.GetApplication(It.IsAny<string>()))
            .ReturnsAsync(response);

        var request = new ApplicationSearch();

        //act
        var result = await _sut.ApplicationDetailAppeal(response.Data.Id);

        //assert
        result.Should().BeOfType<ContentResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Test]
    public async Task Given_ApplicationDetailAppealConfirmation_Returns_ViewResult()
    {
        //Arrange
        _sut.TempData = _tempData;
        var id = _fixture.Create<string>();
        //act
        var result = await _sut.ApplicationDetailAppealConfirmation(id);

        //assert 
        result.Should().BeOfType<ViewResult>();
        _sut.TempData["AppAppealID"].Should().Be(id);
    }

    [Test]
    public async Task Given_ApplicationDetailAppealSend_Returns_ViewResult()
    {
        //Arrange
        var id = "f41e59a2-9847-4084-9e17-0511e77571fb";
        var response = _fixture.Create<Task<ApplicationItemResponse>>();
        response.Result.Data.Establishment.Id = 123456;
        response.Result.Data.Id = id;

        _adminGatewayMock.Setup(x => x.GetApplication(It.IsAny<string>())).Returns(response);

        //act
        var result = await _sut.ApplicationDetailAppealSend(id);

        //assert 
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result as RedirectToActionResult;
        redirect.ActionName.Should().BeEquivalentTo("ApplicationDetailAppealConfirmationSent");
    }

    [Test]
    public async Task Given_ApplicationDetailAppealSend_NoPermission_Returns_ForbiddenResult()
    {
        //Arrange
        var id = "f41e59a2-9847-4084-9e17-0511e77571fb";
        var response = _fixture.Create<Task<ApplicationItemResponse>>();
        response.Result.Data.Establishment.Id = 123456;
        response.Result.Data.Id = "ddac4084-f9d7-4414-8d39-d07a24be82a2";

        _adminGatewayMock.Setup(x => x.GetApplication(It.IsAny<string>())).Returns(response);

        //act
        var result = await _sut.ApplicationDetailAppealSend(id);

        //assert 
        result.Should().BeOfType<ContentResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Test]
    public async Task ApplicationDetailAppealSend_Should_SendNotification_When_StatusUpdated()
    {
        // Arrange
        var id = "f41e59a2-9847-4084-9e17-0511e77571fb";
        var email = "parent@example.com";
        var reference = "FSM-12345";
        var parentFirstName = "Test";
        
        var applicationResponse = new ApplicationItemResponse
        {
            Data = new ApplicationResponse
            {
                Id = id,
                Reference = reference,
                ParentEmail = email,
                ParentFirstName = parentFirstName,
                Establishment = new ApplicationEstablishment
                {
                    Id = 123456,
                    Name = "Test School"
                }
            }
        };

        _adminGatewayMock.Setup(x => x.GetApplication(id)).ReturnsAsync(applicationResponse);
        _sendNotificationUseCaseMock.Setup(x => x.Execute(It.IsAny<NotificationRequest>()))
            .ReturnsAsync(new NotificationItemResponse());

        // Act
        var result = await _sut.ApplicationDetailAppealSend(id);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result as RedirectToActionResult;
        redirect.ActionName.Should().BeEquivalentTo("ApplicationDetailAppealConfirmationSent");

        // Verify status was updated
        _adminGatewayMock.Verify(x => x.PatchApplicationStatus(id, ApplicationStatus.SentForReview), Times.Once);
        
        // Verify notification was sent with correct data
        _sendNotificationUseCaseMock.Verify(x => x.Execute(It.Is<NotificationRequest>(req => 
            req.Data.Email == email && 
            req.Data.Type == NotificationType.ParentApplicationEvidenceSent && 
            req.Data.Personalisation.ContainsKey("reference") &&
            req.Data.Personalisation["reference"].ToString() == reference &&
            req.Data.Personalisation.ContainsKey("parentFirstName") &&
            req.Data.Personalisation["parentFirstName"].ToString() == parentFirstName
        )), Times.Once);
    }

    [Test]
    public async Task Given_FinaliseApplications_Results_Page_Returns_Valid_Data()
    {
        //arrange
        _sut.TempData = _tempData;
        var response = _fixture.Create<ApplicationSearchResponse>();

        _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch>()))
            .ReturnsAsync(response);

        var request = new ApplicationSearch();

        //act
        var result = await _sut.FinaliseApplications(0);

        //assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeAssignableTo<PeopleSelectionViewModel>();

        var model = viewResult.Model as PeopleSelectionViewModel;
        model.Should().NotBeNull();
    }

    [Test]
    public async Task Given_FinaliseApplications_Returns_No_Records_null()
    {
        //Arrange
        _sut.TempData = _tempData;
        _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch>()))
            .ReturnsAsync(default(ApplicationSearchResponse));

        var request = new ApplicationSearch();

        //act
        var result = await _sut.FinaliseApplications(0);

        //assert 
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        var resultData = viewResult.Model as PeopleSelectionViewModel;
    }


    [Test]
    public async Task Given_ApplicationDetailFinalise_Results_Page_Returns_Valid_Data()
    {
        //arrange
        var response = _fixture.Create<ApplicationItemResponse>();
        response.Data.ChildDateOfBirth = "2007-08-14";
        response.Data.ParentDateOfBirth = "2007-08-14";
        var claims = DfeSignInExtensions.GetDfeClaims(_httpContext.Object.User.Claims);
        response.Data.Establishment.Id = Convert.ToInt32(claims.Organisation.Urn);

        _adminGatewayMock.Setup(s => s.GetApplication(It.IsAny<string>()))
            .ReturnsAsync(response);

        var request = new ApplicationSearch();

        //act
        var result = await _sut.ApplicationDetailFinalise(response.Data.Id);

        //assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeAssignableTo<ApplicationDetailViewModel>();

        var model = viewResult.Model as ApplicationDetailViewModel;
        model.Should().NotBeNull();
    }

    [Test]
    public async Task Given_ApplicationDetailFinalise_Results_Returns_NotFound()
    {
        //arrange
        var response = _fixture.Create<ApplicationItemResponse>();
        var claims = DfeSignInExtensions.GetDfeClaims(_httpContext.Object.User.Claims);
        response.Data.Establishment.Id = Convert.ToInt32(claims.Organisation.Urn);

        _adminGatewayMock.Setup(s => s.GetApplication(It.IsAny<string>()))
            .ReturnsAsync(default(ApplicationItemResponse));

        var request = new ApplicationSearch();

        //act
        var result = await _sut.ApplicationDetailFinalise(response.Data.Id);

        //assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task Given_ApplicationDetailFinalise_Results_Returns_ForbiddenResult()
    {
        //arrange
        var response = _fixture.Create<ApplicationItemResponse>();
        response.Data.Establishment.Id = -99;

        _adminGatewayMock.Setup(s => s.GetApplication(It.IsAny<string>()))
            .ReturnsAsync(response);

        var request = new ApplicationSearch();

        //act
        var result = await _sut.ApplicationDetailFinalise(response.Data.Id);

        //assert
        result.Should().BeOfType<ContentResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Test]
    public async Task Given_FinaliseSelectedApplications_Returns_ViewResult()
    {
        //Arrange
        _sut.TempData = _tempData;
        var model = _fixture.Create<PeopleSelectionViewModel>();
        foreach (var item in model.People) item.Selected = true;
        var ids = model.getSelectedIds();
        //act
        var result = _sut.FinaliseSelectedApplications(model);

        //assert 
        result.Should().BeOfType<ViewResult>();
        var tempDataIds = _sut.TempData["FinaliseApplicationIds"];

        _sut.TempData["FinaliseApplicationIds"].Should().BeEquivalentTo(ids);
    }

    [Test]
    public async Task Given_ApplicationFinaliseSend_Returns_ViewResult()
    {
        //Arrange
        _sut.TempData = _tempData;
        var model = _fixture.Create<PeopleSelectionViewModel>();
        foreach (var item in model.People) item.Selected = true;
        var ids = model.getSelectedIds();
        _sut.TempData["FinaliseApplicationIds"] = model.getSelectedIds();
        _adminGatewayMock.Setup(x => x.PatchApplicationStatus(It.IsAny<string>(), It.IsAny<ApplicationStatus>()));
        //act
        var result = await _sut.ApplicationFinaliseSend();

        //assert 
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result as RedirectToActionResult;
        redirect.ActionName.Should().BeEquivalentTo("FinaliseApplications");
    }

    [Test]
    public async Task Given_FinalisedApplicationsdownload_Page_Returns_Valid_Data()
    {
        //arrange
        var response = _fixture.Create<ApplicationSearchResponse>();
        foreach (var item in response.Data)
        {
            item.ChildDateOfBirth = "1990-01-01";
            item.ParentDateOfBirth = "1990-01-01";
        }

        _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch>()))
            .ReturnsAsync(response);

        var request = new ApplicationSearch();

        //act
        var result = await _sut.FinalisedApplicationsdownload();

        //assert
        result.Should().BeOfType<FileStreamResult>();
    }


    [Test]
    public async Task Given_PendingApplications_Results_Page_Returns_Valid_Data()
    {
        //arrange
        _sut.TempData = _tempData;
        var response = _fixture.Create<ApplicationSearchResponse>();

        _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch>()))
            .ReturnsAsync(response);

        var request = new ApplicationSearch();

        //act
        var result = await _sut.PendingApplications(0);

        //assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeAssignableTo<PeopleSelectionViewModel>();

        var model = viewResult.Model as PeopleSelectionViewModel;
        model.Should().NotBeNull();
    }

    [Test]
    public async Task Given_PendingApplications_Returns_No_Records_null()
    {
        //Arrange
        _sut.TempData = _tempData;
        _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch>()))
            .ReturnsAsync(new ApplicationSearchResponse() {Data = new []{new ApplicationResponse()}, Meta = new ApplicationSearchResponseMeta()});

        var request = new ApplicationSearch();

        //act
        var result = await _sut.PendingApplications(0);

        //assert 
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        var resultData = viewResult.Model as PeopleSelectionViewModel;
    }


    [Test]
    public async Task Given_ApplicationDetailLa_Results_Page_Returns_Valid_Data()
    {
        //arrange
        var response = _fixture.Create<ApplicationItemResponse>();
        response.Data.ChildDateOfBirth = "2007-08-14";
        response.Data.ParentDateOfBirth = "2007-08-14";
        var claims = DfeSignInExtensions.GetDfeClaims(_httpContext.Object.User.Claims);
        response.Data.Establishment.Id = Convert.ToInt32(claims.Organisation.Urn);

        _adminGatewayMock.Setup(s => s.GetApplication(It.IsAny<string>()))
            .ReturnsAsync(response);

        var request = new ApplicationSearch();

        //act
        var result = await _sut.ApplicationDetailLa(response.Data.Id);

        //assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeAssignableTo<ApplicationDetailViewModel>();

        var model = viewResult.Model as ApplicationDetailViewModel;
        model.Should().NotBeNull();
    }

    [Test]
    public async Task Given_ApplicationDetailLa_Results_Returns_NotFound()
    {
        //arrange
        var response = _fixture.Create<ApplicationItemResponse>();
        var claims = DfeSignInExtensions.GetDfeClaims(_httpContext.Object.User.Claims);
        response.Data.Establishment.Id = Convert.ToInt32(claims.Organisation.Urn);

        _adminGatewayMock.Setup(s => s.GetApplication(It.IsAny<string>()))
            .ReturnsAsync(default(ApplicationItemResponse));

        var request = new ApplicationSearch();

        //act
        var result = await _sut.ApplicationDetailLa(response.Data.Id);

        //assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task Given_ApplicationDetailLa_Results_Returns_UnauthorizedResult()
    {
        //arrange
        var response = _fixture.Create<ApplicationItemResponse>();
        response.Data.Establishment.Id = -99;

        _adminGatewayMock.Setup(s => s.GetApplication(It.IsAny<string>()))
            .ReturnsAsync(response);

        var request = new ApplicationSearch();

        //act
        var result = await _sut.ApplicationDetailLa(response.Data.Id);

        //assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Test]
    public async Task Given_ApproveConfirmation_Returns_ViewResult()
    {
        //Arrange
        _sut.TempData = _tempData;
        var id = _fixture.Create<string>();
        //act
        var result = await _sut.ApproveConfirmation(id);

        //assert 
        result.Should().BeOfType<ViewResult>();
        _sut.TempData["AppApproveId"].Should().Be(id);
    }


    [Test]
    public async Task Given_DeclineConfirmation_Returns_ViewResult()
    {
        //Arrange
        _sut.TempData = _tempData;
        var id = _fixture.Create<string>();
        //act
        var result = await _sut.DeclineConfirmation(id);

        //assert 
        result.Should().BeOfType<ViewResult>();
        _sut.TempData["AppApproveId"].Should().Be(id);
    }

    [Test]
    public async Task Given_ApplicationApproveSend_Returns_ViewResult()
    {
        //Arrange
        var id = "f41e59a2-9847-4084-9e17-0511e77571fb";
        var response = _fixture.Create<Task<ApplicationItemResponse>>();
        response.Result.Data.Establishment.Id = 123456;
        response.Result.Data.Id = id;


        _adminGatewayMock.Setup(x => x.GetApplication(It.IsAny<string>())).Returns(response);
        //act
        var result = await _sut.ApplicationApproveSend(id);

        //assert 
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result as RedirectToActionResult;
        redirect.ActionName.Should().BeEquivalentTo("ApplicationApproved");
    }

    [Test]
    public async Task Given_ApplicationApproveSend_NoPermission_Returns_ForbiddenResult()
    {
        //Arrange
        var id = "f41e59a2-9847-4084-9e17-0511e77571fb";
        var response = _fixture.Create<Task<ApplicationItemResponse>>();
        response.Result.Data.Establishment.Id = 123456;
        response.Result.Data.Id = "ddac4084-f9d7-4414-8d39-d07a24be82a2";

        _adminGatewayMock.Setup(x => x.GetApplication(It.IsAny<string>())).Returns(response);
        //Act
        var result = await _sut.ApplicationApproveSend(id);

        //Assert
        result.Should().BeOfType<ContentResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Test]
    public async Task Given_ApplicationDeclineSend_Returns_ViewResult()
    {
        //Arrange
        var id = "f41e59a2-9847-4084-9e17-0511e77571fb";
        var response = _fixture.Create<Task<ApplicationItemResponse>>();
        response.Result.Data.Establishment.Id = 123456;
        response.Result.Data.Id = id;


        _adminGatewayMock.Setup(x => x.GetApplication(It.IsAny<string>())).Returns(response);
        //act
        var result = await _sut.ApplicationDeclineSend(id);

        //assert 
        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = result as RedirectToActionResult;
        redirect.ActionName.Should().BeEquivalentTo("ApplicationDeclined");
    }

    [Test]
    public async Task Given_ApplicationDeclineSend_NoPermission_Returns_ForbiddenResult()
    {
        //Arrange
        var id = "f41e59a2-9847-4084-9e17-0511e77571fb";
        var response = _fixture.Create<Task<ApplicationItemResponse>>();
        response.Result.Data.Establishment.Id = 123456;
        response.Result.Data.Id = "ddac4084-f9d7-4414-8d39-d07a24be82a2";

        _adminGatewayMock.Setup(x => x.GetApplication(It.IsAny<string>())).Returns(response);
        //Act
        var result = await _sut.ApplicationDeclineSend(id);

        //Assert
        result.Should().BeOfType<ContentResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }
}