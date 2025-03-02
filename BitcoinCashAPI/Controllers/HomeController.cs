using Microsoft.AspNetCore.Mvc;

namespace BitcoinCash.API.Controllers
{
    public class HomeController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        public RedirectResult RedirectToSwagger() => Redirect("/swagger");
    }
}