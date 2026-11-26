using CheckYourEligibility.FrontEnd.Boundary.Responses;
using CheckYourEligibility.FrontEnd.Boundary.Shared;
using CheckYourEligibility.FrontEnd.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CheckYourEligibility.FrontEnd.Tests.Views;

[TestFixture]
public class ApplicationSentViewTests
{
    [TestCase(0, TestName = "ApplicationSent_ShowsEvidence_WhenOnlyFirstChildHasEvidence")]
    [TestCase(1, TestName = "ApplicationSent_ShowsEvidence_WhenOnlySecondChildHasEvidence")]
    public async Task ApplicationSent_ShowsEvidence_WhenOnlyOneChildHasEvidence(int childWithEvidence)
    {
        const string fileName = "test-evidence.png";
        var responses = new[]
        {
            CreateResponse("First", "11111111"),
            CreateResponse("Second", "22222222")
        };
        responses[childWithEvidence].Data.Evidence = [new ApplicationEvidence { FileName = fileName }];

        var html = await RenderConfirmation(responses);

        Assert.Multiple(() =>
        {
            Assert.That(html, Does.Contain("<h1 class=\"govuk-panel__title\">Application and evidence sent</h1>"));
            Assert.That(html, Does.Contain("<title>Application and evidence sent - "));
            Assert.That(html, Does.Contain("We have sent the following evidence:"));
            Assert.That(html, Does.Contain($">{fileName}</td>"));
            Assert.That(html, Does.Contain(">First Child</th>"));
            Assert.That(html, Does.Contain(">Second Child</th>"));
        });
    }

    private static async Task<string> RenderConfirmation(ApplicationSaveItemResponse[] responses)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddControllersWithViews().AddApplicationPart(typeof(CheckController).Assembly);
        await using var app = builder.Build();
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var httpContext = new DefaultHttpContext { RequestServices = services };
        httpContext.SetEndpoint(new Endpoint(null, EndpointMetadataCollection.Empty, "Confirmation"));
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), actionContext.ModelState);
        var tempData = services.GetRequiredService<ITempDataDictionaryFactory>().GetTempData(httpContext);
        tempData["FsmApplicationResponses"] = JsonConvert.SerializeObject(responses);

        var viewEngine = services.GetRequiredService<ICompositeViewEngine>();
        var view = viewEngine.GetView(null, "/Views/Check/Application_Sent.cshtml", isMainPage: true).View!;
        using var writer = new StringWriter();
        await view.RenderAsync(new ViewContext(actionContext, view, viewData, tempData, writer, new HtmlHelperOptions()));
        return writer.ToString();
    }

    private static ApplicationSaveItemResponse CreateResponse(string childName, string reference) => new()
    {
        Data = new ApplicationResponse
        {
            ChildFirstName = childName,
            ChildLastName = "Child",
            Establishment = new ApplicationResponse.ApplicationEstablishment { Name = "Test School" },
            Reference = reference,
            Created = new DateTime(2026, 11, 26, 12, 0, 0, DateTimeKind.Utc)
        }
    };
}
