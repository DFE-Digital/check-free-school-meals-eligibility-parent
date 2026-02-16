using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.Usecases;
using Microsoft.Extensions.Logging;
using Moq;

namespace CheckYourEligibility.Admin.Tests.Usecases;

[TestFixture]
public class GetBulkCheckStatusesUseCase_FsmBasicTests
{
    private Mock<ILogger<GetBulkCheckStatusesUseCase_FsmBasic>> _loggerMock;
    private Mock<ICheckGateway> _checkGatewayMock;
    private GetBulkCheckStatusesUseCase_FsmBasic _useCase;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<GetBulkCheckStatusesUseCase_FsmBasic>>();
        _checkGatewayMock = new Mock<ICheckGateway>();
        _useCase = new GetBulkCheckStatusesUseCase_FsmBasic(_loggerMock.Object, _checkGatewayMock.Object);
    }

    [Test]
    public async Task Execute_WithValidResponse_ReturnsBulkChecks()
    {
        // Arrange
        var organisationId = "123456";
        var apiResponse = new CheckEligibilityBulkProgressByLAResponse
        {
            Checks = new List<CheckEligibilityBulkProgressResponse>
            {
                new CheckEligibilityBulkProgressResponse
                {
                    Id = "guid-1",
                    Filename = "test1.csv",
                    NumberOfRecords = 25,
                    Status = "Completed",
                    SubmittedDate = DateTime.Now.AddDays(-1),
                    SubmittedBy = "user@test.com",
                    EligibilityType = "FreeSchoolMeals",
                    FinalNameInCheck = "Smith"
                },
                new CheckEligibilityBulkProgressResponse
                {
                    Id = "guid-2",
                    Filename = "test2.csv",
                    NumberOfRecords = 50,
                    Status = "InProgress",
                    SubmittedDate = DateTime.Now.AddDays(-2),
                    SubmittedBy = "user2@test.com",
                    EligibilityType = "FreeSchoolMeals",
                    FinalNameInCheck = "Jones"
                }
            }
        };

        _checkGatewayMock
            .Setup(x => x.GetBulkCheckStatuses_FsmBasic(organisationId))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _useCase.Execute(organisationId);

        // Assert
        var resultList = result.ToList();
        Assert.That(resultList.Count, Is.EqualTo(2));
        Assert.That(resultList[0].BulkCheckId, Is.EqualTo("guid-1"));
        Assert.That(resultList[0].Filename, Is.EqualTo("test1.csv"));
        Assert.That(resultList[0].NumberOfRecords, Is.EqualTo(25));
        Assert.That(resultList[0].FinalNameInCheck, Is.EqualTo("Smith"));
        Assert.That(resultList[0].Status, Is.EqualTo("Completed"));
    }

    [Test]
    public async Task Execute_FiltersOnlyFreeSchoolMeals()
    {
        // Arrange
        var organisationId = "123456";
        var apiResponse = new CheckEligibilityBulkProgressByLAResponse
        {
            Checks = new List<CheckEligibilityBulkProgressResponse>
            {
                new CheckEligibilityBulkProgressResponse
                {
                    Id = "guid-1",
                    Filename = "fsm.csv",
                    EligibilityType = "FreeSchoolMeals",
                    Status = "Completed",
                    SubmittedDate = DateTime.Now
                },
                new CheckEligibilityBulkProgressResponse
                {
                    Id = "guid-2",
                    Filename = "2yo.csv",
                    EligibilityType = "TwoYearOffer", // Should be filtered out
                    Status = "Completed",
                    SubmittedDate = DateTime.Now
                },
                new CheckEligibilityBulkProgressResponse
                {
                    Id = "guid-3",
                    Filename = "eypp.csv",
                    EligibilityType = "EarlyYearPupilPremium", // Should be filtered out
                    Status = "Completed",
                    SubmittedDate = DateTime.Now
                }
            }
        };

        _checkGatewayMock
            .Setup(x => x.GetBulkCheckStatuses_FsmBasic(organisationId))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _useCase.Execute(organisationId);

        // Assert
        var resultList = result.ToList();
        Assert.That(resultList.Count, Is.EqualTo(1));
        Assert.That(resultList[0].Filename, Is.EqualTo("fsm.csv"));
        Assert.That(resultList[0].EligibilityType, Is.EqualTo("FreeSchoolMeals"));
    }

    [Test]
    public async Task Execute_MapsStatusCorrectly()
    {
        // Arrange
        var organisationId = "123456";
        var apiResponse = new CheckEligibilityBulkProgressByLAResponse
        {
            Checks = new List<CheckEligibilityBulkProgressResponse>
            {
                new CheckEligibilityBulkProgressResponse
                {
                    Id = "guid-1",
                    Status = "completed",
                    EligibilityType = "FreeSchoolMeals",
                    SubmittedDate = DateTime.Now
                },
                new CheckEligibilityBulkProgressResponse
                {
                    Id = "guid-2",
                    Status = "inprogress",
                    EligibilityType = "FreeSchoolMeals",
                    SubmittedDate = DateTime.Now
                },
                new CheckEligibilityBulkProgressResponse
                {
                    Id = "guid-3",
                    Status = "notstarted",
                    EligibilityType = "FreeSchoolMeals",
                    SubmittedDate = DateTime.Now
                },
                new CheckEligibilityBulkProgressResponse
                {
                    Id = "guid-4",
                    Status = "failed",
                    EligibilityType = "FreeSchoolMeals",
                    SubmittedDate = DateTime.Now
                }
            }
        };

        _checkGatewayMock
            .Setup(x => x.GetBulkCheckStatuses_FsmBasic(organisationId))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _useCase.Execute(organisationId);

        // Assert
        var resultList = result.ToList();
        Assert.That(resultList[0].Status, Is.EqualTo("Completed"));
        Assert.That(resultList[1].Status, Is.EqualTo("In progress"));
        Assert.That(resultList[2].Status, Is.EqualTo("Not started"));
        Assert.That(resultList[3].Status, Is.EqualTo("Failed"));
    }

    [Test]
    public async Task Execute_WhenNullResponse_ReturnsEmptyList()
    {
        // Arrange
        var organisationId = "123456";
        _checkGatewayMock
            .Setup(x => x.GetBulkCheckStatuses_FsmBasic(organisationId))
            .ReturnsAsync((CheckEligibilityBulkProgressByLAResponse)null);

        // Act
        var result = await _useCase.Execute(organisationId);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task Execute_WhenNullChecks_ReturnsEmptyList()
    {
        // Arrange
        var organisationId = "123456";
        var apiResponse = new CheckEligibilityBulkProgressByLAResponse { Checks = null };

        _checkGatewayMock
            .Setup(x => x.GetBulkCheckStatuses_FsmBasic(organisationId))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _useCase.Execute(organisationId);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task Execute_WithNullableFields_HandlesGracefully()
    {
        // Arrange
        var organisationId = "123456";
        var apiResponse = new CheckEligibilityBulkProgressByLAResponse
        {
            Checks = new List<CheckEligibilityBulkProgressResponse>
            {
                new CheckEligibilityBulkProgressResponse
                {
                    Id = "guid-1",
                    Filename = "test.csv",
                    NumberOfRecords = null, // Optional - not provided
                    FinalNameInCheck = null, // Optional - not provided
                    Status = "Completed",
                    SubmittedDate = DateTime.Now,
                    EligibilityType = "FreeSchoolMeals"
                }
            }
        };

        _checkGatewayMock
            .Setup(x => x.GetBulkCheckStatuses_FsmBasic(organisationId))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _useCase.Execute(organisationId);

        // Assert
        var resultList = result.ToList();
        Assert.That(resultList.Count, Is.EqualTo(1));
        Assert.That(resultList[0].NumberOfRecords, Is.Null);
        Assert.That(resultList[0].FinalNameInCheck, Is.Null);
    }
}
