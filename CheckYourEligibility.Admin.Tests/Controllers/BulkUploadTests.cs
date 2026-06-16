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
using static CheckYourEligibility.Admin.Helpers.CsvBulkCheckValidatorHelper;

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
        var parseResult = new BulkCheckCsvResult<CheckEligibilityRequestDataBase>
        {
            ErrorMessage = string.Empty,
            ValidRequests = new List<CheckEligibilityRequestDataBase>
            {
                new CheckEligibilityRequestDataBase
                {
                    LastName = "Smith",
                    DateOfBirth = "1985-03-15",
                    NationalInsuranceNumber = "AB123456C",
                    Sequence = 1
                }
            }
        };

        _parseBulkCheckFileUseCaseMock
            .Setup(p => p.Execute<CheckEligibilityRequestDataBase>(
                It.IsAny<Stream>(),
                It.IsAny<Func<CsvHelper.IReaderRow, int, CheckEligibilityRequestDataBase>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
            .ReturnsAsync(parseResult);

        var response =
            new CheckEligibilityResponseBulk
            {
                Data = new StatusValue { Status = "processing" },
                Links = new CheckEligibilityResponseBulkLinks
                { Get_BulkCheck_Results = "someUrl", Get_Progress_Check = "someUrl", Get_BulkCheck_Status = "someUrl" }
            };
        _checkGatewayMock.Setup(s => s.PostBulkCheck_FsmBasic(It.IsAny<CheckEligibilityRequestBulk>()))
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
        viewResult.ActionName.Should().BeEquivalentTo("Bulk_Check_History");
    }

    [TestCase]
    public async Task Given_11_Successive_Bulk_Checks_In_1_Hour_11th_Check_Returns_Error()
    {
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
        // Arrange
        var parseResult = new BulkCheckCsvResult<CheckEligibilityRequestDataBase>
        {
            ErrorMessage = string.Empty,
            ValidRequests = new List<CheckEligibilityRequestDataBase>
            {
                new CheckEligibilityRequestDataBase
                {
                    LastName = "Smith",
                    DateOfBirth = "1985-03-15",
                    NationalInsuranceNumber = "AB123456C",
                    Sequence = 1
                }
            }
        };

        _parseBulkCheckFileUseCaseMock
            .Setup(p => p.Execute<CheckEligibilityRequestDataBase>(
                It.IsAny<Stream>(),
                It.IsAny<Func<CsvHelper.IReaderRow, int, CheckEligibilityRequestDataBase>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
            .ReturnsAsync(parseResult);

        var response =
            new CheckEligibilityResponseBulk
            {
                Data = new StatusValue { Status = "processing" },
                Links = new CheckEligibilityResponseBulkLinks
                { Get_BulkCheck_Results = "someUrl", Get_Progress_Check = "someUrl", Get_BulkCheck_Status = "someUrl" }
            };
        _checkGatewayMock.Setup(s => s.PostBulkCheck_FsmBasic(It.IsAny<CheckEligibilityRequestBulk>()))
            .ReturnsAsync(response);


        //act
        for (var i = 0; i < 10; i++)
        {       
            var result = await _sut.Bulk_Check(file,viewModel);
            result.Should().BeOfType<RedirectToActionResult>();
            var viewResult = result as RedirectToActionResult;
            viewResult.ActionName.Should().BeEquivalentTo("Bulk_Check_History");

            if (i == 10)
                // assert
                viewResult.ActionName.Should().BeEquivalentTo("Bulk_Check");
        }
    }
    [Test]
    [TestCase("CSV File cannot contain more than 250 records")]
    public async Task Given_Bulk_Check_When_FileHasTooManyRecords_Should_ReturnBulkCheckPage(string errorMessage)
    {


        // Arrange
        var viewModel = _fixture.Create<BulkCheckUploadViewModel>();
        var content = Resources.bulkchecktemplate_too_many_records;
        // arrange mock parse result to simulate too many records
        var parseResult = new BulkCheckCsvResult<CheckEligibilityRequestDataBase>
        {
            ErrorMessage = errorMessage
        };

        _parseBulkCheckFileUseCaseMock
            .Setup(p => p.Execute<CheckEligibilityRequestDataBase>(
                It.IsAny<Stream>(),
                It.IsAny<Func<CsvHelper.IReaderRow, int, CheckEligibilityRequestDataBase>>(),
                It.IsAny<string[]>(),
                It.IsAny<bool>()))
            .ReturnsAsync(parseResult);

        _sut.TempData["ErrorMessage"] = errorMessage;
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
        _sut.TempData["ErrorMessage"].Should().BeEquivalentTo(errorMessage);
    }
}