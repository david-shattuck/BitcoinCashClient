using BitcoinCash.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinCash.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FiatController : ControllerBase
    {        
        private readonly IFiatService _fiatService;

        public FiatController(IFiatService fiatService)
        {
            _fiatService = fiatService;
        }

        [HttpGet]
        [Route("[action]")]
        public decimal GetValue([FromQuery] string currency) => _fiatService.GetValue(currency);
    }
}