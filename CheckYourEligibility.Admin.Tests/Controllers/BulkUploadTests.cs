using CheckYourEligibility.Domain.Enums;
using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Domain.Responses;
using CheckYourEligibility.Admin.Controllers;
using CheckYourEligibility.Admin.Gateways;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Tests.Properties;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility.Admin.Tests.Controllers
{
    [TestFixture]
    public class BulkUploadTests : TestBase
    {
        // mocks
        private ILogger<BulkCheckController> _loggerMock;
        private Mock<ICheckGateway> _checkGatewayMock;

        // system under test
        private BulkCheckController _sut;

        [SetUp]
        public void SetUp()
        {
            _checkGatewayMock = new Mock<ICheckGateway>();
            _loggerMock = Mock.Of<ILogger<BulkCheckController>>();
            _sut = new BulkCheckController(_loggerMock, _checkGatewayMock.Object, _configMock.Object);

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

            // Act
            var result = await _sut.Bulk_Check(file);

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

            // Act
            var result = await _sut.Bulk_Check(file);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().BeEquivalentTo("BulkOutcome/Error_Data_Issue");
            string output = viewResult.TempData["BulkParentCheckItemsErrors"].ToString();
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

            // Act
            var result = await _sut.Bulk_Check(file);

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
                    Links = new CheckEligibilityResponseBulkLinks { Get_BulkCheck_Results = "someUrl", Get_Progress_Check = "someUrl" }
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

            // Act
            var result = await _sut.Bulk_Check(file);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var viewResult = result as RedirectToActionResult;
            viewResult.ActionName.Should().BeEquivalentTo("Bulk_Loader");

        }


        [Test]
        public async Task Given_Loader_When_LoadingPage_Should_return_LoadLoaderPage()
        {
            // Act
            var result = await _sut.Bulk_Loader();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeNull();
        }

        [Test]
        public async Task Given_Loader_When_Results_Should_return_redirect()
        {
            //Arrange
            //  HttpContext.Session.SetString("Get_Progress_Check", result.Links.Get_Progress_Check);
            var response =
               new CheckEligibilityBulkStatusResponse { Data = new BulkStatus { Complete = 10, Total = 10 } };

            _checkGatewayMock.Setup(s => s.GetBulkCheckProgress(It.IsAny<string>()))
                   .ReturnsAsync(response);

            // Act

            var result = await _sut.Bulk_Loader();

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var viewResult = result as RedirectToActionResult;
            viewResult.ActionName.Should().BeEquivalentTo("Bulk_check_success");
        }

        [Test]
        public async Task Given_Bulk_check_success_When_LoadingPage_Should_return_Bulk_check_success()
        {
            // Act
            var result = await _sut.Bulk_check_success();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.ViewName.Should().BeEquivalentTo("BulkOutcome/Success");
        }

        [Test]
        public async Task Given_Bulk_check_download_When_LoadingPage_Should_return_csvFile()
        {
            //arrange
            var response =
              new CheckEligibilityBulkResponse
              {
                  Data = new List<CheckEligibilityItem>() {
                  new CheckEligibilityItem() {Status = CheckEligibilityStatus.eligible.ToString() } }
              };

            _checkGatewayMock.Setup(s => s.GetBulkCheckResults(It.IsAny<string>()))
                   .ReturnsAsync(response);
            // Act
            var result = await _sut.Bulk_check_download();

            // Assert
            result.Should().BeOfType<FileStreamResult>();
            var viewResult = result as FileStreamResult;
            viewResult.ContentType.Should().BeEquivalentTo("text/csv");
        }


        [TestCase]
        public async Task Given_11_Successive_Bulk_Checks_In_1_Hour_11th_Check_Returns_Error()
        {
            // arrange
            var response = new CheckEligibilityResponseBulk
            {
                Data = new StatusValue { Status = "processing" },
                Links = new CheckEligibilityResponseBulkLinks { Get_BulkCheck_Results = "someUrl", Get_Progress_Check = "someUrl" }
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
            for (int i = 0; i < 10; i++)
            {
                var result = await _sut.Bulk_Check(file);
                result.Should().BeOfType<RedirectToActionResult>();
                var viewResult = result as RedirectToActionResult;
                viewResult.ActionName.Should().BeEquivalentTo("Bulk_Loader");

                if (i == 10)
                {
                    // assert
                    viewResult.ActionName.Should().BeEquivalentTo("Bulk_Check");
                }
            }
        }
        [Test]
        public async Task Given_Bulk_Check_When_FileIsValid_Should_ReturnBulkLoaderPage()
        {
            // Arrange
            var response = new CheckEligibilityResponseBulk
            {
                Data = new StatusValue { Status = "processing" },
                Links = new CheckEligibilityResponseBulkLinks { Get_BulkCheck_Results = "someUrl", Get_Progress_Check = "someUrl" }
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
            var result = await _sut.Bulk_Check(file);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var viewResult = result as RedirectToActionResult;
            viewResult.ActionName.Should().BeEquivalentTo("Bulk_Loader");
        }
        [Test]
        public async Task Given_Bulk_Check_When_FileHasTooManyRecords_Should_ReturnBulkCheckPage()
        {
            // Arrange
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
            var result = await _sut.Bulk_Check(file);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var viewResult = result as RedirectToActionResult;
            viewResult.ActionName.Should().BeEquivalentTo("Bulk_Check");
            _sut.TempData["ErrorMessage"].Should().BeEquivalentTo("CSV File cannot contain more than 250 records");
        }
    }
}

