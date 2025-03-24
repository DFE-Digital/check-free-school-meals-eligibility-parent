using CheckYourEligibility.Admin.Domain.DfeSignIn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheckYourEligibility.Admin.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        

        protected DfeClaims? _Claims;

    }
}
