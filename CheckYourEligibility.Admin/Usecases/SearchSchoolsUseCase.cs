using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Gateways.Interfaces;

namespace CheckYourEligibility.Admin.UseCases;

public interface ISearchSchoolsUseCase
{
    Task<IEnumerable<Establishment>> Execute(string query, string la);
}

public class SearchSchoolsUseCase : ISearchSchoolsUseCase
{
    private readonly IParentGateway _parentGatewayService;

    public SearchSchoolsUseCase(IParentGateway parentGatewayService)
    {
        _parentGatewayService = parentGatewayService ?? throw new ArgumentNullException(nameof(parentGatewayService));
    }

    public async Task<IEnumerable<Establishment>> Execute(string query, string la)
    {
        if (string.IsNullOrEmpty(query) || query.Length < 3)
            throw new ArgumentException("Query must be at least 3 characters long.", nameof(query));

        var results = await _parentGatewayService.GetSchool(query, la);
        return results?.Data ?? new List<Establishment>();
    }
}