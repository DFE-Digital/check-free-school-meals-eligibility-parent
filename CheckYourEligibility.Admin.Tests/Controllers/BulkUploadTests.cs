using AutoFixture;
using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Controllers;
using CheckYourEligibility.Admin.Domain.Constants.BulkCheck;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Domain.Enums;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.Tests.Properties;
using CheckYourEligibility.Admin.Usecases;
using CheckYourEligibility.Admin.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility.Admin.Tests.Controllers;

[TestFixture]
public class BulkUploadTests : TestBase
{
    [SetUp]
    public void SetUp()
    {
        _schoolMenuContextResolverMock = new Mock<ISchoolMenuContextResolver>();
        _schoolMenuContextResolverMock
            .Setup(x => x.ResolveAsync(It.IsAny<DfeClaims>()))
            .ReturnsAsync(new SchoolMenuContext());
        _localAuthoritySettingsGatewayMock = new Mock<ILocalAuthoritySettingsGateway>();

        _checkGatewayMock = new Mock<ICheckGateway>();
        _loggerMock = Mock.Of<ILogger<BulkCheckController>>();
        _dfeSignInApiServiceCaseMock = new Mock<IDfeSignInApiService>();
        _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
        _parseBulkCheckFileUseCaseMock = new Mock<IParseBulkCheckFileUseCase>();
        _getBulkCheckStatusesUseCaseMock = new Mock<IGetBulkCheckStatusesUseCase>();
        _deleteBulkCheckFileUseCaseMock = new Mock<IDeleteBulkCheckFileUseCase>();


    _sut = new BulkCheckController(
            _loggerMock,
            _checkGatewayMock.Object,
            _configMock.Object,
            _webHostEnvironmentMock.Object,
            _parseBulkCheckFileUseCaseMock.Object,
            _getBulkCheckStatusesUseCaseMock.Object,
            _deleteBulkCheckFileUseCaseMock.Object,
            _dfeSignInApiServiceCaseMock.Object,
            _schoolMenuContextResolverMock.Object,
            _localAuthoritySettingsGatewayMock.Object);
        base.SetUp();
		_sut.ControllerContext.HttpContext = _httpContext.Object;
		_sut.GetDfeClaimsAsync().Wait();
        _sut.TempData = _tempData;        
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
    }

    // mocks
    private ILogger<BulkCheckController> _loggerMock;
    private Mock<ICheckGateway> _checkGatewayMock;
    private Mock<IDfeSignInApiService> _dfeSignInApiServiceCaseMock;
    private Mock<ISchoolMenuContextResolver> _schoolMenuContextResolverMock;
    private Mock<ILocalAuthoritySettingsGateway> _localAuthoritySettingsGatewayMock;
    private Mock<IWebHostEnvironment> _webHostEnvironmentMock;
    private Mock<IParseBulkCheckFileUseCase> _parseBulkCheckFileUseCaseMock;
    private Mock<IGetBulkCheckStatusesUseCase> _getBulkCheckStatusesUseCaseMock;
    private Mock<IDeleteBulkCheckFileUseCase> _deleteBulkCheckFileUseCaseMock;

    // system under test
    private BulkCheckController _sut;


    [Test]
    public async Task Given_Bulk_Check_Should_Load_BulkCheckPage()
    {
        // Act
        var result = _sut.Bulk_Check();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.Model.Should().BeNull();
    }

    [Test]
    public async Task Given_Bulk_Check_When_FileData_Invalid_Should_Return_Error_Data_Issue()
    {
        // Arrange
        var content = Resources.bulkchecktemplate_some_invalid_items;
        var fileName = "test.csv";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        //create FormFile with desired data
        var file = new FormFile(stream, 0, stream.Length, fileName, fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };

        var viewModel = new BulkCheckUploadViewModel
        {
            isSchool = false,
            isEnhanced = false,
            GuidanceItems = BulkCheckUploadConstants.GuidanceItemsBasic
        };
        // Act
        var result = await _sut.Bulk_Check(file,viewModel);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().BeEquivalentTo("BulkOutcome/Error_Data_Issue");
        viewResult.TempData["BulkParentCheckItemsErrors"].Should().NotBeNull();
    }

    [Test]
    public async Task Given_Bulk_Check_When_FileData_Empty_Should_Return_Error_Data_Issue()
    {
        // Arrange
        var content = "";
        var fileName = "test.csv";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        //create FormFile with desired data
        var file = new FormFile(stream, 0, stream.Length, fileName, fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };

        var viewModel = new BulkCheckUploadViewModel
        {
            isSchool = false,
            isEnhanced = false,
            GuidanceItems = BulkCheckUploadConstants.GuidanceItemsBasic
        };
        // Act
        var result = await _sut.Bulk_Check(file, viewModel);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().BeEquivalentTo("BulkOutcome/Error_Data_Issue");
        var output = viewResult.TempData["BulkParentCheckItemsErrors"].ToString();
        output.Replace("\r", "").Replace("\n", "").Should().BeEquivalentTo("Invalid file content.");
    }

    [Test]
    public async Task Given_Bulk_Check_When_FileType_Invalid_Should_Return_RedirectToActionResult()
    {
        // Arrange
        var content = "";
        var fileName = "test.xls";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        //create FormFile with desired data
        var file = new FormFile(stream, 0, stream.Length, fileName, fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/xls"
        };
        var viewModel = new BulkCheckUploadViewModel
        {
            isSchool = false,
            isEnhanced = false,
            GuidanceItems = BulkCheckUploadConstants.GuidanceItemsBasic
        };

        // Act
        var result = await _sut.Bulk_Check(file, viewModel);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Test]
    public async Task Given_Bulk_Check_When_FileData_Valid_Should_Return_ValidData()
    {
        // Arrange
        var response =
            new CheckEligibilityResponseBulk
            {
                Data = new StatusValue { Status = "processing" },
                Links = new CheckEligibilityResponseBulkLinks
                { Get_BulkCheck_Results = "someUrl", Get_Progress_Check = "someUrl" }
            };
        _checkGatewayMock.Setup(s => s.PostBulkCheck(It.IsAny<CheckEligibilityRequestBulk_Fsm>()))
            .ReturnsAsync(response);

        var content = Resources.bulkchecktemplate_small_Valid;
        var fileName = "test.csv";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        //create FormFile with desired data
        var file = new FormFile(stream, 0, stream.Length, fileName, fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };
        var viewModel = new BulkCheckUploadViewModel
        {
            isSchool = false,
            isEnhanced = false,
            GuidanceItems = BulkCheckUploadConstants.GuidanceItemsBasic
        };

        // Act
        var result = await _sut.Bulk_Check(file, viewModel);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var viewResult = result as RedirectToActionResult;
        viewResult.ActionName.Should().BeEquivalentTo("Bulk_Loader");
    }

    [TestCase]
    public async Task Given_11_Successive_Bulk_Checks_In_1_Hour_11th_Check_Returns_Error()
    {
        // arrange
        var response = new CheckEligibilityResponseBulk
        {
            Data = new StatusValue { Status = "processing" },
            Links = new CheckEligibilityResponseBulkLinks
            { Get_BulkCheck_Results = "someUrl", Get_Progress_Check = "someUrl" }
        };

        _checkGatewayMock.Setup(
                s => s.PostBulkCheck(It.IsAny<CheckEligibilityRequestBulk_Fsm>()))
            .ReturnsAsync(response);

        _sut.TempData["ErrorMessage"] = "No more than 10 batch check requests can be made per hour";
        var content = Resources.bulkchecktemplate_small_Valid;

        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        //create FormFile with desired data
        var file = new FormFile(stream, 0, stream.Length, "test.csv", "test.csv")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };

        //act
        for (var i = 0; i < 10; i++)
        {
            var viewModel = _fixture.Create<BulkCheckUploadViewModel>();
            var result = await _sut.Bulk_Check(file,viewModel);
            result.Should().BeOfType<RedirectToActionResult>();
            var viewResult = result as RedirectToActionResult;
            viewResult.ActionName.Should().BeEquivalentTo("Bulk_Loader");

            if (i == 10)
                // assert
                viewResult.ActionName.Should().BeEquivalentTo("Bulk_Check");
        }
    }

    [Test]
    public async Task Given_Bulk_Check_When_FileIsValid_Should_ReturnBulkLoaderPage()
    {
        // Arrange
        var viewModel = _fixture.Create<BulkCheckUploadViewModel>();
        var response = new CheckEligibilityResponseBulk
        {
            Data = new StatusValue { Status = "processing" },
            Links = new CheckEligibilityResponseBulkLinks
            { Get_BulkCheck_Results = "someUrl", Get_Progress_Check = "someUrl" }
        };

        _checkGatewayMock.Setup(
                s => s.PostBulkCheck(It.IsAny<CheckEligibilityRequestBulk_Fsm>()))
            .ReturnsAsync(response);

        var content = Resources.bulkchecktemplate_small_Valid;

        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        var file = new FormFile(stream, 0, stream.Length, "test.csv", "test.csv")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };

        // Act
        var result = await _sut.Bulk_Check(file, viewModel);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var viewResult = result as RedirectToActionResult;
        viewResult.ActionName.Should().BeEquivalentTo("Bulk_Loader");
    }

    [Test]
    public async Task Given_Bulk_Check_When_FileHasTooManyRecords_Should_ReturnBulkCheckPage()
    {


        // Arrange
        var viewModel = _fixture.Create<BulkCheckUploadViewModel>();
        var content = Resources.bulkchecktemplate_too_many_records;
        _sut.TempData["ErrorMessage"] = "CSV File cannot contain more than 250 records";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        var file = new FormFile(stream, 0, stream.Length, "test.csv", "test.csv")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };

        // Act
        var result = await _sut.Bulk_Check(file, viewModel);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var viewResult = result as RedirectToActionResult;
        viewResult.ActionName.Should().BeEquivalentTo("Bulk_Check");
        _sut.TempData["ErrorMessage"].Should().BeEquivalentTo("CSV File cannot contain more than 250 records");
    }
}