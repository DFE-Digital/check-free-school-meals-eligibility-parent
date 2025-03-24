using AutoFixture;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility.Admin.Controllers;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.ViewModels;
using CheckYourEligibility.Admin.Gateways;
using CheckYourEligibility.Admin.Domain;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility.Admin.Tests.Controllers
{
    [TestFixture]
    public class ApplicationControllerTests : TestBase
    {
        //mocks
        private ILogger<ApplicationController> _loggerMock;
        private Mock<IAdminGateway> _adminGatewayMock;

        // system under test
        private ApplicationController _sut;

        [SetUp]
        public void SetUp()
        {
            _adminGatewayMock = new Mock<IAdminGateway>();
            _loggerMock = Mock.Of<ILogger<ApplicationController>>();
            _sut = new ApplicationController(_loggerMock, _adminGatewayMock.Object);

            base.SetUp();
            _sut.ControllerContext.HttpContext = _httpContext.Object;
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        #region search

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
        public async Task Given_Application_Search_Results_Page_Returns_Valid_Data()
        {
            //arrange
            _sut.TempData = _tempData;
            var response = _fixture.Create<ApplicationSearchResponse>();

            _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch2>()))
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

        #endregion

        #region school appeals


        [Test]
        public async Task Given_Process_Appeals_Results_Page_Returns_Valid_Data()
        {
            //arrange
            _sut.TempData = _tempData;
            var response = _fixture.Create<ApplicationSearchResponse>();

            _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch2>()))
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
            _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch2>()))
                .ReturnsAsync(default(ApplicationSearchResponse));

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

        #endregion

        #region school finalise


        [Test]
        public async Task Given_FinaliseApplications_Results_Page_Returns_Valid_Data()
        {
            //arrange
            _sut.TempData = _tempData;
            var response = _fixture.Create<ApplicationSearchResponse>();

            _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch2>()))
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
            _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch2>()))
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
            foreach (var item in model.People)
            {
                item.Selected = true;
            }
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
            foreach (var item in model.People)
            {
                item.Selected = true;
            }
            var ids = model.getSelectedIds();
            _sut.TempData["FinaliseApplicationIds"] = model.getSelectedIds();
            _adminGatewayMock.Setup(x=>x.PatchApplicationStatus(It.IsAny<string>(), It.IsAny<ApplicationStatus>()));
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

            _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch2>()))
                   .ReturnsAsync(response);

            var request = new ApplicationSearch();

            //act
            var result = await _sut.FinalisedApplicationsdownload();

            //assert
            result.Should().BeOfType<FileStreamResult>();

        }

        #endregion


        #region la pending applications


        [Test]
        public async Task Given_PendingApplications_Results_Page_Returns_Valid_Data()
        {
            //arrange
            _sut.TempData = _tempData;
            var response = _fixture.Create<ApplicationSearchResponse>();

            _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch2>()))
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
            _adminGatewayMock.Setup(s => s.PostApplicationSearch(It.IsAny<ApplicationRequestSearch2>()))
                .ReturnsAsync(default(ApplicationSearchResponse));

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
        #endregion
    }
}
