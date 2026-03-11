using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;

public interface IGenerateEligibilityCheckReportUseCase
{
    Task<EligibilityCheckReportResponse> Execute(EligibilityCheckReportRequest request);
}

public class GenerateEligibilityCheckReportUseCase : IGenerateEligibilityCheckReportUseCase
{
    private readonly ICheckGateway _gateway;

    public GenerateEligibilityCheckReportUseCase(ICheckGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<EligibilityCheckReportResponse> Execute(EligibilityCheckReportRequest request)
    {
        return await _gateway.GenerateEligibilityCheckReport(request);
    }
}
