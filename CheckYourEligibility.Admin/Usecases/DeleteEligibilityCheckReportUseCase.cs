using CheckYourEligibility.Admin.Gateways.Interfaces;

namespace CheckYourEligibility.Admin.Usecases
{
    public interface IDeleteEligibilityCheckReportUseCase
    {
        Task Execute(string reportId);
    }

    public class DeleteEligibilityCheckReportUseCase : IDeleteEligibilityCheckReportUseCase
    {
        private readonly IEligibilityCheckReportingGateway _gateway;

        public DeleteEligibilityCheckReportUseCase(IEligibilityCheckReportingGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task Execute(string reportId)
        {
            await _gateway.DeleteEligibilityCheckReport(reportId);
        }
    }
}
