// using CheckYourEligibility.Admin.Domain;

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Boundary.Responses;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Infrastructure;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;

namespace CheckYourEligibility.Admin.Gateways;

public class BaseGateway
{
    private static JwtAuthResponse _jwtAuthResponse;
    protected readonly IConfiguration _configuration;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly TelemetryClient _telemetry;
    private DateTime _expiry;

    public BaseGateway(string serviceName, ILoggerFactory logger, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger.CreateLogger(serviceName);
        _httpClient = httpClient;
        _telemetry = new TelemetryClient();
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        Task.Run(Authorise).Wait();

    }

    [ExcludeFromCodeCoverage(Justification =
        "Mocked and partially covered by tests, but not fully required method in report for unit tests coverage")]
    public async Task Authorise()
    {
        var url = $"{_httpClient.BaseAddress}oauth2/token";

        try
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var token = session.GetString("JwtToken");
            DateTime.TryParse(session.GetString("JwtTokenExpiry"), out var sessionExpiry);
            // If token exists and has not expired, use it
            if (!string.IsNullOrEmpty(token) && sessionExpiry > DateTime.UtcNow)
            {
                _jwtAuthResponse = new JwtAuthResponse { access_token = token, expires_in = (int)(sessionExpiry - DateTime.UtcNow).TotalSeconds };
                _expiry = sessionExpiry;
            }
            else 
            {
                var establishemnt = (DfeSignInExtensions.GetDfeClaims(_httpContextAccessor.HttpContext.User.Claims)).Organisation;
                string baseScope = _configuration["Api:AuthorisationScope"];
                string userScope = string.Empty;

                switch (establishemnt.Category.Id)
                {
                    case OrganisationCategory.LocalAuthority:
                        userScope = baseScope + $" local_authority:{establishemnt.EstablishmentNumber}";
                        break;
                    case OrganisationCategory.MultiAcademyTrust:
                        userScope = baseScope + $" multi_academy_trust:{establishemnt.Uid}";
                        break;
                    default:
                        userScope = baseScope;
                        break;

                }
                var formData = new SystemUser
                {
                    client_id = _configuration["Api:AuthorisationUsername"],
                    client_secret = _configuration["Api:AuthorisationPassword"],
                    scope = userScope
                };

                _jwtAuthResponse = await ApiDataPostFormDataAsynch(url, formData, new JwtAuthResponse());
                _expiry = DateTime.UtcNow.AddSeconds(_jwtAuthResponse.expires_in);

                // Store token and expiry for the session
                session.SetString("JwtToken", _jwtAuthResponse.access_token);
                session.SetString("JwtTokenExpiry", _expiry.ToString("o"));
            }

            // Ensure we don't add duplicate headers
            if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _jwtAuthResponse.access_token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Post Check failed. uri:-{_httpClient.BaseAddress}{url}");
        }
    }

    protected async Task<T2> ApiDataPostAsynch<T1, T2>(string address, T1 data, T2 result)
    {
        var uri = address;
        var json = JsonConvert.SerializeObject(data);
        HttpContent content = new StringContent(json);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var task = await _httpClient.PostAsync(uri, content);
        if (task.IsSuccessStatusCode)
        {
            var jsonString = await task.Content.ReadAsStringAsync();
            result = JsonConvert.DeserializeObject<T2>(jsonString);
        }
        else
        {
            var method = "POST";

            if (task.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedAccessException();
            await LogApiError(task, method, uri, json);
        }

        return result;
    }

    protected async Task<T2> ApiDataPostFormDataAsynch<T1, T2>(string address, T1 data, T2 result)
    {
        var uri = address;

        // Convert object properties to dictionary
        var properties = data.GetType().GetProperties();
        var formData = new Dictionary<string, string>();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(data)?.ToString();
            if (value != null) formData.Add(prop.Name, value);
        }

        // Create form content from dictionary
        HttpContent content = new FormUrlEncodedContent(formData);

        var task = await _httpClient.PostAsync(uri, content);
        if (task.IsSuccessStatusCode)
        {
            var jsonString = await task.Content.ReadAsStringAsync();
            result = JsonConvert.DeserializeObject<T2>(jsonString);
        }
        else
        {
            var method = "POST";

            if (task.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedAccessException();
            await LogApiError(task, method, uri, string.Join("&", formData.Select(kv => $"{kv.Key}={kv.Value}")));
        }

        return result;
    }

    [ExcludeFromCodeCoverage(Justification = "Method Not Implemented yet accross the solution")]
    protected async Task<T> ApiDataDeleteAsynch<T>(string address, T result)
    {
        var uri = address;
        var task = await _httpClient.DeleteAsync(uri);
        if (task.IsSuccessStatusCode)
        {
            var jsonString = await task.Content.ReadAsStringAsync();
            result = JsonConvert.DeserializeObject<T>(jsonString);
        }
        else
        {
            var method = "DELETE";
            await LogApiError(task, method, uri);
            if (task.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedAccessException();
        }

        return result;
    }

    protected async Task<T> ApiDataGetAsynch<T>(string address, T result)
    {
        var uri = address;

        var task = await _httpClient.GetAsync(uri);

        if (task.IsSuccessStatusCode)
        {
            var jsonString = await task.Content.ReadAsStringAsync();
            result = JsonConvert.DeserializeObject<T>(jsonString);
        }
        else
        {
            var method = "GET";
            try
            {
                await LogApiError(task, method, uri);
            }
            catch (Exception)
            {
                if (task.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedAccessException();
                if (task.StatusCode == HttpStatusCode.NotFound) return default;
            }
        }

        return result;
    }

    [ExcludeFromCodeCoverage(Justification = "Method Not Implemented yet accross the solution")]
    protected async Task<T2> ApiDataPutAsynch<T1, T2>(string address, T1 data, T2 result)
    {
        var uri = address;
        var json = JsonConvert.SerializeObject(data);
        HttpContent content = new StringContent(json);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var task = await _httpClient.PutAsync(uri, content);
        if (task.IsSuccessStatusCode)
        {
            var jsonString = await task.Content.ReadAsStringAsync();
            result = JsonConvert.DeserializeObject<T2>(jsonString);
        }
        else
        {
            var method = "PUT";
            await LogApiError(task, method, uri, json);
        }

        return result;
    }

    [ExcludeFromCodeCoverage(Justification = "Method Not Implemented yet accross the solution")]
    protected async Task<T2> ApiDataPatchAsynch<T1, T2>(string address, T1 data, T2 result)
    {
        var uri = address;
        var json = JsonConvert.SerializeObject(data);
        HttpContent content = new StringContent(json);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var task = await _httpClient.PatchAsync(uri, content);
        if (task.IsSuccessStatusCode)
        {
            var jsonString = await task.Content.ReadAsStringAsync();
            result = JsonConvert.DeserializeObject<T2>(jsonString);
        }
        else
        {
            var method = "PATCH";
            await LogApiError(task, method, uri, json);
        }

        return result;
    }

    internal async Task LogApiError(HttpResponseMessage task, string method, string uri, string data)
    {
        await LogApiErrorInternal(task, method, uri, data);
    }

    internal async Task LogApiError(HttpResponseMessage task, string method, string uri)
    {
        await LogApiErrorInternal(task, method, uri);
    }

    [ExcludeFromCodeCoverage(Justification =
        "Covered by the LogApiError methods marked as internal which are visible to the Tests project")]
    protected virtual async Task LogApiErrorInternal(HttpResponseMessage task, string method, string uri, string data)
    {
        var guid = Guid.NewGuid().ToString();
        if (task?.Content != null)
        {
            var jsonString = await task.Content.ReadAsStringAsync();
            _telemetry.TrackEvent($"Ui_Calling_API {method} failure",
                new Dictionary<string, string>
                {
                    { "LogId", guid },
                    { "Response Code", task.StatusCode.ToString() },
                    { "Address", uri },
                    { "Request Data", data },
                    { "Response Data", jsonString }
                });
        }
        else
        {
            _telemetry.TrackEvent($"Ui_Calling_API Failure:-{method}",
                new Dictionary<string, string> { { "LogId", guid }, { "Address", uri } });
        }

        throw new Exception(
            $"Ui_Calling_API Failure:-{method} , your issue has been logged please use the following reference:- {guid}");
    }

    [ExcludeFromCodeCoverage(Justification =
        "Covered by the LogApiError methods marked as internal which are visible to the Tests project")]
    protected virtual async Task LogApiErrorInternal(HttpResponseMessage task, string method, string uri)
    {
        var guid = Guid.NewGuid().ToString();
        if (task.Content != null)
        {
            var jsonString = await task.Content.ReadAsStringAsync();
            _telemetry.TrackEvent($"Ui_Calling_API {method} failure",
                new Dictionary<string, string>
                {
                    { "LogId", guid },
                    { "Address", uri },
                    { "Response Code", task.StatusCode.ToString() },
                    { "Data", jsonString }
                });
        }
        else
        {
            _telemetry.TrackEvent($"Ui_Calling_API {method} failure",
                new Dictionary<string, string> { { "LogId", guid }, { "Address", uri } });
        }

        throw new Exception(
            $"Ui_Calling_API Failure:-{method} , your issue has been logged please use the following reference:- {guid}");
    }
}