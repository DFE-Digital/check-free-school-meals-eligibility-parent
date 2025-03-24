using CheckYourEligibility.Domain.Requests;
using CheckYourEligibility.Admin.Gateways;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using CheckYourEligibility.Admin.Gateways.Interfaces;
using CheckYourEligibility.Admin.Infrastructure;

namespace CheckYourEligibility.Admin.UseCases
{
    public interface ICreateUserUseCase
    {
        Task<string> Execute(IEnumerable<Claim> claims);
    }

    public class CreateUserResult
    {
        public bool IsSuccess { get; set; }
        public string? UserId { get; set; }
        public string? ErrorMessage { get; set; }

        public static CreateUserResult Success(string userId) =>
            new() { IsSuccess = true, UserId = userId };

        public static CreateUserResult Error(string message) =>
            new() { IsSuccess = false, ErrorMessage = message };
    }

    [Serializable]
    public class CreateUserException : Exception
    {
        public CreateUserException(string message) : base(message) { }
    }

    public class CreateUserUseCase : ICreateUserUseCase
    {
        private readonly ILogger<CreateUserUseCase> _logger;
        private readonly IParentGateway _parentGateway;

        public CreateUserUseCase(
            ILogger<CreateUserUseCase> logger,
            IParentGateway parentService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parentGateway = parentService ?? throw new ArgumentNullException(nameof(parentService));
        }

        public async Task<string> Execute(IEnumerable<Claim> claims)
        {
            try
            {
                var dfeClaims = DfeSignInExtensions.GetDfeClaims(claims);
                if (dfeClaims?.User == null)
                {
                    throw new CreateUserException("DFE user claims not found");
                }

                var userRequest = new UserCreateRequest
                {
                    Data = new UserData
                    {
                        Email = dfeClaims.User.Email,
                        Reference = dfeClaims.User.Id
                    }
                };

                _logger.LogInformation("Creating user with email {Email}", dfeClaims.User.Email);

                var response = await _parentGateway.CreateUser(userRequest);
                if (response?.Data == null)
                {
                    throw new CreateUserException("User creation response was null");
                }

                return response.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user");
                throw new CreateUserException($"Failed to create user: {ex.Message}");
            }
        }
    }
}